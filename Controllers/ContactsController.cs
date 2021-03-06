using System.Diagnostics;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Newtonsoft.Json;

#pragma warning disable S125 // Sections of code should not be commented out
/*   
*  Example on how to get a string[] of roles from the user's token 
*      var roles = User.Claims.Where(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).FirstOrDefault().Value.Split(',');
*/

namespace API.Controllers {
    /// <summary>
    /// Represents a RESTful service for contacts
    /// IMPORTANT - [Produces("application/json")] is required on every HTTP action or Swagger will not show what object model will be returned
    /// </summary>
    public class ContactsController: Controller {

        private readonly ContactService _contactService;
        private readonly IMessageService _messageService;

        public ContactsController(IMessageService messageService, ContactService contactService) {
            _contactService = contactService;
            _messageService = messageService;
        }

        #region CRUD Operations

        /// <summary>Query contacts</summary>
        /// <returns>A list of contacts</returns>
        /// <response code="200">The contacts were successfully retrieved</response>
        [HttpGet]
        [Route("contacts")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<Contact>),200)] // Ok
        //[Authorize]
        [EnableQuery]
        public IActionResult Get() {
            try {
                /*
                Working = $count, $filter, $orderBy, $skip, $top
                Not working = $select, $expand ($select does work for GetById)
                Mongo Team working on fix for $select and $expand
                   https://jira.mongodb.org/browse/CSHARP-1423
                   https://jira.mongodb.org/browse/CSHARP-1771
                   in meantime, remove them from query, then apply, then apply second LINQ re-applying select
                */
                return Ok(_contactService.Get());
            } catch(Exception ex) {
                Activity.Current?.AddTag("exception",ex);
                return StatusCode(500,ex.Message);
            }
        }

        /// <summary>Query contact by id</summary>
        /// <param name="id">The contact id</param>
        /// <returns>A single contact</returns>
        /// <response code="200">The contact was successfully retrieved</response>
        /// <response code="404">The contact was not found</response>
        [HttpGet]
        [Route("contacts/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Contact),200)] // Ok
        [ProducesResponseType(typeof(void),404)] // Not Found
        [EnableQuery]
        public async Task<IActionResult> GetById([FromRoute] string id) {
            try {
                // Working = $select
                // Not working = $expand
                // Not needed = $count, $filter, $orderBy, $skip, $top
                //OData will handle returning 404 Not Found if IQueriable returns no result
                return Ok(await _contactService.Get(id));
            } catch(Exception ex) {
                Activity.Current?.AddTag("exception",ex);
                return StatusCode(500,ex.Message);
            }
        }

        /// <summary>Create a new contact</summary>
        /// <param name="contact">A full contact object</param>
        /// <returns>A new contact or list of products</returns>
        /// <response code="201">The contact was successfully created</response>
        /// <response code="400">The contact is invalid</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Access denied due to inadaquate claim roles</response>
        [HttpPost]
        [Route("contacts")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<Contact>),201)] // Created
        [ProducesResponseType(typeof(string),400)] // Bad Request (should be ModelStateDictionary)
        [ProducesResponseType(typeof(void),401)] // Unauthorized
        public async Task<IActionResult> Post([FromBody] Contact contact) {
            try {
                var message = JsonConvert.SerializeObject(new TraceMessage("POST","Contact",null,contact));
                // Wrap the message send (first) and the DB change (second) in a message transaction
                _messageService.BeginTransaction();
                _messageService.Send(message);
                var newContact = await _contactService.Create(contact);
                _messageService.CommitTransaction();
                return Created("",contact);
            } catch(Exception ex) {
                // Rollback the event message send since an error has occured with updating the database
                _messageService.RollbackTransaction();
                Activity.Current?.AddTag("exception",ex);
                return StatusCode(500,ex.Message);
            }
        }

        /// <summary>Edit contact</summary>
        /// <param name="id">The contact id</param>
        /// <param name="contactDelta">A partial JSON representation of the contact including only the properites to change.</param>
        /// <returns>An updated contact</returns>
        /// <response code="204">The contact was successfully updated</response>
        /// <response code="400">The contact is invalid</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Access denied due to inadaquate claim roles</response>
        /// <response code="404">The contact was not found</response>
        [HttpPatch]
        [Route("contacts/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(void),204)] // No Content 
        [ProducesResponseType(typeof(string),400)] // Bad Request (should be ModelStateDictionary)
        [ProducesResponseType(typeof(void),401)] // Unauthorized - Product not authenticated
        [ProducesResponseType(typeof(ForbiddenException),403)] // Forbidden - Product does not have required claim roles
        [ProducesResponseType(typeof(void),404)] // Not Found
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + ",BasicAuthentication", Roles = "Admin")]
        public async Task<IActionResult> Patch([FromRoute] string id,[FromBody] dynamic contactDelta) {
            // [FromBody] types other than dynamic will be null as they will be missing properties and not match the static type definition
            try {
                var foundContact = await _contactService.Get(id);
                if(foundContact == null) {
                    return NotFound();
                }
                // The dynamic [FromBody] is different from the dynamic generated when deserializing a JSON string
                contactDelta = JsonConvert.DeserializeObject<dynamic>(Convert.ToString(contactDelta));
                API.Classes.Delta.Patch<Contact>(contactDelta,foundContact); // OData Delta<T>.Patch() not working with .Net 6 as of OData 8.0.6 so we wrote our own
                // Wrap the message send (first) and the DB change (second) in a message transaction
                _messageService.BeginTransaction();
                var message = JsonConvert.SerializeObject(new TraceMessage("PATCH","Contact",id,foundContact));
                _messageService.Send(message);
                await _contactService.Update(id,foundContact);
                _messageService.CommitTransaction();
                Activity.Current?.AddTag("value",foundContact);
                return NoContent();
            } catch(Exception ex) {
                Activity.Current?.AddTag("exception",ex);
                // Rollback the event message send since an error has occured with updating the database
                _messageService.RollbackTransaction();
                return StatusCode(500,ex.Message);
            }
        }

        /// <summary>Edit contact</summary>
        /// <param name="id">The contact id</param>
        /// <param name="contact">A updated contact object.</param>
        /// <returns>An updated contact</returns>
        /// <response code="204">The contact was successfully updated</response>
        /// <response code="400">The contact is invalid</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Access denied due to inadaquate claim roles</response>
        /// <response code="404">The contact was not found</response>
        [HttpPut]
        [Route("contacts/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(void),204)] // No Content
        [ProducesResponseType(typeof(string),400)] // Bad Request (should be ModelStateDictionary)
        [ProducesResponseType(typeof(void),401)] // Unauthorized - Product not authenticated
        [ProducesResponseType(typeof(ForbiddenException),403)] // Forbidden - Product does not have required claim roles
        [ProducesResponseType(typeof(void),404)] // Not Found
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + ",BasicAuthentication", Roles = "Admin")]
        public async Task<IActionResult> Put([FromRoute] string id,[FromBody] Contact contact) {
            try {
                var foundContact = await _contactService.Get(id);
                if(foundContact == null) {
                    return NotFound();
                }
                contact.Id = id;
                var message = JsonConvert.SerializeObject(new TraceMessage("PUT","Contact",id,contact));
                // Wrap the message send (first) and the DB change (second) in a message transaction
                _messageService.BeginTransaction();
                _messageService.Send(message);
                await _contactService.Update(id,contact);
                _messageService.CommitTransaction();
                Activity.Current?.AddTag("value",contact);
                return NoContent();
            } catch(Exception ex) {
                // Rollback the event message send since an error has occured with updating the database
                _messageService.RollbackTransaction();
                Activity.Current?.AddTag("exception",ex);
                return StatusCode(500,ex.Message);
            }
       }

        /// <summary>Delete the given contact</summary>
        /// <param name="id">The contact id</param>
        /// <response code="204">The contact was successfully deleted</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Access denied due to inadaquate claim roles</response>
        /// <response code="404">The product was not found</response>
        [HttpDelete]
        [Route("contacts/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(void),204)] // No Content
        [ProducesResponseType(typeof(void),401)] // Unauthorized
        [ProducesResponseType(typeof(void),404)] // Not Found
        public async Task<IActionResult> Delete([FromRoute] string id) {
            try {
                var contact = await _contactService.Get(id);
                if(contact == null) {
                    return NotFound();
                }
                var message = JsonConvert.SerializeObject(new TraceMessage("DELETE","Contact",id,null));
                // Wrap the message send (first) and the DB change (second) in a message transaction
                _messageService.BeginTransaction();
                _messageService.Send(message);
                await _contactService.Remove(id);
                _messageService.CommitTransaction();
                return NoContent();
            } catch(Exception ex) {
                // Rollback the event message send since an error has occured with updating the database
                _messageService.RollbackTransaction();
                Activity.Current?.AddTag("Exception",ex);
                return StatusCode(500,ex.Message);
            }
        }

        #endregion
    }
}