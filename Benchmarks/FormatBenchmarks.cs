using System.Configuration;
using BenchmarkDotNet.Attributes;
using PureLib.Common;
using WhereAreThem.Model.Persistences;

namespace Benchmarks {
    [MemoryDiagnoser(false)]
    public class FormatBenchmarks {
        private readonly WhereAreThem.Model.Models.Folder _folder;
        private readonly BinaryProvider _binV1 = new();
        private readonly BinaryProvider _binV2 = new();

        public FormatBenchmarks() {
            string path = @"D:\Sync\Software\WhereAreThem\lists\FIRE-DESKTOP\D.Fixed.wat";

            IPersistence persistence = new CompressedPersistence<BinaryProvider>();
            _folder = persistence.Load(path);
        }

        [Benchmark]
        public void BinV1() {
            using var stream = File.OpenWrite(@"d:\b1");
            _binV1.Save(_folder, stream);
        }

        [Benchmark]
        public void BinV2() {
            using var stream = File.OpenWrite(@"d:\b2");
            _binV2.Save(_folder, stream);
        }
    }
}
