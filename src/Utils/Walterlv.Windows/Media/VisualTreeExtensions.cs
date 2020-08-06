using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Walterlv.Windows.Media
{
    /// <summary>
    /// 包含可视化树查找相关的辅助方法。
    /// </summary>
    public static class VisualTreeExtensions
    {
        /// <summary>
        /// 找到此可视化元素的根元素（可能返回元素自身，但不会返回 null）。通常在以下情况下需要调用：
        /// <list type="number">
        /// <item>要查找的目标元素不是此元素的子元素，而是其兄弟元素；</item>
        /// <item>要查找的目标元素在另一个可视化树上（例如跨越了 Popup）。</item>
        /// </list>
        /// </summary>
        /// <param name="visual">从此元素开始查找。</param>
        /// <returns>找到的根元素。</returns>
        public static Visual FindRoot(this Visual visual)
        {
            if (visual is null)
            {
                throw new ArgumentNullException(nameof(visual));
            }

            var current = visual;
            var parent = VisualTreeHelper.GetParent(current) as Visual;
            while (parent != null)
            {
                current = parent;
                parent = VisualTreeHelper.GetParent(current) as Visual;
            }
            return current;
        }

        /// <summary>
        /// 找到此元素的全部满足 <paramref name="condition"/> 条件的子元素（包含此元素自身）。
        /// </summary>
        /// <typeparam name="T">子元素要满足的类型（如果不需要特定类型，请传入 <see cref="Visual"/>。</typeparam>
        /// <param name="visual">从此元素开始查找。通常是 <see cref="FindRoot(Visual)"/> 方法的返回值。</param>
        /// <param name="condition">要满足的条件。</param>
        /// <returns>所有满足条件的子元素。</returns>
        public static IEnumerable<T> FindDecendents<T>(this Visual visual,
            Func<VisualTreeSearchConditions, VisualTreeSearchConditions>? condition = null)
            where T : Visual
        {
            if (visual is null)
            {
                throw new ArgumentNullException(nameof(visual));
            }

            foreach (var v in EnumerateDecendents(visual).OfType<T>())
            {
                var c = new VisualTreeSearchConditions(v);
                if (condition is null)
                {
                    yield return v;
                }
                else
                {
                    condition(c);
                    if (c.CheckMatch())
                    {
                        yield return v;
                    }
                }
            }
        }

        private static IEnumerable<Visual> EnumerateDecendents(Visual visual)
        {
            yield return visual;
            var count = VisualTreeHelper.GetChildrenCount(visual);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(visual, i);
                if (child is Visual c)
                {
                    foreach (var grand in EnumerateDecendents(c))
                    {
                        yield return grand;
                    }
                }
            }
        }
    }
}
