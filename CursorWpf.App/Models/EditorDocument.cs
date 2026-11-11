namespace CursorWpf.Models
{
    public class EditorDocument
    {
        public string Title { get; set; }
        public string FullPath { get; set; }
        public string Content { get; set; }
        public bool IsDirty { get; set; }
    }
}
