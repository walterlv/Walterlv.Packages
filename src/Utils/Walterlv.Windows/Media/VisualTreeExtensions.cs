using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Walterlv.Windows.Media
{
    public static class VisualTreeExtensions
    {
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
                else if (c.CheckMatch())
                {
                    yield return v;
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
