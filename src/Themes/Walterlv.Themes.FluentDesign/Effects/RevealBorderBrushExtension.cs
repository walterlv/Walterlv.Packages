using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace Walterlv.Windows.Effects
{
    /// <summary>
    /// Paints a control border with a reveal effect using composition brush and light effects.
    /// </summary>
    public class RevealBorderBrushExtension : MarkupExtension
    {
        [ThreadStatic]
        private static Dictionary<RadialGradientBrush, WeakReference<FrameworkElement>> _globalRevealingElements;

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

        public double Radius { get; set; } = 100.0;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // 如果没有服务，则直接返回。
            if (!(serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget service))
                return null;
            // MarkupExtension 在样式模板中，返回 this 以延迟提供值。
            if (service.TargetObject.GetType().Name.EndsWith("SharedDp"))
                return this;
            if (!(service.TargetObject is FrameworkElement element))
                return this;
            if (DesignerProperties.GetIsInDesignMode(element))
                return new SolidColorBrush(FallbackColor);

            var brush = CreateGlobalBrush(element);
            return brush;
        }

        private Brush CreateBrush(UIElement rootVisual, FrameworkElement element)
        {
            var brush = CreateRadialGradientBrush();
            rootVisual.MouseMove += OnMouseMove;
            return brush;

            void OnMouseMove(object sender, MouseEventArgs e)
            {
                UpdateBrush(brush, e.GetPosition(element));
            }
        }

        private Brush CreateGlobalBrush(FrameworkElement element)
        {
            var brush = CreateRadialGradientBrush();
            if (_globalRevealingElements is null)
            {
                CompositionTarget.Rendering -= OnRendering;
                CompositionTarget.Rendering += OnRendering;
                _globalRevealingElements = new Dictionary<RadialGradientBrush, WeakReference<FrameworkElement>>();
            }

            _globalRevealingElements.Add(brush, new WeakReference<FrameworkElement>(element));
            return brush;
        }

        private void OnRendering(object sender, EventArgs e)
        {
            if (_globalRevealingElements is null)
            {
                return;
            }

            var toCollect = new List<RadialGradientBrush>();
            foreach (var pair in _globalRevealingElements)
            {
                var brush = pair.Key;
                var weak = pair.Value;
                if (weak.TryGetTarget(out var element))
                {
                    Reveal(brush, element);
                }
                else
                {
                    toCollect.Add(brush);
                }
            }

            foreach (var brush in toCollect)
            {
                _globalRevealingElements.Remove(brush);
            }

            void Reveal(RadialGradientBrush brush, IInputElement element)
            {
                UpdateBrush(brush, Mouse.GetPosition(element));
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
    }
}
