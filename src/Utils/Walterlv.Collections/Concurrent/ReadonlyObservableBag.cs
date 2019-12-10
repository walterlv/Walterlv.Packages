using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Walterlv.Collections.Concurrent
{
    public class ReadonlyObservableBag<T> : IProducerConsumerCollection<T>, IEnumerable<T>, IEnumerable, ICollection, IReadOnlyCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly ObservableConcurrentBag<T> _originalCollection;
        private readonly ConcurrentBag<T> _collection = new ConcurrentBag<T>();
        private readonly Func<T, bool> _predicate;
        public event PropertyChangedEventHandler? PropertyChanged;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public ReadonlyObservableBag(ObservableConcurrentBag<T> originalCollection, Func<T, bool> predicate)
        {
            _originalCollection = originalCollection;
            _predicate = predicate;
            originalCollection.CollectionChanged += OriginalCollection_CollectionChanged;
        }

        private void OriginalCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => _ = e.Action switch
        {
            NotifyCollectionChangedAction.Add => AddRange(e.NewItems.OfType<T>().Where(_predicate)),
            _ => throw new NotSupportedException("仅支持向集合中添加元素。"),
        };

        bool IProducerConsumerCollection<T>.TryAdd(T item)
        {
            _collection.Add(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<T> { item }));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            return true;
        }

        private int AddRange(IEnumerable<T> items)
        {
            var count = 0;
            foreach (var item in items)
            {
                _collection.Add(item);
                count++;
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items.ToList()));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            return count;
        }

        bool IProducerConsumerCollection<T>.TryTake(out T item)
            => throw new NotSupportedException("只能以只读的方式访问集合。");

        public int Count => _collection.Count;
        object ICollection.SyncRoot => throw new NotSupportedException("此集合本身是线程安全的，且对外访问为只读，因此不支持进行同步。");
        bool ICollection.IsSynchronized => false;
        void IProducerConsumerCollection<T>.CopyTo(T[] array, int index) => _collection.CopyTo(array, index);
        void ICollection.CopyTo(Array array, int index) => ((ICollection)_collection).CopyTo(array, index);
        public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();
        public T[] ToArray() => _collection.ToArray();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
