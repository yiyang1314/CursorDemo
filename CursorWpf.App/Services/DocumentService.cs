using System.IO;
namespace CursorWpf.Services
{
    public class DocumentService
    {
        public string Read(string path) { return File.ReadAllText(path); }
        public void Save(string path, string content) { File.WriteAllText(path, content); }
    }
}
