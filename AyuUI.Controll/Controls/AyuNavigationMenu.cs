using AyuUI.Controll.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AyuUI.Controll.Controls
{
    public enum NavigationPosition
    {
        Left,   // 侧边栏导航（选中指示条在右侧）
        Top,    // 顶部导航（选中指示条在底部）
        Bottom  // 底部导航（选中指示条在顶部）
    }

    public class NavigationItem : ObservableObject
    {
        private string _id = string.Empty;
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _iconData = string.Empty;
        public string IconData
        {
            get => _iconData;
            set => SetProperty(ref _iconData, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public ICommand? Command { get; set; }
    }
}
