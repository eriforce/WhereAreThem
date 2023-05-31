using System.Configuration;
using BenchmarkDotNet.Attributes;
using PureLib.Common;
using WhereAreThem.Model.Persistences;

namespace Benchmarks {
    [MemoryDiagnoser(false)]
    public class FormatBenchmarks {
        private readonly WhereAreThem.Model.Models.Folder _folder;
        private readonly BinaryProvider _provider = new();
        private readonly IPersistence _gzip = new GzipPersistence<BinaryProvider>();
        private readonly IPersistence _zstd = new ZstdPersistence<BinaryProvider>();

        public FormatBenchmarks() {
            string path = @"D:\Sync\Software\WhereAreThem\lists\FIRE-DESKTOP\D.Fixed.wat";

            _folder = _gzip.Load(path);
        }

        [Benchmark]
        public void Zstd() {
            _zstd.Save(_folder, "D:\\zstd");
        }

        [Benchmark]
        public void Gzip() {
            _gzip.Save(_folder, "D:\\gizp");
        }

        [Benchmark]
        public void ZstdLoad() {
            _zstd.Load("D:\\zstd1");
        }

        [Benchmark]
        public void GzipLoad() {
            _gzip.Load("D:\\gzip");
        }
    }
}
