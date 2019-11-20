using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Walterlv.Windows.Media
{
    public class VisualTreeSearchConditions
    {
        private readonly Visual _self;
        private Func<bool> _conditions;

        internal VisualTreeSearchConditions(Visual self)
        {
            _self = self;
        }

        public VisualTreeSearchConditions NameIs(string name)
        {
            _conditions += () => (_self as FrameworkElement)?.Name?.Equals(name, StringComparison.Ordinal) is true;
            return this;
        }

        public VisualTreeSearchConditions ParentIs<T>(string name = null)
            where T : Visual
        {
            var v = VisualTreeHelper.GetParent(_self);
            _conditions += () => v is T && (v as FrameworkElement)?.Name?.Equals(name, StringComparison.Ordinal) is true;
            return this;
        }

        public VisualTreeSearchConditions HasChild<T>(string name = null)
            where T : Visual
        {
            _conditions += () =>
            {
                var count = VisualTreeHelper.GetChildrenCount(_self);
                for (var i = 0; i < count; i++)
                {
                    var v = VisualTreeHelper.GetChild(_self, i);
                    var result = v is T && (v as FrameworkElement)?.Name?.Equals(name, StringComparison.Ordinal) is true;
                    if (result)
                    {
                        return true;
                    }
                }
                return false;
            };
            return this;
        }

        internal bool CheckMatch()
        {
            return _conditions.GetInvocationList().Cast<Func<bool>>().All(x => x());
        }
    }
}
