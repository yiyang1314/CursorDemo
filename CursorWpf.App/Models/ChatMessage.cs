using System;
namespace CursorWpf.Models
{
    public class ChatMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
        public bool IsUser { get { return Role == "user"; } }
    }
}
