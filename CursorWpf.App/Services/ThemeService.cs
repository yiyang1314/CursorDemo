using System;
using System.Windows;
namespace CursorWpf.Services
{
 public static class ThemeService
 {
  public static void Apply(string theme)
  {
   var uri = new Uri(theme == "Light" ? "Themes/Light.xaml" : "Themes/Dark.xaml", UriKind.Relative);
   var dictionary = new ResourceDictionary { Source = uri };
   var merged = Application.Current.Resources.MergedDictionaries;
   merged.Clear(); merged.Add(dictionary);
  }
 }
}
