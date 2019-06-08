// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using MicroBenchmarks;
using Newtonsoft.Json;

namespace System.Text.Json.Tests
{
    [BenchmarkCategory(Categories.CoreFX, Categories.JSON)]
    public class Perf_Base64
    {
        private MemoryStream _memoryStream;
        private byte[] _dataWithNoEscaping;
        private byte[] _dataWithEscaping;

        [Params(100, 1000)]
        public int NumberOfBytes { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _memoryStream = new MemoryStream();

            // Results in a number of A plus padding
            _dataWithNoEscaping = new byte[NumberOfBytes];

            // Results in a lot + and /
            _dataWithEscaping = Enumerable.Range(0, NumberOfBytes)
                .Select(i => i % 2 == 0 ? 0xFB : 0xFF)
                .Select(i => (byte)i)
                .ToArray();
        }

        [Benchmark]
        public void WriteByteArrayAsBase64_NoEscaping() => WriteByteArrayAsBase64Core(_dataWithNoEscaping);

        [Benchmark]
        public void WriteByteArrayAsBase64_HeavyEscaping() => WriteByteArrayAsBase64Core(_dataWithEscaping);

        [Benchmark]
        public void WriteByteArrayAsBase64_NoEscaping_JsonNet() => WriteByteArrayAsBase64Core_JsonNet(_dataWithNoEscaping);

        [Benchmark]
        public void WriteByteArrayAsBase64_HeavyEscaping_JsonNet() => WriteByteArrayAsBase64Core_JsonNet(_dataWithEscaping);

        private void WriteByteArrayAsBase64Core(byte[] data)
        {
            _memoryStream.Position = 0;
            var json = new Utf8JsonWriter(_memoryStream);
            json.WriteBase64StringValue(data);
            json.Flush();
        }

        private void WriteByteArrayAsBase64Core_JsonNet(byte[] data)
        {
            _memoryStream.Position = 0;
            var sw = new StreamWriter(_memoryStream);
            var json = new JsonTextWriter(sw);
            json.WriteValue(data);
            json.Flush();
        }
    }
}
