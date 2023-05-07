using System.Diagnostics;

namespace FluentHttp.Benchmark
{
    internal class ObjectPool<T> where T : class
    {
        private struct Element
        {
            internal T? Value;
        }

        internal delegate T Factory();

        private T? _firstItem;
        private readonly Element[] _items;
        private readonly Factory _factory;

        public readonly bool TrimOnFree;

        internal ObjectPool(Factory factory, bool trimOnFree = true)
           : this(factory, Environment.ProcessorCount * 2, trimOnFree)
        {
        }

        internal ObjectPool(Factory factory, int size, bool trimOnFree = true)
        {
            _factory = factory;
            _items = new Element[size - 1];
            TrimOnFree = trimOnFree;
        }

        internal ObjectPool(Func<ObjectPool<T>, T> factory, int size)
        {
            Debug.Assert(size >= 1);
            _factory = () => factory(this);
            _items = new Element[size - 1];
        }

        private T CreateInstance()
        {
            var inst = _factory();
            return inst;
        }

        internal T Allocate()
        {
            // PERF: Examine the first element. If that fails, AllocateSlow will look at the remaining elements.
            // Note that the initial read is optimistically not synchronized. That is intentional.
            // We will interlock only when we have a candidate. in a worst case we may miss some
            // recently returned objects. Not a big deal.
            var inst = _firstItem;
            if (inst == null || inst != Interlocked.CompareExchange(ref _firstItem, null, inst))
            {
                inst = AllocateSlow();
            }

            return inst;
        }

        private T AllocateSlow()
        {
            var items = _items;

            for (var i = 0; i < items.Length; i++)
            {
                // Note that the initial read is optimistically not synchronized. That is intentional.
                // We will interlock only when we have a candidate. in a worst case we may miss some
                // recently returned objects. Not a big deal.
                var inst = items[i].Value;
                if (inst != null)
                {
                    if (inst == Interlocked.CompareExchange(ref items[i].Value, null, inst))
                    {
                        return inst;
                    }
                }
            }

            return CreateInstance();
        }

        /// <summary>
        /// Returns objects to the pool.
        /// </summary>
        /// <remarks>
        /// Search strategy is a simple linear probing which is chosen for it cache-friendliness.
        /// Note that Free will try to store recycled objects close to the start thus statistically
        /// reducing how far we will typically search in Allocate.
        /// </remarks>
        internal void Free(T obj)
        {
            Validate(obj);
            if (_firstItem == null)
            {
                // Intentionally not using interlocked here.
                // In a worst case scenario two objects may be stored into same slot.
                // It is very unlikely to happen and will only mean that one of the objects will get collected.
                _firstItem = obj;
            }
            else
            {
                FreeSlow(obj);
            }
        }

        private void FreeSlow(T obj)
        {
            var items = _items;
            for (var i = 0; i < items.Length; i++)
            {
                if (items[i].Value == null)
                {
                    items[i].Value = obj;
                    break;
                }
            }
        }

        private void Validate(object obj)
        {
            Debug.Assert(obj != null, "freeing null?");

            Debug.Assert(_firstItem != obj, "freeing twice?");

            var items = _items;
            for (var i = 0; i < items.Length; i++)
            {
                var value = items[i].Value;
                if (value == null)
                {
                    return;
                }

                Debug.Assert(value != obj, "freeing twice?");
            }
        }
    }
}