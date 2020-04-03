using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Walterlv.Windows.Effects
{
    /// <summary>
    /// 使用 2D 的缩放来模拟按下元素时的倾斜效果。
    /// 如果搭配 <see cref="RevealBorderBrushExtension"/> 光照效果带来的渐变，会更像 3D 的倾斜。
    /// </summary>
    public class TiltEffect2D
    {
        /// <summary>
        /// 标识 IsEnabled 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled", typeof(bool), typeof(TiltEffect2D),
            new PropertyMetadata(false, OnTiltIsEnabledChanged));

        /// <summary>
        /// 设置是否开启倾斜效果。
        /// </summary>
        /// <param name="element">要设置倾斜效果的元素。</param>
        /// <param name="value">true 表示设置倾斜效果，false 表示取消倾斜效果。</param>
        public static void SetIsEnabled(DependencyObject element, bool value)
        {
            element.SetValue(IsEnabledProperty, value);
        }

        /// <summary>
        /// 获取是否开启倾斜效果。
        /// </summary>
        /// <param name="element">要获取倾斜效果的元素。</param>
        public static bool GetIsEnabled(DependencyObject element)
        {
            return (bool)element.GetValue(IsEnabledProperty);
        }

        /// <summary>
        /// 标识 TiltEffect 的依赖属性。
        /// </summary>
        public static readonly DependencyProperty TiltEffectProperty = DependencyProperty.RegisterAttached(
            "TiltEffect", typeof(TiltEffect2D), typeof(TiltEffect2D),
            new PropertyMetadata(null, OnTiltEffectChanged));

        /// <summary>
        /// 设置倾斜效果，设置此属性为非 null 会自动打开此元素的倾斜效果。
        /// </summary>
        /// <param name="element">要设置倾斜效果的元素。</param>
        /// <param name="value">设置的倾斜效果。</param>
        public static void SetTiltEffect(DependencyObject element, TiltEffect2D value)
        {
            element.SetValue(TiltEffectProperty, value);
        }

        /// <summary>
        /// 获取倾斜效果。
        /// </summary>
        /// <param name="element">要获取倾斜效果的元素。</param>
        public static TiltEffect2D GetTiltEffect(DependencyObject element)
        {
            return (TiltEffect2D)element.GetValue(TiltEffectProperty);
        }

        /// <summary>
        /// 获取或设置一个可继承的依赖项属性的值，该值指示当前是否已经有一个元素正在倾斜。
        /// 这可以避免带有嵌套倾斜的元素出现多重倾斜效果。
        /// </summary>
        private static readonly DependencyProperty IsTiltingProperty = DependencyProperty.RegisterAttached(
            "IsTilting", typeof(bool), typeof(TiltEffect2D),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

        private static void OnTiltIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                var oldEffect = (TiltEffect2D)d.GetValue(TiltEffectProperty);

                if (e.OldValue is true)
                {
                    oldEffect.Disable();
                    d.ClearValue(TiltEffectProperty);
                }

                if (e.NewValue is true)
                {
                    var newEffect = new TiltEffect2D();
                    d.SetValue(TiltEffectProperty, newEffect);
                    newEffect.Enable(element);
                }
            }
        }

        private static void OnTiltEffectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                if (e.OldValue is TiltEffect2D oldEffect)
                {
                    oldEffect.Disable();
                }

                if (e.NewValue is TiltEffect2D newEffect)
                {
                    d.SetValue(IsEnabledProperty, true);
                    newEffect.Enable(element);
                }
            }
        }

        /// <summary>
        /// 创建 <see cref="TiltEffect2D"/> 的新实例。
        /// </summary>
        public TiltEffect2D()
        {
            _downHandler = OnMouseLeftButtonDown;
            _upHandler = OnMouseLeftButtonUp;
            _leaveHandler = OnMouseLeave;
        }

        private readonly MouseButtonEventHandler _downHandler;
        private readonly MouseButtonEventHandler _upHandler;
        private readonly MouseEventHandler _leaveHandler;

        private FrameworkElement? _target;
        private Lazy<Storyboard>? _tiltDownStoryboard;
        private Lazy<Storyboard>? _tiltUpStoryboard;
        private Storyboard? TiltDownStoryboard => _tiltDownStoryboard?.Value;
        private Storyboard? TiltUpStoryboard => _tiltUpStoryboard?.Value;

        /// <summary>
        /// 当前正在倾斜时，获取或设置元素的根父级。
        /// 同一父子链下的元素通过此依赖对象获取 <see cref="IsTiltingProperty"/> 属性时可拿到同一个值。
        /// 这可以避免带有嵌套倾斜的元素出现多重倾斜效果。
        /// </summary>
        private DependencyObject? _currentRoot;

        /// <summary>
        /// 获取或设置按下时倾斜动画播放的时长。
        /// </summary>
        public Duration DownDuration { get; set; } = new Duration(TimeSpan.FromSeconds(0.1));

        /// <summary>
        /// 获取或设置松开时倾斜动画播放的时长。
        /// </summary>
        public Duration UpDuration { get; set; } = new Duration(TimeSpan.FromSeconds(0.2));

        /// <summary>
        /// 获取或设置倾斜程度。
        /// 使用的是类似于有效像素的单位，(2, 2) 表示 X Y 方向在完全倾斜时会偏移 (2, 2) 个有效像素。
        /// 对于长宽比特别大的元素，这个值会成为参考；其他元素，这个值会接近有效像素值。
        /// </summary>
        public Size TiltOffset { get; set; } = new Size(2, 2);

        /// <summary>
        /// 开启倾斜效果。
        /// </summary>
        /// <param name="element">要开启倾斜效果的元素。</param>
        private void Enable(FrameworkElement element)
        {
            _target = element;

            _tiltDownStoryboard = new Lazy<Storyboard>(() => CreateTiltStoryboard(element, true));
            _tiltUpStoryboard = new Lazy<Storyboard>(() => CreateTiltStoryboard(element, false));

            element.AddHandler(UIElement.MouseLeftButtonDownEvent, _downHandler, true);
            element.AddHandler(UIElement.MouseLeftButtonUpEvent, _upHandler, true);
            element.AddHandler(UIElement.MouseLeaveEvent, _leaveHandler, true);
        }

        /// <summary>
        /// 关闭倾斜效果。
        /// </summary>
        private void Disable()
        {
            _tiltDownStoryboard = null;
            _tiltUpStoryboard = null;
            if (_target is FrameworkElement element)
            {
                element.RemoveHandler(UIElement.MouseLeftButtonDownEvent, _downHandler);
                element.RemoveHandler(UIElement.MouseLeftButtonUpEvent, _upHandler);
                element.RemoveHandler(UIElement.MouseLeaveEvent, _leaveHandler);
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var target = _target;
            if (target is null)
            {
                return;
            }

            if (target.GetValue(IsTiltingProperty) is true)
            {
                return;
            }

            _currentRoot = FindRootVisual(target);
            _currentRoot.SetValue(IsTiltingProperty, true);

            var position = e.GetPosition(_target);
            target.RenderTransformOrigin = new Point(
                1 - position.X / target.ActualWidth,
                1 - position.Y / target.ActualHeight);

            TiltUpStoryboard?.Stop(_target);
            TiltDownStoryboard?.Begin(_target);
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _currentRoot?.ClearValue(IsTiltingProperty);

            TiltDownStoryboard?.Stop(_target);
            TiltUpStoryboard?.Begin(_target);
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            _currentRoot?.ClearValue(IsTiltingProperty);

            TiltDownStoryboard?.Stop(_target);
            TiltUpStoryboard?.Begin(_target);
        }

        /// <summary>
        /// 创建倾斜效果的故事板。
        /// </summary>
        /// <param name="element">要创建倾斜效果的元素。</param>
        /// <param name="shouldScale">是否通过缩放来模拟倾斜效果。</param>
        [Pure]
        private Storyboard CreateTiltStoryboard(FrameworkElement element, bool shouldScale)
        {
            var scaleXAnimation = new DoubleAnimation
            {
                Duration = DownDuration,
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut },
            };
            var scaleYAnimation = new DoubleAnimation
            {
                Duration = UpDuration,
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut },
            };

            if (shouldScale)
            {
                var x = 1 - TiltOffset.Width / element.ActualWidth;
                var y = 1 - TiltOffset.Height / element.ActualHeight;
                var ratio = element.ActualWidth / element.ActualHeight;
                var ratioThreshold = TiltOffset.Width + TiltOffset.Height;
                if (ratio < 1 / ratioThreshold || ratio > ratioThreshold)
                {
                    // 当元素的长宽比非常大时，使用分别的缩放会使得元素拉伸严重，于是使用统一的分量。
                    var to = Math.Max(x, y);
                    scaleXAnimation.To = to;
                    scaleYAnimation.To = to;
                }
                else
                {
                    // 当元素的长宽比较小时，使用分别的缩放以获得较明显的效果。
                    scaleXAnimation.To = x;
                    scaleYAnimation.To = y;
                }
            }

            var tiltStoryboard = new Storyboard();
            Storyboard.SetTarget(scaleXAnimation, element);
            Storyboard.SetTarget(scaleYAnimation, element);
            Storyboard.SetTargetProperty(scaleXAnimation, BuildScalePropertyPath(element, "X"));
            Storyboard.SetTargetProperty(scaleYAnimation, BuildScalePropertyPath(element, "Y"));
            tiltStoryboard.Children.Add(scaleXAnimation);
            tiltStoryboard.Children.Add(scaleYAnimation);
            return tiltStoryboard;
        }

        /// <summary>
        /// 根据元素当前设置的 <see cref="UIElement.RenderTransform"/> 来决定应该使用怎样的属性路径来做倾斜动画。
        /// </summary>
        /// <param name="element">查找属性的元素。</param>
        /// <param name="directionName">X 或者 Y。</param>
        /// <returns>一个 <see cref="PropertyPath"/> 表示倾斜动画的对应属性。</returns>
        [Pure]
        private static PropertyPath BuildScalePropertyPath(FrameworkElement element, string directionName)
        {
            if (element.RenderTransform is ScaleTransform)
            {
                return new PropertyPath($"(UIElement.RenderTransform).(ScaleTransform.Scale{directionName})");
            }

            if (element.RenderTransform is TransformGroup group)
            {
                var index = group.Children.ToList().FindIndex(x => x is ScaleTransform);
                if (index < 0)
                {
                    throw new InvalidOperationException("进行倾斜变换的元素，如果指定了 RenderTransform，必须包含缩放部分。");
                }

                return new PropertyPath(
                    $"(UIElement.RenderTransform).(TransformGroup.Children)[{index}].(ScaleTransform.Scale{directionName})");
            }

            if (element.RenderTransform is MatrixTransform mt && mt.Matrix == Matrix.Identity)
            {
                element.RenderTransform = new ScaleTransform();
                return new PropertyPath($"(UIElement.RenderTransform).(ScaleTransform.Scale{directionName})");
            }

            throw new InvalidOperationException("进行倾斜变换的元素，如果指定了 RenderTransform，必须包含缩放部分。");
        }

        /// <summary>
        /// 查找某个元素当前的根元素，用于避免带有嵌套倾斜的元素出现多重倾斜效果。
        /// </summary>
        /// <param name="element">查找跟元素的查找源。</param>
        private static DependencyObject FindRootVisual(DependencyObject element)
        {
            var parent = LogicalTreeHelper.GetParent(element) ?? VisualTreeHelper.GetParent(element);
            while (parent != null)
            {
                var p = LogicalTreeHelper.GetParent(parent) ?? VisualTreeHelper.GetParent(parent);
                if (p == null)
                {
                    return parent;
                }

                parent = p;
            }

            return element;
        }
    }
}
