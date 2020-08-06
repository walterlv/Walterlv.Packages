   using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Walterlv.Windows.Media
{
    /// <summary>
    /// 为 <see cref="VisualTreeExtensions.FindDecendents"/> 提供查询条件。
    /// </summary>
    public class VisualTreeSearchConditions
    {
        private readonly Visual _self;
        private Func<bool>? _conditions;

        internal VisualTreeSearchConditions(Visual self)
        {
            _self = self;
        }

        /// <summary>
        /// 此元素具有名称 <paramref name="name"/>。
        /// </summary>
        /// <param name="name">元素名称。</param>
        /// <returns>构造器模式。</returns>
        public VisualTreeSearchConditions NameIs(string name)
        {
            _conditions += () => (_self as FrameworkElement)?.Name?.Equals(name, StringComparison.Ordinal) is true;
            return this;
        }

        /// <summary>
        /// 此元素的父级元素是 <typeparamref name="T"/> 类型，且具有指定名称 <paramref name="name"/>。
        /// </summary>
        /// <typeparam name="T">父级元素的类型。</typeparam>
        /// <param name="name">父元素应具有的名称（如果传入 null，则不限名称）。</param>
        /// <returns>构造器模式。</returns>
        public VisualTreeSearchConditions ParentIs<T>(string? name = null)
            where T : Visual
        {
            var v = VisualTreeHelper.GetParent(_self);
            _conditions += () => v is T && (v as FrameworkElement)?.Name?.Equals(name, StringComparison.Ordinal) is true;
            return this;
        }

        /// <summary>
        /// 此元素包含一个 <typeparamref name="T"/> 类型的子元素，且此子元素具有指定名称 <paramref name="name"/>。
        /// </summary>
        /// <typeparam name="T">子元素的类型。</typeparam>
        /// <param name="name">子元素应具有的名称（如果传入 null，则不限名称）。</param>
        /// <returns>构造器模式。</returns>
        public VisualTreeSearchConditions HasChild<T>(string? name = null)
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
            if (_conditions is null)
            {
                return true;
            }

            return _conditions.GetInvocationList().Cast<Func<bool>>().All(x => x());
        }
    }
}
