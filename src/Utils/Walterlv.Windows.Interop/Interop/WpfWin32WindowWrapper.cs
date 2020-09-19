using System;
using System.Windows;

using Lsj.Util.Win32;
using Lsj.Util.Win32.BaseTypes;
using Lsj.Util.Win32.Enums;

namespace Walterlv.Windows.Interop
{
    /// <summary>
    /// 为 Win32 窗口句柄提供 WPF <see cref="Window"/> 风格的 API 访问。
    /// </summary>
    public class WpfWin32WindowWrapper : DependencyObject
    {
        /// <summary>
        /// 根据窗口句柄 <paramref name="handle"/> 创建一个 WPF <see cref="Window"/> 风格的 API 访问。
        /// </summary>
        /// <param name="handle">窗口句柄。</param>
        public WpfWin32WindowWrapper(IntPtr handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// 获取此 <see cref="WpfWin32WindowWrapper"/> 包装的 Win32 窗口句柄。
        /// </summary>
        public IntPtr Handle { get; }

        public static readonly DependencyProperty WindowStateProperty = DependencyProperty.Register(
            nameof(WindowState), typeof(WindowState), typeof(WpfWin32WindowWrapper),
            new PropertyMetadata(WindowState.Normal, (d, e) =>
            {
                if (d is WpfWin32WindowWrapper wrapper && wrapper.Handle != IntPtr.Zero)
                {
                    wrapper.OnWindowStateChanged((WindowState)e.NewValue);
                }
            }));

        public WindowState WindowState
        {
            get => (WindowState)GetValue(WindowStateProperty);
            set => SetValue(WindowStateProperty, value);
        }

        private void OnWindowStateChanged(WindowState newValue)
        {
            _ = newValue switch
            {
                WindowState.Maximized => User32.ShowWindow(Handle, ShowWindowCommands.SW_MAXIMIZE),
                WindowState.Normal => User32.ShowWindow(Handle, ShowWindowCommands.SW_SHOW),
                WindowState.Minimized => User32.ShowWindow(Handle, ShowWindowCommands.SW_MINIMIZE),
                _ => (BOOL)false
            };
        }
    }
}
