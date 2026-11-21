using BenchmarkDotNet.Attributes;

namespace FluentHttp.Benchmark.Benchmarks
{
    [MemoryDiagnoser]
    [RPlotExporter]
    [SimpleJob]
    public class ObjectBenchmark
    {

        [Params(100, 1000)]
        public int N;

        private ObjectPool<HttpModel> objectPool2;
        private HttpModelPool objectPool1;
        private ObjectPool3<HttpModel> objectPool3;

        [GlobalSetup]
        public void Setup()
        {
            objectPool2 = new ObjectPool<HttpModel>(() =>
            {
                return new HttpModel();
            }, 20);
            objectPool1 = new HttpModelPool(20);
            objectPool3 = new ObjectPool3<HttpModel>(() =>
            {
                return new HttpModel();
            }, null, 20);
        }

        [Benchmark]
        public void NativeHttpModel()
        {
            for (int i = 0; i < N; i++)
            {
                _ = new HttpModel();
            }
        }

        [Benchmark]
        public void PoolHttpModel2()
        {
            for (int i = 0; i < N; i++)
            {
                HttpModel model = null;
                try
                {
                    model = objectPool2.Allocate();
                }
                finally
                {
                    objectPool2.Free(model);
                }
            }

        }

        [Benchmark]
        public void PoolHttpModel1()
        {
            for (int i = 0; i < N; i++)
            {
                int index = -1;
                try
                {
                    HttpModel model = objectPool1.Get(out index);
                }
                finally
                {
                    objectPool1.Free(index);
                }
            }

        }

        [Benchmark]
        public void PoolHttpModel3()
        {
            for (int i = 0; i < N; i++)
            {
                int index = -1;
                HttpModel model = null;
                try
                {
                    model = objectPool3.Get(out index);
                }
                finally
                {
                    objectPool3.Free(model, index);
                }
            }
        }
    }
}