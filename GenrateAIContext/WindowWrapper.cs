using System;
using System.Windows.Forms;

namespace GenrateAIContext
{
    internal class WindowWrapper : IWin32Window
    {
        private readonly IntPtr _hwnd;
        public WindowWrapper(IntPtr hwnd) => _hwnd = hwnd;
        public IntPtr Handle => _hwnd;
    }
}
