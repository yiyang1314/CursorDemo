using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using CursorWpf.Models;
using CursorWpf.Services;
namespace CursorWpf.ViewModels
{
 public class MainViewModel:ViewModelBase
 {
  private string _inputText,_editorText="// CursorWpf 1.1.0\r\n// Welcome to your local coding workspace.\r\n\r\nnamespace CursorWpf\r\n{\r\n    public class Welcome\r\n    {\r\n        public string Message => \"Hello, CursorWpf!\";\r\n    }\r\n}\r\n";
  private string _projectName="CursorWpf",_currentFile="Welcome.cs",_selectedProjectItem;
  private string _openedPath;
  public ObservableCollection<ChatMessage> Messages{get;}=new ObservableCollection<ChatMessage>();
  public ObservableCollection<string> ProjectItems{get;}=new ObservableCollection<string>();
  public string InputText{get=>_inputText;set=>Set(ref _inputText,value);}
  public string EditorText{get=>_editorText;set=>Set(ref _editorText,value);}
  public string ProjectName{get=>_projectName;set=>Set(ref _projectName,value);}
  public string CurrentFile{get=>_currentFile;set=>Set(ref _currentFile,value);}
  public string SelectedProjectItem{get=>_selectedProjectItem;set=>Set(ref _selectedProjectItem,value);}
  public ICommand SendCommand{get;} public ICommand OpenFileCommand{get;} public ICommand OpenFolderCommand{get;} public ICommand SaveCommand{get;}
  public ICommand DarkCommand{get;} public ICommand LightCommand{get;} public ICommand ChineseCommand{get;} public ICommand EnglishCommand{get;} public ICommand HelpCommand{get;}
  public MainViewModel(){SendCommand=new RelayCommand(Send);OpenFileCommand=new RelayCommand(OpenFile);OpenFolderCommand=new RelayCommand(OpenFolder);SaveCommand=new RelayCommand(Save);DarkCommand=new RelayCommand(()=>ThemeService.Apply("Dark"));LightCommand=new RelayCommand(()=>ThemeService.Apply("Light"));ChineseCommand=new RelayCommand(()=>LocalizationService.SetLanguage("zh-CN"));EnglishCommand=new RelayCommand(()=>LocalizationService.SetLanguage("en-US"));HelpCommand=new RelayCommand(OpenHelp);
   Messages.Add(new ChatMessage{Role="assistant",Content="你好！欢迎使用 CursorWpf。这里是本地 AI 工作区演示。你可以打开项目、编辑代码、预览图片与文档，并切换主题和语言。当前 AI 回复为本地模拟，不会向外部服务发送数据。"});
   ProjectItems.Add("▸  CursorWpf.App");ProjectItems.Add("▸  Docs");ProjectItems.Add("▸  Build");ProjectItems.Add("▣  .github");ProjectItems.Add("◇  CursorWpf.sln");ProjectItems.Add("▤  README.md");
  }
  private void Send(){if(string.IsNullOrWhiteSpace(InputText))return;Messages.Add(new ChatMessage{Role="user",Content=InputText.Trim()});Messages.Add(new ChatMessage{Role="assistant",Content="已收到消息。当前为离线 UI 演示版本；尚未配置 AI API，因此不会伪造模型响应。后续可接入兼容 OpenAI 的流式对话服务。"});InputText=string.Empty;}
  private void OpenFile(){var d=new OpenFileDialog{Filter="代码与文档|*.cs;*.xaml;*.json;*.xml;*.txt;*.md;*.py;*.js;*.ts;*.html;*.css|所有文件|*.*"};if(d.ShowDialog()==true){try{_openedPath=d.FileName;CurrentFile=Path.GetFileName(_openedPath);EditorText=File.ReadAllText(_openedPath);}catch(Exception ex){MessageBox.Show("打开文件失败："+ex.Message,"CursorWpf",MessageBoxButton.OK,MessageBoxImage.Error);}}}
  private void Save(){try{if(string.IsNullOrEmpty(_openedPath)){var d=new SaveFileDialog{FileName=CurrentFile,Filter="所有文件|*.*"};if(d.ShowDialog()!=true)return;_openedPath=d.FileName;}File.WriteAllText(_openedPath,EditorText);MessageBox.Show("文件已保存。","CursorWpf",MessageBoxButton.OK,MessageBoxImage.Information);}catch(Exception ex){MessageBox.Show("保存失败："+ex.Message,"CursorWpf",MessageBoxButton.OK,MessageBoxImage.Error);}}
  private void OpenFolder(){var d=new OpenFileDialog{CheckFileExists=false,FileName="选择此文件夹"};if(d.ShowDialog()==true){var path=Path.GetDirectoryName(d.FileName);if(string.IsNullOrEmpty(path))return;ProjectName=Path.GetFileName(path);ProjectItems.Clear();foreach(var item in new ProjectService().GetTopLevelItems(path))ProjectItems.Add(item);}}
  private void OpenHelp(){var candidates=new[]{Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Docs","功能使用手册.md"),Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"..\\..\\..\\Docs\\功能使用手册.md"))};foreach(var p in candidates)if(File.Exists(p)){System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("notepad.exe", "\""+p+"\""){UseShellExecute=true});return;}MessageBox.Show("帮助文档位于 Docs 文件夹。","帮助");}
 }
 public class RelayCommand:ICommand{private readonly Action _execute;public RelayCommand(Action execute){_execute=execute;}public bool CanExecute(object p)=>true;public void Execute(object p)=>_execute();public event EventHandler CanExecuteChanged{add{CommandManager.RequerySuggested+=value;}remove{CommandManager.RequerySuggested-=value;}}}
}