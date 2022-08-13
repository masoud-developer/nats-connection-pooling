using NATSConnectionPool.Interfaces;

namespace NATSConnectionPool.Implementations;

internal class Pool<T> : IDisposable, IPool<T>
{
    #region Constructor & Private Fields

    public Pool(int size, Func<Pool<T>, T> factory)
    {
        if (size <= 0)
            throw new ArgumentOutOfRangeException("size", size,
                "Argument 'size' must be greater than zero.");
        if (factory == null)
            throw new ArgumentNullException("factory");

        this.size = size;
        itemStore = new CircularStore(size);
        this.factory = factory;
        sync = new Semaphore(size, size);
    }

    private readonly Func<Pool<T>, T> factory;
    private readonly IItemStore itemStore;
    private readonly int size;
    private int count;
    private readonly Semaphore sync;

    #endregion

    #region Classes & Interfaces

    private class CircularStore : IItemStore
    {
        private int position = -1;
        private readonly List<Slot> slots;

        public CircularStore(int capacity)
        {
            slots = new List<Slot>(capacity);
        }

        public T Fetch()
        {
            if (Count == 0)
                throw new InvalidOperationException("The buffer is empty.");

            var startPosition = position;
            do
            {
                Advance();
                var slot = slots[position];
                if (!slot.IsInUse)
                {
                    slot.IsInUse = true;
                    --Count;
                    return slot.Item;
                }
            } while (startPosition != position);

            throw new InvalidOperationException("No free slots.");
        }

        public void Store(T item)
        {
            var slot = slots.Find(s => Equals(s.Item, item));
            if (slot == null)
            {
                slot = new Slot(item);
                slots.Add(slot);
            }

            slot.IsInUse = false;
            ++Count;
        }

        public int Count { get; private set; }

        private void Advance()
        {
            position = (position + 1) % slots.Count;
        }

        private class Slot
        {
            public Slot(T item)
            {
                Item = item;
            }

            public T Item { get; }
            public bool IsInUse { get; set; }
        }
    }

    private interface IItemStore
    {
        int Count { get; }
        T Fetch();
        void Store(T item);
    }

    #endregion

    #region Methods

    public T Acquire()
    {
        sync.WaitOne();
        return AcquireLazy();
    }

    private T AcquireEager()
    {
        lock (itemStore)
        {
            return itemStore.Fetch();
        }
    }

    private T AcquireLazy()
    {
        lock (itemStore)
        {
            if (itemStore.Count > 0) return itemStore.Fetch();
        }

        Interlocked.Increment(ref count);
        return factory(this);
    }

    private T AcquireLazyExpanding()
    {
        var shouldExpand = false;
        if (count < size)
        {
            var newCount = Interlocked.Increment(ref count);
            if (newCount <= size)
                shouldExpand = true;
            else
                Interlocked.Decrement(ref count);
        }

        if (shouldExpand)
            return factory(this);
        lock (itemStore)
        {
            return itemStore.Fetch();
        }
    }

    private void PreloadItems()
    {
        for (var i = 0; i < size; i++)
        {
            var item = factory(this);
            itemStore.Store(item);
        }

        count = size;
    }

    public void Release(T item)
    {
        lock (itemStore)
        {
            itemStore.Store(item);
        }

        sync.Release();
    }

    public void Dispose()
    {
        if (IsDisposed) return;

        IsDisposed = true;
        if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
            lock (itemStore)
            {
                while (itemStore.Count > 0)
                {
                    var disposable = (IDisposable)itemStore.Fetch();
                    disposable.Dispose();
                }
            }

        sync.Close();
    }

    public bool IsDisposed { get; private set; }

    #endregion
}