namespace FluentHttp.Benchmark
{
    public abstract class AbstractObjectPool<T>
    {
        private readonly T[] factorys;
        private int[] locks;

        public abstract T CreateInstance();

        private int MaxPoolSize = 10;

        public AbstractObjectPool(int poolSize)
        {
            MaxPoolSize = Math.Max(1, poolSize);
            factorys = new T[MaxPoolSize];
            locks = new int[MaxPoolSize];
            for (int i = 0; i < MaxPoolSize; i++)
            {
                var instance = CreateInstance();
                factorys[i] = instance;
                locks[i] = 0;
            }
        }

        public AbstractObjectPool() : this(Environment.ProcessorCount / 2)
        {
        }

        public T Get(out int identifier)
        {
            for (int i = 0; i < MaxPoolSize; i++)
            {
                if (Interlocked.CompareExchange(ref locks[i], 1, 0) == 0)
                {
                    if (factorys[i] == null)
                    {
                        var conn = CreateInstance();
                        factorys[i] = conn;
                    }
                    identifier = i;
                    return factorys[i];
                }
            }
            identifier = -1;
            return CreateInstance();
        }

        public void Free(int identifier)
        {
            if (identifier < 0 || identifier >= MaxPoolSize)
            {
                return;
            }
            Interlocked.Exchange(ref locks[identifier], 0);
        }
    }
}