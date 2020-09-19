using System;

namespace Walterlv.Windows.Interop
{
    /// <summary>
    /// 包含 Win32 窗口信息的事件参数。
    /// </summary>
    public class Win32WindowEventArgs : EventArgs
    {
        /// <summary>
        /// 创建一个包含指定窗口 <paramref name="hWnd"/> 信息的 Win32 窗口事件参数。
        /// </summary>
        /// <param name="hWnd">窗口句柄。</param>
        public Win32WindowEventArgs(IntPtr hWnd)
        {
            Handle = hWnd;
        }

        /// <summary>
        /// 窗口句柄。
        /// </summary>
        public IntPtr Handle { get; set; }
    }
}
