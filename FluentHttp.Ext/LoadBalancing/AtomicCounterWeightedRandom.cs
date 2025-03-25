namespace FluentHttp.Ext.LoadBalancing
{
    public class AtomicCounterWeightedRandom<T>
    {
        private readonly List<T> _items = new List<T>();
        private readonly List<double> _weights = new List<double>();
        private long _totalWeight;

        public void Add(T item, double weight)
        {
            _items.Add(item);
            _weights.Add(weight);
            Interlocked.Add(ref _totalWeight, (long)weight);
        }

        public T? Next()
        {
            double randomValue = Random.Shared.NextDouble() * _totalWeight;
            double cumulativeWeight = 0;

            for (int i = 0; i < _items.Count; i++)
            {
                cumulativeWeight += _weights[i];
                if (randomValue < cumulativeWeight)
                {
                    return _items[i];
                }
            }
            return default;
        }
    }
}