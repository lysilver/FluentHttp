using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace FluentHttp.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net70)]
    [SimpleJob(RuntimeMoniker.Net80)]
    [RPlotExporter]
    public class ObjectBenchmark
    {
        [Params(1000000, 10000000)]
        public int N;

        private ObjectPool<HttpModel> objectPool2;
        private HttpModelPool objectPool1;

        [GlobalSetup]
        public void Setup()
        {
            objectPool2 = new ObjectPool<HttpModel>(() =>
            {
                return new HttpModel();
            }, 20);
            objectPool1 = new HttpModelPool(20);
        }

        [Benchmark]
        public HttpModel NativeHttpModel()
        {
            return new HttpModel();
        }

        [Benchmark]
        public HttpModel PoolHttpModel()
        {
            HttpModel model = null;
            try
            {
                model = objectPool2.Allocate();
                return model;
            }
            finally
            {
                objectPool2.Free(model);
            }
        }

        [Benchmark]
        public HttpModel PoolHttpModel1()
        {
            int index = -1;
            try
            {
                HttpModel model = objectPool1.Get(out index);
                return model;
            }
            finally
            {
                objectPool1.Free(index);
            }
        }
    }
}