using System;
using System.Collections;
using System.Collections.Generic;

namespace Walterlv.Collections.Generic
{
    /// <summary>
    /// 包含一系列元素弱引用的集合。如果元素被垃圾回收，那么也不会出现在此集合中。
    /// </summary>
    /// <typeparam name="T">元素类型。</typeparam>
    public class WeakCollection<T> : IEnumerable<T> where T : class
    {
        /// <summary>
        /// 内部实现：弱引用列表。
        /// </summary>
        private readonly List<WeakReference<T>> _weakList = new List<WeakReference<T>>();

        /// <summary>
        /// 确定此弱引用集合中是否存在某个元素。
        /// </summary>
        /// <param name="item">要检查是否存在的元素。</param>
        /// <returns>
        /// 如果从未加入到此集合中或已从集合中移除，则会返回 false；如果已加入过则返回 true。
        /// 不可能出现元素被回收结果返回 false 的情况，因为传入的参数就此实例的一个引用，必定不会被回收。
        /// </returns>
        public bool Contains(T item)
        {
            // 请注意：
            //  - 只有本身就是修改集合的方法（如 Add/Remove/Clear）才可以进行集合压缩（指将已被垃圾回收后的元素删除）操作，只读方法不可以如此。
            //  - 因为修改集合时会阻止遍历，使得遍历是安全的；但如果使用 Contains 等只读方法也修改集合，那么遍历将不安全。
            return _weakList.Find(x => x.TryGetTarget(out var value) && Equals(value, item)) != null;
        }

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
        public IEnumerator<T> GetEnumerator()
        {
            // 请注意：遍历集合时不允许进行任何集合压缩操作，因为遍历随时可能终止，导致集合压缩方法出现并发危险。
            foreach (var weak in _weakList)
            {
                if (weak.TryGetTarget(out var value))
                {
                    yield return value;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
