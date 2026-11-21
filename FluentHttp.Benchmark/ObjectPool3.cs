namespace FluentHttp.Benchmark
{

    public sealed class ObjectPool3<T> where T : class
    {
        private readonly T[] _items;
        private readonly int[] _locks;
        private readonly Func<T> _factory;
        private readonly Action<T>? _reset;
        private readonly int _poolSize;

        public ObjectPool3(
            Func<T> factory,
            Action<T>? reset = null,
            int poolSize = 0)
        {
            ArgumentNullException.ThrowIfNull(factory);

            var size = poolSize > 0
                ? poolSize
                : Math.Max(1, Environment.ProcessorCount / 2);

            _factory = factory;
            _reset = reset;
            _poolSize = size;
            _items = new T[size];
            _locks = new int[size];
        }

        public T Get(out int identifier)
        {
            for (var i = 0; i < _poolSize; i++)
            {
                if (Interlocked.CompareExchange(ref _locks[i], 1, 0) == 0)
                {
                    var item = Volatile.Read(ref _items[i]);

                    if (item is null)
                    {
                        item = _factory()
                            ?? throw new InvalidOperationException(
                                "Object pool factory returned null.");

                        Volatile.Write(ref _items[i], item);
                    }

                    identifier = i;
                    return item;
                }
            }

            identifier = -1;
            return _factory()
                ?? throw new InvalidOperationException(
                    "Object pool factory returned null.");
        }

        public void Free(T t, int identifier)
        {
            if (identifier < 0 || identifier >= _poolSize)
            {
                (t as IDisposable)?.Dispose();
                return;
            }

            if (Interlocked.Exchange(ref _locks[identifier], 0) != 1)
            {
                return;
            }

            var item = Volatile.Read(ref _items[identifier]);
            if (item is not null)
            {
                _reset?.Invoke(item);
            }
        }
    }
}
