using System.Collections.ObjectModel;
using System.IO;
namespace CursorWpf.Services
{
    public class ProjectService
    {
        public ObservableCollection<string> GetTopLevelItems(string path)
        {
            var items = new ObservableCollection<string>();
            if (!Directory.Exists(path)) return items;
            foreach (var dir in Directory.GetDirectories(path)) items.Add("📁 " + Path.GetFileName(dir));
            foreach (var file in Directory.GetFiles(path)) items.Add("▤ " + Path.GetFileName(file));
            return items;
        }
    }
}
