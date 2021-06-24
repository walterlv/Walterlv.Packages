using System;
using System.Windows;
using System.Windows.Interop;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;
using Size = System.Windows.Size;
using Lsj.Util.Win32;
using Lsj.Util.Win32.Enums;
using Walterlv.Windows.Media;
using Lsj.Util.Win32.BaseTypes;

namespace Walterlv.Windows.Interop
{
    /// <summary>
    /// 包装一个 <see cref="Window"/> 成为一个 WPF 控件。
    /// </summary>
    public class WindowWrapper : FrameworkElement
    {
        static WindowWrapper()
        {
            FocusableProperty.OverrideMetadata(typeof(WindowWrapper), new UIPropertyMetadata(true));
            FocusVisualStyleProperty.OverrideMetadata(typeof(WindowWrapper), new FrameworkPropertyMetadata(null));
        }

        private readonly WindowStyles _originalChildWindowStyles;

        /// <summary>
        /// 创建包装句柄为 <paramref name="childHandle"/> 窗口的 <see cref="WindowWrapper"/> 的实例。
        /// </summary>
        /// <param name="childHandle">要包装的窗口的句柄。</param>
        public WindowWrapper(IntPtr childHandle)
        {
            // 初始化。
            Handle = childHandle;

            // 监听事件。
            IsVisibleChanged += OnIsVisibleChanged;
            GotFocus += OnGotFocus;
            PreviewKeyDown += OnKeyDown;

            // 设置窗口样式为子窗口。这里的样式值与 HwndHost/HwndSource 成对时设置的值一模一样。
            _originalChildWindowStyles = (WindowStyles)User32.GetWindowLong(Handle, GetWindowLongIndexes.GWL_STYLE).SafeToInt32();
            User32.SetWindowLong(Handle, GetWindowLongIndexes.GWL_STYLE,
                (IntPtr)(_originalChildWindowStyles | WindowStyles.WS_CHILDWINDOW));
        }

        /// <summary>
        /// 获取包装的窗口的句柄。
        /// </summary>
        public IntPtr Handle { get; }

        /// <summary>
        /// 当子窗口被捕获后发生，你可以对此窗口通过 Win32 函数进行更多的处理。
        /// </summary>
        public event EventHandler<Win32WindowEventArgs>? Captured;

        /// <summary>
        /// 当子窗口被释放后发生，你可以对此窗口通过 Win32 函数进行更多的处理。
        /// </summary>
        public event EventHandler<Win32WindowEventArgs>? Released;

        /// <summary>
        /// 获取或设置当前窗口是否已被捕获和自动管理中。
        /// 默认为 true，即窗口不当作控件自动管理；如果设置为 false，则窗口与此控件断开联系。
        /// </summary>
        public bool IsCaptured
        {
            get => (bool)GetValue(IsCapturedProperty);
            set => SetValue(IsCapturedProperty, value);
        }

        /// <summary>
        /// 标识 <see cref="IsCaptured"/> 的附加属性。
        /// </summary>
        public static readonly DependencyProperty IsCapturedProperty = DependencyProperty.Register(
            nameof(IsCaptured), typeof(bool), typeof(WindowWrapper),
            new PropertyMetadata(true, (sender, args) => ((WindowWrapper)sender).OnIsCapturedChanged((bool)args.OldValue, (bool)args.NewValue)));

        private async void OnIsCapturedChanged(bool oldValue, bool newValue)
        {
            bool? captured = null;
            if (newValue)
            {
                if (IsVisible)
                {
                    // 因为 Win32 函数的薛定谔性，以下方法的顺序都是精心测试过的：
                    //  - 不闪
                    //  - 不错位
                    //  - 样式正确
                    //  - 及时渲染
                    await ShowChildAsync().ConfigureAwait(true);
                    IsChildStyle = true;
                    await ArrangeChildAsync().ConfigureAwait(true);
                    captured = true;
                }
            }
            else
            {
                await ArrangeChildAsync().ConfigureAwait(true);
                await ShowChildAsync().ConfigureAwait(true);
                IsChildStyle = false;
                captured = false;
            }

            // 有些程序窗口大小不变时，无论如何刷新渲染都没用。因此必须强制通知窗口大小已经改变。
            // 实际情况是虽然窗口大小不变，但客户区大小变化了，所以本就应该刷新布局。
            await Task.Delay(1).ConfigureAwait(false);
            User32.SendMessage(Handle, WindowsMessages.WM_SIZE, UIntPtr.Zero, IntPtr.Zero);

            if (captured is true)
            {
                Captured?.Invoke(this, new Win32WindowEventArgs(Handle));
            }
            else if (captured is false)
            {
                Released?.Invoke(this, new Win32WindowEventArgs(Handle));
            }
        }

        private async void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var parent = Window.GetWindow(this);
            if (IsVisible)
            {
                LayoutUpdated -= OnLayoutUpdated;
                LayoutUpdated += OnLayoutUpdated;
                if (!IsCaptured)
                {
                    return;
                }
                await ShowChildAsync().ConfigureAwait(false);
            }
            else
            {
                LayoutUpdated -= OnLayoutUpdated;
                if (!IsCaptured)
                {
                    return;
                }
                await HideChildAsync().ConfigureAwait(false);
            }
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (!IsCaptured)
            {
                return;
            }

            // 设置子窗口获取焦点。
            // 这样，Tab 键的切换以及快捷键将仅在 Shell 端生效。
            User32.SetFocus(Handle);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsCaptured)
            {
                return;
            }

            // 当此控件获取焦点的时候，将吞掉全部的键盘事件：
            // 1. 将所有事件转发到子窗口；
            // 2. 避免按键被父窗口响应（此控件获取焦点可理解为已经切换到子窗口了，不能让父窗口处理事件）；
            // 3. 避免焦点转移到其他控件。
            e.Handled = true;
        }

        protected override Size MeasureOverride(Size availableSize) => default;

        protected override Size ArrangeOverride(Size finalSize) =>
            // _ = ArrangeChildAsync(finalSize);
            // Dispatcher.InvokeAsync(() => ArrangeChildAsync(finalSize), DispatcherPriority.Loaded);
            finalSize;

        private void OnLayoutUpdated(object? sender, EventArgs e)
        {
            if (!IsCaptured)
            {
                return;
            }

            // 最终决定在 LayoutUpdated 事件里面更新子窗口的位置和尺寸，因为：
            //  1. Arrange 仅在大小改变的时候才会触发，这使得如果外面更新了 Margin 等导致位置改变大小不变时（例如窗口最大化），窗口位置不会刷新。
            //  2. 使用 Loaded 优先级延迟布局可以解决以上问题，但会导致布局更新不及时，部分依赖于布局的代码（例如拖拽调整窗口大小）会计算错误。
            _ = ArrangeChildAsync();
        }

        /// <summary>
        /// 获取或设置子窗口当前是否是子窗口样式。
        /// </summary>
        private bool IsChildStyle
        {
            get
            {
                var style = (WindowStyles)User32.GetWindowLong(Handle, GetWindowLongIndexes.GWL_STYLE).SafeToInt32();
                return (int)(style & WindowStyles.WS_CHILDWINDOW) != 0;
            }
            set
            {
                var expectedStyle = value
                    ? WindowStyles.WS_CHILDWINDOW | WindowStyles.WS_CLIPCHILDREN
                    : _originalChildWindowStyles;
                expectedStyle = IsVisible
                    ? expectedStyle | WindowStyles.WS_VISIBLE
                    : expectedStyle & ~WindowStyles.WS_VISIBLE;
                User32.SetWindowLong(Handle, GetWindowLongIndexes.GWL_STYLE, (IntPtr)expectedStyle);
            }
        }

        /// <summary>
        /// 显示子窗口。
        /// </summary>
        private async Task ShowChildAsync()
        {
            // 计算父窗口的句柄。
            var hwndParent = PresentationSource.FromVisual(this) is HwndSource parentSource
                ? parentSource.Handle
                : IntPtr.Zero;

            if (IsCaptured)
            {
                // 连接子窗口。
                // 注意连接子窗口后，窗口的消息循环会强制同步，这可能降低 UI 的响应性能。详情请参考：
                // https://blog.walterlv.com/post/all-processes-freezes-if-their-windows-are-connected-via-setparent.html
                User32.SetParent(Handle, hwndParent);
                Debug.WriteLine("[Window] 嵌入");
            }
            else
            {
                User32.SetParent(Handle, IntPtr.Zero);
            }

            // 显示子窗口。
            // 注意调用顺序：先嵌入子窗口，这可以避免任务栏中出现窗口图标。
            User32.ShowWindow(Handle, ShowWindowCommands.SW_SHOW);
            // 发送窗口已取消激活消息。
            const int WA_ACTIVE = 0x0001;
            User32.SendMessage(Handle, WindowsMessages.WM_ACTIVATE, WA_ACTIVE, IntPtr.Zero);
        }

        /// <summary>
        /// 隐藏子窗口。
        /// </summary>
        private async Task HideChildAsync()
        {
            // 发送窗口已取消激活消息。
            const int WA_INACTIVE = 0x0000;
            User32.SendMessage(Handle, WindowsMessages.WM_ACTIVATE, WA_INACTIVE, IntPtr.Zero);
            // 隐藏子窗口。
            User32.ShowWindow(Handle, ShowWindowCommands.SW_HIDE);
            // 显示到奇怪的地方。
            RawMoveWindow(-32000, -32000);

            // 断开子窗口的连接。
            // 这是为了避免一直连接窗口对 UI 响应性能的影响。详情请参考：
            // https://blog.walterlv.com/post/all-processes-freezes-if-their-windows-are-connected-via-setparent.html
            User32.SetParent(Handle, IntPtr.Zero);
            Debug.WriteLine("[Window] 取出");
        }

        /// <summary>
        /// 布局子窗口。
        /// </summary>
        /// <param name="size">设定子窗口的显示尺寸。</param>
        private async Task ArrangeChildAsync(Size? size = default)
        {
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource is null)
            {
                // 因为此方法可能被延迟执行，所以可能此元素已经不在可视化树了，这时会进入此分支。
                return;
            }

            // 获取子窗口相对于父窗口的相对坐标。
            var transform = TransformToAncestor(hwndSource.RootVisual);
            var scalingFactor = this.GetScalingRatioToDevice();
            var offset = transform.Transform(default);

            // 计算布局尺寸。
            if (size == null)
            {
                size = !IsCaptured && User32.GetWindowRect(Handle, out var oldRect)
                    ? (Size?)new Size(oldRect.right - oldRect.left, oldRect.bottom - oldRect.top)
                    : (Size?)new Size(ActualWidth * scalingFactor.Width, ActualHeight * scalingFactor.Height);
            }
            var (width, height) = (size.Value.Width, size.Value.Height);

            // 转移到后台线程执行代码，这可以让 UI 短暂地立刻响应。
            // await Dispatcher.ResumeBackgroundAsync();

            // 移动子窗口到合适的布局位置。
            var x = (int)(offset.X * scalingFactor.Width);
            var y = (int)(offset.Y * scalingFactor.Height);
            var w = (int)width;
            var h = (int)height;

            if (!IsCaptured && User32.GetWindowRect(hwndSource.Handle, out var ownerRect))
            {
                // 如果窗口未被捕获，则还需叠加主窗口坐标。
                x += ownerRect.left;
                y += ownerRect.top;
            }

            User32.MoveWindow(Handle, x, y, w, h, true);
        }

        /// <summary>
        /// 移动当前子窗口到某个特定的 Win32 坐标的位置。
        /// </summary>
        /// <param name="x">屏幕坐标 X（像素单位）。</param>
        /// <param name="y">屏幕坐标 Y（像素单位）。</param>
        private void RawMoveWindow(int x, int y)
        {
            User32.SetWindowPos(Handle, IntPtr.Zero, x, y, 0, 0,
                SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOACTIVATE);
        }
    }
}
