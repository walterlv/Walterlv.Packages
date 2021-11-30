using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Walterlv.Windows.Controls
{
    /// <summary>
    /// 为布局类动画提供父控件。
    /// <para>通常，为宽度或高度设置动画时必须写死宽高的动画目标值，否则会因为动画设置的宽高值影响了布局而导致无法动画到布局尺寸。</para>
    /// <para>本类型就为了解决此类问题。你可以设置从 0% 动画到 100% 的布局宽高。</para>
    /// </summary>
    public class LayoutAnimationParent : Decorator
    {
        public static readonly DependencyProperty WidthFractionProperty = DependencyProperty.Register(
            nameof(WidthFraction), typeof(double), typeof(LayoutAnimationParent),
            new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double WidthFraction
        {
            get => (double)GetValue(WidthFractionProperty);
            set => SetValue(WidthFractionProperty, value);
        }

        public static readonly DependencyProperty HeightFractionProperty = DependencyProperty.Register(
            nameof(HeightFraction), typeof(double), typeof(LayoutAnimationParent),
            new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double HeightFraction
        {
            get => (double)GetValue(HeightFractionProperty);
            set => SetValue(HeightFractionProperty, value);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            // 第一次布局，获取子元素的最大期望尺寸。（以更大的尺寸布局，可得知元素的真实期望尺寸。）
            base.MeasureOverride(constraint);
            var childFullDesiredSize = Child?.DesiredSize ?? default;
            // 第二次布局，获取子元素的当前期望尺寸。（以更小的尺寸布局，可避免 Arrange 时传入较小尺寸导致被裁。）
            var childLimitedDesiredSize = CalcSizeFraction(childFullDesiredSize);
            var selfDesiredSize = base.MeasureOverride(childLimitedDesiredSize);
            // 返回第二次布局结果作为期望布局尺寸，这样能让 Arrange 方法传入的参数使用此尺寸。
            return selfDesiredSize;
        }

        private Size CalcSizeFraction(Size constraint)
        {
            double width;
            double height;
            if (WidthFraction == 0)
            {
                // 因为是赋值而不是计算，所以必须严格相等。
                width = 0;
            }
            else if (double.IsInfinity(constraint.Width))
            {
                width = double.PositiveInfinity;
            }
            else
            {
                width = constraint.Width * WidthFraction;
            }
            if (HeightFraction == 0)
            {
                // 因为是赋值而不是计算，所以必须严格相等。
                height = 0;
            }
            else if (double.IsInfinity(constraint.Height))
            {
                height = double.PositiveInfinity;
            }
            else
            {
                height = constraint.Height * HeightFraction;
            }
            var size = new Size(
                Math.Max(0, width),
                Math.Max(0, height));
            return size;
        }
    }
}
