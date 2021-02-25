#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;
using Walterlv.Windows.Core;

namespace Walterlv.Windows.Controls
{
    /// <summary>
    /// 如果你使用 <see cref="WindowChrome"/> 将界面扩展到非客户区，那么你可以在 <see cref="Window"/> 的模板中加入此容器，以便让内部的内容自动填充客户区部分。
    /// 使用此容器可以免去在 <see cref="Window"/> 的样式里面通过 Setter/Trigger 在窗口状态改变时做的各种边距适配。
    /// </summary>
    public class ClientAreaBorder : Border
    {
#pragma warning disable IDE1006 // 命名样式
#pragma warning disable IDE0052 // 删除未读的私有成员
        private const int SM_CXFRAME = 32;
        private const int SM_CYFRAME = 33;
        private const int SM_CXPADDEDBORDER = 92;
#pragma warning restore IDE0052 // 删除未读的私有成员
#pragma warning restore IDE1006 // 命名样式

        [DllImport("user32", ExactSpelling = true)]
        private static extern int GetSystemMetrics(int nIndex);

        private Window? _oldWindow;
        private static Thickness? _paddedBorderThickness;
        private static Thickness? _resizeFrameBorderThickness;
        private static Thickness? _windowChromeNonClientFrameThickness;

        /// <inheritdoc />
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            if (_oldWindow is { } oldWindow)
            {
                oldWindow.StateChanged -= Window_StateChanged;
            }

            var newWindow = (Window?)Window.GetWindow(this);
            if (newWindow is not null)
            {
                newWindow.StateChanged -= Window_StateChanged;
                newWindow.StateChanged += Window_StateChanged;
            }

            _oldWindow = newWindow;
        }

        private void Window_StateChanged(object? sender, EventArgs e)
        {
            var window = (Window)sender!;
            Padding = window.WindowState switch
            {
                WindowState.Maximized => WindowChromeNonClientFrameThickness,
                _ => default,
            };
        }

        /// <summary>
        /// 获取系统的 <see cref="SM_CXPADDEDBORDER"/> 作为 WPF 单位的边框数值。
        /// </summary>
        public Thickness PaddedBorderThickness
        {
            get
            {
                if (_paddedBorderThickness is null)
                {
                    var paddedBorder = GetSystemMetrics(SM_CXPADDEDBORDER);
                    var dpi = GetDpi();
                    var frameSize = new Size(paddedBorder, paddedBorder);
                    var frameSizeInDips = new Size(frameSize.Width / dpi.FactorX, frameSize.Height / dpi.FactorY);
                    _paddedBorderThickness = new Thickness(frameSizeInDips.Width, frameSizeInDips.Height, frameSizeInDips.Width, frameSizeInDips.Height);
                }

                return _paddedBorderThickness.Value;
            }
        }

        /// <summary>
        /// 获取系统的 <see cref="SM_CXFRAME"/> 和 <see cref="SM_CYFRAME"/> 作为 WPF 单位的边框数值。
        /// </summary>
        public Thickness ResizeFrameBorderThickness => _resizeFrameBorderThickness ??= new Thickness(
            SystemParameters.ResizeFrameVerticalBorderWidth,
            SystemParameters.ResizeFrameHorizontalBorderHeight,
            SystemParameters.ResizeFrameVerticalBorderWidth,
            SystemParameters.ResizeFrameHorizontalBorderHeight);

        /// <summary>
        /// 如果使用了 <see cref="WindowChrome"/> 来制作窗口样式以将窗口客户区覆盖到非客户区，那么就需要自己来处理窗口最大化后非客户区的边缘被裁切的问题。
        /// 使用此属性获取窗口最大化时窗口样式应该内缩的边距数值，这样在窗口最大化时客户区便可以在任何 DPI 下不差任何一个像素地完全覆盖屏幕工作区。
        /// <see cref="GetSystemMetrics"/> 方法无法直接获得这个数值。
        /// </summary>
        public Thickness WindowChromeNonClientFrameThickness => _windowChromeNonClientFrameThickness ??= new Thickness(
            ResizeFrameBorderThickness.Left + PaddedBorderThickness.Left,
            ResizeFrameBorderThickness.Top + PaddedBorderThickness.Top,
            ResizeFrameBorderThickness.Right + PaddedBorderThickness.Right,
            ResizeFrameBorderThickness.Bottom + PaddedBorderThickness.Bottom);

        private Dpi GetDpi() => PresentationSource.FromVisual(this) is { } source
            ? new Dpi(
                (int)(Dpi.ScreenStandard.X * source.CompositionTarget.TransformToDevice.M11),
                (int)(Dpi.ScreenStandard.Y * source.CompositionTarget.TransformToDevice.M22))
            : Dpi.System;
    }
}
