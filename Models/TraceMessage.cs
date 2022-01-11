using System;
using Newtonsoft.Json;

namespace API.Models {
    public class TraceMessage {
        public TraceMessage(string? Action, string? Name, int? Id, object? Value) {
            this.Action = Action;
            this.Name = Name;
            this.Id = Id;
            this.Value = Value;
        }
        public string? Action { get; set; }
        public string? Name { get; set; }
        public int? Id { get; set; }
        public object? Value { get; set; }
    }
}

