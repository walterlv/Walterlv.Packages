using System;
using System.Collections.Generic;

namespace Walterlv.WeakEvents
{
    /// <summary>
    /// 定义一个弱事件。
    /// 此类型的所有方法是线程安全的。
    /// </summary>
    /// <typeparam name="TSender">事件源类型。如果是常见的 <see cref="object"/> 类型，可以使用 <see cref="WeakEventRelay{TEventSource}"/> 泛型类型。</typeparam>
    /// <typeparam name="TArgs">事件参数类型。如果不知道事件参数的类型，可以查看委托定义中事件参数的定义。</typeparam>
    /// <remarks>
    /// 有两种用法：
    /// 1. 在事件源定义事件的时候使用，这可以使得此事件不会强引用事件的订阅者；
    /// 2. 配合 <see cref="WeakEventRelay{TEventSource}"/> 做一个弱事件中继，为库中原来没有做弱事件的类型添加弱事件支持。
    /// 有关此类型的两种不同用法，请参阅文档：
    /// 1. https://blog.walterlv.com/post/implement-custom-dotnet-weak-event.html
    /// 2. https://blog.walterlv.com/post/implement-custom-dotnet-weak-event-relay.html
    /// </remarks>
    public class WeakEvent<TSender, TArgs>
    {
        /// <summary>
        /// 提供线程安全的锁。
        /// </summary>
        private readonly object _locker = new object();

        /// <summary>
        /// 包含所有原始的事件处理函数，也就是订阅者通过 += 或者 -= 后面的部分代码。
        /// 我们必须保存原始处理函数的弱引用实例，以便在使用 -= 注销事件时能够注销到与 += 相同的实例。
        /// </summary>
        private readonly List<WeakReference<Delegate>> _originalHandlers = new List<WeakReference<Delegate>>();

        /// <summary>
        /// 包含所有已经通用化的事件处理函数，也就是原始事件处理函数经过隐式转换后转成的 Action。
        /// 在实际上引发事件的时候，我们会使用此隐式转换后的实例，这样可以避免使用原始事件处理函数导致的反射、IL 生成等耗性能的执行。
        /// </summary>
        private readonly List<WeakReference<Action<TSender, TArgs>>> _castedHandlers = new List<WeakReference<Action<TSender, TArgs>>>();

        /// <summary>
        /// 订阅弱事件处理函数。
        /// </summary>
        /// <param name="originalHandler">原始处理函数，请始终传入 <code>value</code>。</param>
        /// <param name="castedHandler">可被隐式转换为 Action 的方法组，请始终传入 <code>value.Invoke</code>。</param>
        public void Add(Delegate originalHandler, Action<TSender, TArgs> castedHandler)
        {
            lock (_locker)
            {
                _originalHandlers.Add(new WeakReference<Delegate>(originalHandler));
                _castedHandlers.Add(new WeakReference<Action<TSender, TArgs>>(castedHandler));
            }
        }

        /// <summary>
        /// 注销弱事件处理函数。
        /// </summary>
        /// <param name="originalHandler">原始处理函数，请始终传入 <code>value</code>。</param>
        public void Remove(Delegate originalHandler)
        {
            lock (_locker)
            {
                var index = _originalHandlers.FindIndex(x => x.TryGetTarget(out var handler) && handler == originalHandler);
                if (index >= 0)
                {
                    _originalHandlers.RemoveAt(index);
                    _castedHandlers.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// 引发弱事件，并传入事件引发源和事件参数。
        /// </summary>
        /// <param name="sender">事件引发源。</param>
        /// <param name="e">事件参数。</param>
        /// <returns>
        /// 如果在引发事件后发现已经没有任何对象订阅了此事件，则返回 false，这表明可以着手回收事件中继了。
        /// 相反，如果返回了 true，说明还有存活的对象正在订阅此事件。
        /// </returns>
        public bool Invoke(TSender sender, TArgs e)
        {
            List<Action<TSender, TArgs>> invokingHandlers = null;
            lock (_locker)
            {
                var handlers = _castedHandlers.ConvertAll(x => x.TryGetTarget(out var target) ? target : null);
                var anyHandlerAlive = handlers.Exists(x => x != null);
                if (anyHandlerAlive)
                {
                    invokingHandlers = handlers;
                }
                else
                {
                    invokingHandlers = null;
                    _castedHandlers.Clear();
                }
            }
            if (invokingHandlers != null)
            {
                foreach (var handler in invokingHandlers)
                {
                    var strongHandler = handler;
                    strongHandler(sender, e);
                }
            }
            return invokingHandlers != null;
        }
    }

    /// <summary>
    /// 定义一个弱事件。
    /// 此类型的所有方法是线程安全的。
    /// </summary>
    /// <typeparam name="TArgs">事件参数类型。如果不知道事件参数的类型，可以查看委托定义中事件参数的定义。</typeparam>
    /// <remarks>
    /// 有两种用法：
    /// 1. 在事件源定义事件的时候使用，这可以使得此事件不会强引用事件的订阅者；
    /// 2. 配合 <see cref="WeakEventRelay{TEventSource}"/> 做一个弱事件中继，为库中原来没有做弱事件的类型添加弱事件支持。
    /// 有关此类型的两种不同用法，请参阅文档：
    /// 1. https://blog.walterlv.com/post/implement-custom-dotnet-weak-event.html
    /// 2. https://blog.walterlv.com/post/implement-custom-dotnet-weak-event-relay.html
    /// </remarks>
    public class WeakEvent<TArgs> : WeakEvent<object, TArgs>
    {
    }
}
