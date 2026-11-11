using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
namespace CursorWpf
{
 public partial class MainWindow : Window
 {
  public MainWindow(){InitializeComponent();DataContext=new ViewModels.MainViewModel();}
  private void TitleBar_MouseLeftButtonDown(object sender,MouseButtonEventArgs e){if(e.ChangedButton==MouseButton.Left){if(e.ClickCount==2)ToggleMaximize();else if(e.ButtonState==MouseButtonState.Pressed)DragMove();}}
  private void ToggleMaximize(){WindowState=WindowState==WindowState.Maximized?WindowState.Normal:WindowState.Maximized;}
  private void Minimize_Click(object s,RoutedEventArgs e){WindowState=WindowState.Minimized;}
  private void Maximize_Click(object s,RoutedEventArgs e){ToggleMaximize();}
  private void Close_Click(object s,RoutedEventArgs e){Close();}
  private ViewModels.MainViewModel Vm=>DataContext as ViewModels.MainViewModel;
  private void FileMenu_Click(object s,RoutedEventArgs e){var m=CreateMenu();Add(m,"打开文件...",()=>Vm.OpenFileCommand.Execute(null));Add(m,"打开文件夹...",()=>Vm.OpenFolderCommand.Execute(null));Add(m,"保存",()=>Vm.SaveCommand.Execute(null));m.Items.Add(new Separator());Add(m,"退出",Close);ShowMenu((Button)s,m);}
  private void EditMenu_Click(object s,RoutedEventArgs e){var m=CreateMenu();Add(m,"撤销",()=>ApplicationCommands.Undo.Execute(null,Keyboard.FocusedElement as IInputElement));Add(m,"重做",()=>ApplicationCommands.Redo.Execute(null,Keyboard.FocusedElement as IInputElement));Add(m,"剪切",()=>ApplicationCommands.Cut.Execute(null,Keyboard.FocusedElement as IInputElement));Add(m,"复制",()=>ApplicationCommands.Copy.Execute(null,Keyboard.FocusedElement as IInputElement));Add(m,"粘贴",()=>ApplicationCommands.Paste.Execute(null,Keyboard.FocusedElement as IInputElement));ShowMenu((Button)s,m);}
  private void ViewMenu_Click(object s,RoutedEventArgs e){var m=CreateMenu();Add(m,"深色主题",()=>Vm.DarkCommand.Execute(null));Add(m,"浅色主题",()=>Vm.LightCommand.Execute(null));Add(m,"中文",()=>Vm.ChineseCommand.Execute(null));Add(m,"English",()=>Vm.EnglishCommand.Execute(null));ShowMenu((Button)s,m);}
  private void HelpMenu_Click(object s,RoutedEventArgs e){var m=CreateMenu();Add(m,"打开帮助文档",()=>Vm.HelpCommand.Execute(null));Add(m,"关于 CursorWpf",()=>MessageBox.Show("CursorWpf 1.1.0\nWPF · .NET Framework 4.8","关于",MessageBoxButton.OK,MessageBoxImage.Information));ShowMenu((Button)s,m);}
  private ContextMenu CreateMenu(){return new ContextMenu{Placement=System.Windows.Controls.Primitives.PlacementMode.Bottom,Background=(System.Windows.Media.Brush)FindResource("PanelBrush"),Foreground=(System.Windows.Media.Brush)FindResource("TextBrush"),BorderBrush=(System.Windows.Media.Brush)FindResource("BorderBrush"),Padding=new Thickness(5)};}
  private void Add(ContextMenu m,string title,Action action){var item=new MenuItem{Header=title,Padding=new Thickness(18,7,26,7)};item.Click+=(s,e)=>action();m.Items.Add(item);}
  private void ShowMenu(Button b,ContextMenu m){b.ContextMenu=m;m.PlacementTarget=b;m.IsOpen=true;}
  private void UserProfile_Click(object s,RoutedEventArgs e){var w=new Window{Title="用户信息",Owner=this,WindowStartupLocation=WindowStartupLocation.CenterOwner,Width=320,Height=245,ResizeMode=ResizeMode.NoResize,WindowStyle=WindowStyle.ToolWindow,Background=(System.Windows.Media.Brush)FindResource("PanelBrush"),Foreground=(System.Windows.Media.Brush)FindResource("TextBrush")};var p=new StackPanel{Margin=new Thickness(22)};p.Children.Add(new TextBlock{Text="本地用户",FontSize=20,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,8)});p.Children.Add(new TextBlock{Text="账户类型：本地免费账户",Margin=new Thickness(0,4,0,4)});p.Children.Add(new TextBlock{Text="工作区：CursorWpf",Margin=new Thickness(0,4,0,4)});p.Children.Add(new TextBlock{Text="版本：1.1.0",Margin=new Thickness(0,4,0,16)});var b=new Button{Content="关闭",Width=82,Padding=new Thickness(8),HorizontalAlignment=HorizontalAlignment.Right};b.Click+=(a,x)=>w.Close();p.Children.Add(b);w.Content=p;w.ShowDialog();}
  private void ImagePreview_Click(object s,RoutedEventArgs e){var d=new OpenFileDialog{Filter="图片文件|*.png;*.jpg;*.jpeg;*.bmp;*.gif|所有文件|*.*"};if(d.ShowDialog()==true){var w=new Window{Title=System.IO.Path.GetFileName(d.FileName),Owner=this,Width=900,Height=650,WindowStartupLocation=WindowStartupLocation.CenterOwner,Background=(System.Windows.Media.Brush)FindResource("EditorBrush")};var img=new System.Windows.Controls.Image{Source=new System.Windows.Media.Imaging.BitmapImage(new Uri(d.FileName)),Stretch=System.Windows.Media.Stretch.Uniform,Margin=new Thickness(12)};w.Content=new ScrollViewer{Content=img};w.ShowDialog();}}
 }
}