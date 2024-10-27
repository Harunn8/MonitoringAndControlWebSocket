using System.Collections.Generic;

namespace Domain.Models
{
    public class WebSocketCommand
    {
        public string Action { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }
}
