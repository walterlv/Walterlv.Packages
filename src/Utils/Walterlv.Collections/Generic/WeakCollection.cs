using System;
using System.Collections.Generic;

namespace Walterlv.Collections.Generic
{
    /// <summary>
    /// 包含一系列元素弱引用的集合。如果元素被垃圾回收，那么也不会出现在此集合中。
    /// </summary>
    /// <typeparam name="T">元素类型。</typeparam>
    public class WeakCollection<T> where T : class
    {
        /// <summary>
        /// 内部实现：弱引用列表。
        /// </summary>
        private readonly List<WeakReference<T>> _weakList = new List<WeakReference<T>>();

        /// <summary>
        /// 将某个元素添加到弱引用集合中。
        /// </summary>
        /// <param name="item">要添加的元素。</param>
        public void Add(T item)
        {
            for (var i = 0; i < _weakList.Count; i++)
            {
                var weak = _weakList[i];
                if (!weak.TryGetTarget(out _))
                {
                    _weakList.RemoveAt(i);
                    i--;
                }
            }
            _weakList.Add(new WeakReference<T>(item));
        }

        /// <summary>
        /// 将某个元素从弱引用集合中移除。
        /// </summary>
        /// <param name="item">要移除的元素。</param>
        /// <returns>如果元素已经在此次操作中被移除，则返回 true；如果元素不在集合中，则返回 false。</returns>
        public bool Remove(T item)
        {
            for (var i = 0; i < _weakList.Count; i++)
            {
                var weak = _weakList[i];
                if (weak.TryGetTarget(out var value))
                {
                    if (Equals(value, item))
                    {
                        _weakList.RemoveAt(i);
                        return true;
                    }
                }
                else
                {
                    _weakList.RemoveAt(i);
                    i--;
                }
            }
            return false;
        }

        /// <summary>
        /// 清除此弱引用集合中的所有元素。
        /// </summary>
        public void Clear() => _weakList.Clear();

        /// <summary>
        /// 获取此弱引用集合中元素的枚举器。
        /// </summary>
        /// <returns>弱引用集合中元素的枚举器。</returns>
        public T[] TryGetItems(Func<T, bool> filter)
        {
            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var list = new List<T>();
            for (var i = 0; i < _weakList.Count; i++)
            {
                var weak = _weakList[i];
                if (weak.TryGetTarget(out var value))
                {
                    if (filter(value))
                    {
                        list.Add(value);
                    }
                }
                else
                {
                    _weakList.RemoveAt(i);
                    i--;
                }
            }
            return list.ToArray();
        }
    }
}
