using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Walterlv.WeakEvents
{
    /// <summary>
    /// 为已有对象的事件添加弱事件中继，这可以避免町事件的对象因为无法释放而导致的内存泄漏问题。
    /// 通过编写一个继承自此类型的自定义类型，可以将原有任何 CLR 事件转换为弱事件。
    /// </summary>
    /// <typeparam name="TEventSource">事件原始引发源的类型。</typeparam>
    /// <remarks>
    /// 此弱事件中继要求具有较高的事件中转性能，所以没有使用到任何反射或其他动态调用方法。
    /// 为此，编写一个自定义的弱事件中继可能会有些困难，如果需要，请参阅文档：
    /// https://blog.walterlv.com/post/implement-custom-dotnet-weak-event-relay.html
    /// </remarks>
    public abstract class WeakEventRelay<TEventSource> where TEventSource : class
    {
        /// <summary>
        /// 获取事件引发源（也就是事件参数里的那个 sender 参数）。
        /// 由于此弱事件中继会在有事件订阅的时候被 sender 强引用，所以两者的生命周期近乎相同，不需要弱引用此对象。
        /// </summary>
        private readonly TEventSource _eventSource;

        /// <summary>
        /// 保留所有已订阅的事件名（相当于一个线程安全的哈希表）。
        /// 这样，每一个原始事件仅仅会真实地订阅一次，专门用于让中转方法被调用一次；当然，最终中转引发弱事件的时候可以有很多次，但与此字段无关。
        /// </summary>
        private readonly ConcurrentDictionary<string, string> _events = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 初始化弱事件中继对象的基类属性。
        /// 在初始化此实例后，请不要用任何方式保留此实例的引用，除非你自己能处理好事件的注销（-=）。
        /// </summary>
        /// <param name="eventSource">事件引发源的实例。</param>
        protected WeakEventRelay(TEventSource eventSource) => _eventSource = eventSource ?? throw new ArgumentNullException(nameof(eventSource));

        /// <summary>
        /// 在派生类中实现自定义事件的中继的时候，需要在事件的 add 方法中调用此方法以订阅弱事件。
        /// </summary>
        /// <param name="sourceEventAdder">请始终写为 <code>o => o.事件名 += On事件名</code>；例如 <code>o => o.Changed += OnChanged</code>。</param>
        /// <param name="relayEventAdder">请始终写为 <code>() => 弱事件.Add(value, value.Invoke)</code>；例如 <code>() => _changed.Add(value, value.Invoke)</code>。</param>
        /// <param name="eventName">请让编译器自动传入此参数。此事件名不会作反射或其他耗性能的用途，仅仅用于防止事件重复订阅造成的额外性能问题。</param>
        /// <remarks>
        /// 有关详细写法，请参阅文档：
        /// https://blog.walterlv.com/post/implement-custom-dotnet-weak-event-relay.html
        /// </remarks>
        protected void Subscribe(Action<TEventSource> sourceEventAdder, Action relayEventAdder, [CallerMemberName] string? eventName = null)
        {
            if (eventName is null)
            {
                throw new ArgumentNullException(nameof(eventName));
            }

            //                                  <--订阅--   [最终订阅者 1]
            // [事件源]   <--订阅--   [事件中继]   <--订阅--   [最终订阅者 2]
            //                                  <--订阅--   [最终订阅者 3]

            if (_events.TryAdd(eventName, eventName))
            {
                // 中继仅仅向源事件订阅一次。
                sourceEventAdder(_eventSource);
            }

            // 但是允许弱事件订阅者订阅很多次弱事件。
            relayEventAdder();
        }

        /// <summary>
        /// 请在原始事件的事件处理函数中调用此方法，并且请始终写为 <code>TryInvoke(弱事件, sender, e)</code>。
        /// </summary>
        /// <typeparam name="TSender">事件引发源的类型，可隐式推断。</typeparam>
        /// <typeparam name="TArgs">事件参数的类型，可隐式推断。</typeparam>
        /// <param name="weakEvent">弱事件对象，请使用 <see cref="WeakEvent{TArgs}"/> 来创建并存为字段。</param>
        /// <param name="sender">源事件的引发者，即 sender。请始终传入 sender。</param>
        /// <param name="e">源事件的事件参数，即 e。请始终传入 e。</param>
        /// <remarks>
        /// 有关详细写法，请参阅文档：
        /// https://blog.walterlv.com/post/implement-custom-weak-event-relay.html
        /// </remarks>
        protected void TryInvoke<TSender, TArgs>(WeakEvent<TSender, TArgs> weakEvent, TSender sender, TArgs e)
        {
            // 引发弱事件，并确认是否仍有订阅者存活（未被 GC 回收）。
            var anyAlive = weakEvent.Invoke(sender, e);
            if (!anyAlive)
            {
                // 如果没有任何订阅者存活，那么要求派生类清除事件源的订阅，这可以清除此事件中继的实例。
                OnReferenceLost(_eventSource);
            }
        }

        /// <summary>
        /// 当没有任何事件订阅者存活的时候，会调用此方法。
        /// 在派生类中实现此方法的时候，需要清除所有对事件源中全部事件的订阅，以便清除此事件中继的实例。
        /// 另外，如果事件源实现了 <see cref="IDisposable"/> 接口，建议在可能的情况下调用 <see cref="IDisposable.Dispose"/> 方法，这可以释放事件源的资源。
        /// </summary>
        /// <param name="source">事件源的实例。</param>
        /// <remarks>
        /// 此方法可能调用多次，也可能永远不会被调用。
        /// 如果调用多次，说明事件在引发/回收之后有对象发生了新的订阅。
        /// 如果永远不会调用，这是个好事，说明事件源自己已经被回收了，那么此中继对象自然也被回收；这时不调用此方法也不会产生任何泄漏。
        /// </remarks>
        protected abstract void OnReferenceLost(TEventSource source);
    }
}
