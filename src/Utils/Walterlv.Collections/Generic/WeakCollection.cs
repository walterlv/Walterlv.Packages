using System;
using System.Collections.Generic;
using System.Linq;

namespace Walterlv.Collections.Generic;
/// <summary>
/// 包含一系列元素弱引用的集合。如果元素被垃圾回收，那么也不会出现在此集合中。
/// </summary>
/// <typeparam name="T">元素类型。</typeparam>
public class WeakCollection<T> where T : class
{
    /// <summary>
    /// 内部实现：弱引用列表。
    /// </summary>
    private readonly List<WeakReference<T>> _weakList = new();

    /// <summary>
    /// 将某个元素添加到弱引用集合中。
    /// </summary>
    /// <param name="item">要添加的元素。</param>
    public void Add(T item)
    {
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

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
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }

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
    /// <param name="filter"></param>
    /// <returns>弱引用集合中元素的枚举器。</returns>
    public T[] TryGetItems(Func<T, bool>? filter = null)
    {
        return Enumerate(filter).ToArray();

        IEnumerable<T> Enumerate(Func<T, bool>? filter)
        {
            for (var i = 0; i < _weakList.Count; i++)
            {
                var weak = _weakList[i];
                if (weak.TryGetTarget(out var value))
                {
                    if (filter?.Invoke(value) is not false)
                    {
                        yield return value;
                    }
                }
                else
                {
                    _weakList.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    /// <summary>
    /// 在此弱引用集合中查找满足条件的第一个元素，如果存在则将其返回；如果不存在，则立即创建一个新的元素、添加到集合中并返回。
    /// </summary>
    /// <param name="predicate">元素的查找条件。</param>
    /// <param name="itemFactory">当元素不存在时应如何创建元素。</param>
    /// <returns>查找到的或新添加的元素。</returns>
    public T GetOrAdd(Func<T, bool> predicate, Func<T> itemFactory)
    {
        for (var i = 0; i < _weakList.Count; i++)
        {
            var weak = _weakList[i];
            if (weak.TryGetTarget(out var value))
            {
                if (predicate(value))
                {
                    return value;
                }
            }
            else
            {
                _weakList.RemoveAt(i);
                i--;
            }
        }

        var item = itemFactory() ?? throw new ArgumentException("The item factory should not return null.");
        if (!predicate(item))
        {
            throw new ArgumentException("The item factory should return an item that matches the predicate.");
        }

        _weakList.Add(new WeakReference<T>(item));
        return item;
    }
}
