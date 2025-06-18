using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.ConsoleClient.Models
{
    public class ChatMessage
    {
        public string? UserName { get; set; }
        public string? Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
