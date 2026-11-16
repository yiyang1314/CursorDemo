using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AyuUI.Controll.Toasts
{
    public class ToastModel : INotifyPropertyChanged
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public ToastType Type { get; set; }
        public string IconData { get; set; } = string.Empty;
        public int DurationSeconds { get; set; }
        public Action? OnClick { get; set; }

        private Visibility _badgeVisibility = Visibility.Collapsed;
        public Visibility BadgeVisibility
        {
            get => _badgeVisibility;
            set { _badgeVisibility = value; OnPropertyChanged(nameof(BadgeVisibility)); }
        }

        private string _totalCountText = string.Empty;
        public string TotalCountText
        {
            get => _totalCountText;
            set { _totalCountText = value; OnPropertyChanged(nameof(TotalCountText)); }
        }

        public DispatcherTimer? Timer { get; set; }
        public bool IsPaused { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
