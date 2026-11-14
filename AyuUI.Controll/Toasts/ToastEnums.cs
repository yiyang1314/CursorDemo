using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AyuUI.Controll.Toasts
{
    public enum ToastPosition
    {
        TopRight,
        BottomRight,
        TopLeft,
        BottomLeft,
        TopCenter,
        BottomCenter
    }

    public enum ToastType
    {
        Success,
        Info,
        Warning,
        Error,
        Loading
    }
}
