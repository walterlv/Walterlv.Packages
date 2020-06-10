using System;
using System.Windows;
using System.Windows.Media;

namespace Walterlv.Windows.Media
{
    /// <summary>
    /// 包含获取 <see cref="Visual"/> 缩放比例的静态方法。
    /// </summary>
    public static class VisualScalingExtensions
    {
        /// <summary>
        /// 获取一个 <paramref name="visual"/> 在显示设备上的尺寸相对于自身尺寸的缩放比。
        /// </summary>
        /// <param name="visual">要获取缩放比的可视化对象。</param>
        /// <returns></returns>
        public static Size GetScalingRatioToDevice(this Visual visual)
        {
            return visual.GetTransformInfoToDevice().size;
        }

        /// <summary>
        /// 获取一个 <paramref name="visual"/> 在显示设备上的尺寸相对于自身尺寸的缩放比和旋转角度（顺时针为正角度）。
        /// </summary>
        /// <param name="visual">要获取缩放比的可视化对象。</param>
        /// <returns></returns>
        public static (Size size, double angle) GetTransformInfoToDevice(this Visual visual)
        {
            if (visual == null) throw new ArgumentNullException(nameof(visual));

            // 内部缩放。
            var root = VisualRoot(visual);
            var transform = ((MatrixTransform)visual.TransformToAncestor(root)).Value;
            // 外部缩放。
            var ct = PresentationSource.FromVisual(visual)?.CompositionTarget;
            if (ct != null)
            {
                transform.Append(ct.TransformToDevice);
            }

            // 计算旋转分量。
            var unitVector = new Vector(1, 0);
            var vector = transform.Transform(unitVector);
            //如果图片旋转了，那么得到的值和图片显示的不同，会被计算旋转后的值，参见 Element 旋转。
            //所以需要把图片还原
            //还原的方法是计算获得的角度，也就是和单位分量角度，由角度可以得到旋转度。
            //用转换旋转之前旋转角度反过来就是得到原来图片的值
            var angle = Vector.AngleBetween(unitVector, vector);
            transform.Rotate(-angle);
            // 综合缩放。
            var rect = new Rect(new Size(1, 1));
            rect.Transform(transform);

            return (rect.Size, angle);
        }

        internal static Size GetScaleSize(Matrix transform)
        {
            // 计算旋转分量。
            var unitVector = new Vector(1, 0);
            var vector = transform.Transform(unitVector);
            //如果图片旋转了，那么得到的值和图片显示的不同，会被计算旋转后的值，参见 Element 旋转。
            //所以需要把图片还原
            //还原的方法是计算获得的角度，也就是和单位分量角度，由角度可以得到旋转度。
            //用转换旋转之前旋转角度反过来就是得到原来图片的值
            var angle = Vector.AngleBetween(unitVector, vector);
            transform.Rotate(-angle);
            // 综合缩放。
            var rect = new Rect(new Size(1, 1));
            rect.Transform(transform);

            return rect.Size;
        }

        /// <summary>
        /// 寻找一个 <see cref="Visual"/> 连接着的可视化树的根。
        /// 通常，如果这个 <see cref="Visual"/> 显示在窗口中，则根为 <see cref="Window"/>；
        /// 不过，如果此 <see cref="Visual"/> 没有显示出来，则根为某一个包含它的 <see cref="Visual"/>。
        /// 如果此 <see cref="Visual"/> 未连接到任何其它 <see cref="Visual"/>，则根为它自身。
        /// </summary>
        /// <param name="visual">要查找可视化树根的起始元素。</param>
        private static Visual VisualRoot(Visual visual)
        {
            if (visual == null)
            {
                throw new ArgumentNullException(nameof(visual));
            }

            var root = visual;
            var parent = VisualTreeHelper.GetParent(visual);
            while (parent != null)
            {
                if (parent is Visual r)
                {
                    root = r;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return root;
        }
    }
}
