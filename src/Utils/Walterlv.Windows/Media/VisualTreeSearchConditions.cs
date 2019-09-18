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

        public VisualTreeSearchConditions ParentIs<T>(string name = null)
            where T : Visual
        {
            var v = VisualTreeHelper.GetParent(_self);
            _conditions += () => v is T && (v as FrameworkElement)?.Name?.Equals(name) is true;
            return this;
        }

        internal bool CheckMatch()
        {
            return _conditions.GetInvocationList().Cast<Func<bool>>().All(x => x());
        }
    }
}
