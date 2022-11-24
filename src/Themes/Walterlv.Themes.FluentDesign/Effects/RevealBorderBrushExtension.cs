using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace Walterlv.Windows.Effects
{
    /// <summary>
    /// Paints a control border with a reveal effect using composition brush and light effects.
    /// </summary>
    public class RevealBorderBrushExtension : MarkupExtension
    {
        [ThreadStatic]
        private static Dictionary<RadialGradientBrush, WeakReference<FrameworkElement>>? GlobalRevealingElements;

        /// <summary>
        /// The color to use for rendering in case the <see cref="MarkupExtension"/> can't work correctly.
        /// </summary>
        public Color FallbackColor { get; set; } = Colors.White;

        /// <summary>
        /// Gets or sets a value that specifies the base background color for the brush.
        /// </summary>
        public Color Color { get; set; } = Colors.White;

        public Transform Transform { get; set; } = Transform.Identity;

        public Transform RelativeTransform { get; set; } = Transform.Identity;

        public double Opacity { get; set; } = 1.0;

        /// <summary>
        /// 高亮半径。
        /// </summary>
        public double Radius { get; set; } = 100.0;

        public override object? ProvideValue(IServiceProvider serviceProvider)
        {
            // 如果没有服务，则直接返回。
            if (serviceProvider.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget service)
            {
                return null;
            }

            // MarkupExtension 在样式模板中，返回 this 以延迟提供值。
            if (service.TargetObject.GetType().Name.EndsWith("SharedDp", StringComparison.Ordinal))
            {
                return this;
            }

            if (service.TargetObject is not FrameworkElement element)
            {
                return this;
            }

            if (DesignerProperties.GetIsInDesignMode(element))
            {
                return new SolidColorBrush(FallbackColor);
            }

            var brush = CreateGlobalBrush(element);
            return brush;
        }

        private Brush CreateGlobalBrush(FrameworkElement element)
        {
            var brush = CreateRadialGradientBrush();
            if (GlobalRevealingElements is null)
            {
                if (element.IsLoaded)
                {
                    EnableReveal();
                }
                else
                {
                    element.Loaded += Element_Loaded;
                }
                element.Unloaded += Element_Unloaded;

                GlobalRevealingElements = new Dictionary<RadialGradientBrush, WeakReference<FrameworkElement>>();
            }

            GlobalRevealingElements.Add(brush, new WeakReference<FrameworkElement>(element));
            return brush;
        }

        private void Element_Loaded(object sender, RoutedEventArgs e)
        {
            EnableReveal();
        }

        private void Element_Unloaded(object sender, RoutedEventArgs e)
        {
            DisableReveal();
        }

        private void EnableReveal()
        {
            CompositionTarget.Rendering -= OnRendering;
            CompositionTarget.Rendering += OnRendering;
        }

        private void DisableReveal()
        {
            CompositionTarget.Rendering -= OnRendering;
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            if (GlobalRevealingElements is null)
            {
                return;
            }

            var toCollect = new List<RadialGradientBrush>();
            foreach (var pair in GlobalRevealingElements)
            {
                var brush = pair.Key;
                var weak = pair.Value;
                if (weak.TryGetTarget(out var element))
                {
                    element.Dispatcher.InvokeAsync(() => Reveal(brush, element), DispatcherPriority.Normal);
                }
                else
                {
                    toCollect.Add(brush);
                }
            }

            foreach (var brush in toCollect)
            {
                GlobalRevealingElements.Remove(brush);
            }

            void Reveal(RadialGradientBrush brush, IInputElement element)
            {
                if (element is FrameworkElement fe && PresentationSource.FromVisual(fe) is HwndSource source
                    && IsWindow(source.Handle))
                {
                    var p = Mouse.GetPosition(element);
                    UpdateBrush(brush, p);
                }
                else
                {
                    UpdateBrush(brush, new Point(double.NegativeInfinity, double.NegativeInfinity));
                }
            }
        }

        private void UpdateBrush(RadialGradientBrush brush, Point origin)
        {
            if (IsUsingCursorPointerDevice())
            {
                brush.GradientOrigin = origin;
                brush.Center = origin;
            }
            else
            {
                brush.Center = new Point(double.NegativeInfinity, double.NegativeInfinity);
            }
        }

        private RadialGradientBrush CreateRadialGradientBrush()
        {
            var brush = new RadialGradientBrush(Color, Colors.Transparent)
            {
                MappingMode = BrushMappingMode.Absolute,
                RadiusX = Radius,
                RadiusY = Radius,
                Opacity = Opacity,
                Transform = Transform,
                RelativeTransform = RelativeTransform,
                Center = new Point(double.NegativeInfinity, double.NegativeInfinity),
            };
            return brush;
        }

        /// <summary>
        /// 判断当前正在使用的点输入设备是否是包含指示位置的指针型输入设备。
        /// 在笔、触摸和触笔三种输入中，笔和触笔会在实际操作之前显示表示其位置的光标，而触摸则只能在发生操作时得知其位置；
        /// 因此，这三种输入方式中，只有笔和触笔会返回 true，其他返回 false。
        /// </summary>
        private bool IsUsingCursorPointerDevice()
        {
            var device = Stylus.CurrentStylusDevice;
            if (device is null)
            {
                return true;
            }

            if (device.TabletDevice.Type == TabletDeviceType.Stylus)
            {
                return true;
            }

            return false;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern bool IsWindow(IntPtr hwnd);
    }
}
