using System.Reflection;
using API.Classes;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.OData;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// CORS support
builder.Services.AddCors(options => {
    options.AddPolicy("AllOrigins",
         builder => {
             builder
         .AllowAnyOrigin()
         .AllowAnyMethod()
         .AllowAnyHeader();
         });
});
builder.Services.AddSingleton<ApplicationSettings>();
builder.Services.AddSingleton<ContactService>();
var applicationSettings = new ApplicationSettings();
// Example showing support for multiple messaging platforms
switch(applicationSettings.QueueType) {
    case "AzureServiceBus":
        builder.Services.AddSingleton<IMessageService,MessageServiceAzureServiceBus>();
        break;
    case "Dapr":
        builder.Services.AddSingleton<IMessageService,MessageServiceDapr>();
        break;
    default:
        builder.Services.AddSingleton<IMessageService,MessageServiceRabbitMQ>();
        break;
}
builder.Services.AddControllers().AddOData(options => options.Count().Expand().Filter().OrderBy().Select().SetMaxTop(1000));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    if(basePath != null) {
        options.IncludeXmlComments(Path.Combine(basePath,"mongodb-api.xml"));
    }
    //Configure Swagger to filter out $expand objects to improve performance for large highly relational APIs
    options.SchemaFilter<SwaggerIgnoreFilter>();
    options.OperationFilter<ODataEnableQueryFiler>();
});
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {}
app.UseODataQueryRequest();
app.UseODataBatching();
app.UseSwagger();
app.UseSwaggerUI(options => {
    options.DefaultModelExpandDepth(2);
    options.DefaultModelsExpandDepth(-1);
    options.DefaultModelRendering(ModelRendering.Model);
    options.DisplayRequestDuration();
});
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

/*
 * 
 * lsof -i:7163
 * kill -p <PID>
 */