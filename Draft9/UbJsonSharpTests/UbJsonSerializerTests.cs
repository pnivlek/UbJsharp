using System.Collections.Generic;
using NUnit.Framework;
using com.rogusdev.UbJsonSharp;

namespace UbJsonSharpTests
{
    class UbJsonSerializerTests
    {
        private const sbyte Int8Value = 42;
        private const byte UInt8Value = 200;
        private const short Int16Value = 31234;
        private const int Int32Value = 2123456789;
        private const long Int64Value = 9123456789123456789L;
        // http://en.wikipedia.org/wiki/Single-precision_floating-point_format
        // http://www.binaryconvert.com/convert_float.html
        private const float Float32Value = 12.375F;
        // http://en.wikipedia.org/wiki/Double_precision
        // http://www.binaryconvert.com/convert_double.html
        private const double Float64Value = 12.375D;
        private const string StringValue = "Test!";
        private const ulong ULongValue = 9223372036854775808;
        private const decimal DecimalValue = 300.5m;
        private const char SmallCharValue = '*';
        private const char BigCharValue = '\u30A2';  // // katakana A char

        private static readonly byte[]
            Int8Bytes = new byte[] { UbJsonCommon.Int8Marker, 42 },  // 42
            UInt8Bytes = new byte[] { UbJsonCommon.UInt8Marker, 200 },  // 200
            Int16Bytes = new byte[] { UbJsonCommon.Int16Marker, 0x7A, 0x02 },  // 31234
            Int32Bytes = new byte[] { UbJsonCommon.Int32Marker, 0x7E, 0x91, 0x61, 0x15 },  // 2123456789
            Int64Bytes = new byte[] { UbJsonCommon.Int64Marker, 0x7E, 0x9D, 0x07, 0x9C, 0x8F, 0x54, 0x5F, 0x15 },  // 9123456789123456789L
            Float32Bytes = new byte[] { UbJsonCommon.Float32Marker, 0x41, 0x46, 0x00, 0x00 },  // 12.375
            Float64Bytes = new byte[] { UbJsonCommon.Float64Marker, 0x40, 0x28, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00 },  // 12.375

            SmallCharBytes = new byte[] { UbJsonCommon.CharMarker, 0x2A },
            BigCharBytes = new byte[] { UbJsonCommon.Int16Marker, 0x30, 0xA2 },
            StringBytes = new byte[] { UbJsonCommon.StringMarker, 0x69, 0x05, 0x54, 0x65, 0x73, 0x74, 0x21 },  // "Test!"
            ULongBytes = new byte[] { UbJsonCommon.PrecisionMarker, 0x69, 0x13, 0x39, 0x32, 0x32, 0x33, 0x33, 0x37, 0x32, 0x30, 0x33, 0x36, 0x38, 0x35, 0x34, 0x37, 0x37, 0x35, 0x38, 0x30, 0x38 },
            DecimalBytes = new byte[] { UbJsonCommon.PrecisionMarker, 0x69, 0x05, 0x33, 0x30, 0x30, 0x2E, 0x35 },

        // FIXME -- add negatives for all number types as well!

            NullBytes = new[] { UbJsonCommon.NullMarker },
            NoopBytes = new[] { UbJsonCommon.NoopMarker },
            TrueBytes = new[] { UbJsonCommon.TrueMarker },
            FalseBytes = new[] { UbJsonCommon.FalseMarker };

        private IUbJsonSerializer _serializer;

        [SetUp]
        public void Setup ()
        {
            _serializer = new UbJsonSerializer();
        }

        [Test]
        public void ShouldSerializeNull ()
        {
            Assert.That(_serializer.Serialize(null), Is.EquivalentTo(NullBytes));
        }

        [Test]
        public void ShouldSerializeTrue ()
        {
            Assert.That(_serializer.Serialize(true), Is.EquivalentTo(TrueBytes));
        }

        [Test]
        public void ShouldSerializeFalse ()
        {
            Assert.That(_serializer.Serialize(false), Is.EquivalentTo(FalseBytes));
        }

        [Test]
        public void ShouldSerializeNoop ()
        {
            Assert.That(_serializer.SerializeNoop(), Is.EquivalentTo(NoopBytes));
        }

        [Test]
        public void ShouldSerializeInt8 ()
        {
            Assert.That(_serializer.Serialize(Int8Value), Is.EquivalentTo(Int8Bytes));
        }

        [Test]
        public void ShouldSerializeNegativeInt8 ()
        {
            var bytes = new byte[Int8Bytes.Length];
            Int8Bytes.CopyTo(bytes, 0);
            bytes[1] = (byte)(256 - bytes[1]);
            Assert.That(_serializer.Serialize(-Int8Value), Is.EquivalentTo(bytes));
        }

        [Test]
        public void ShouldSerializeUInt8 ()
        {
            Assert.That(_serializer.Serialize(UInt8Value), Is.EquivalentTo(UInt8Bytes));
        }

        [Test]
        public void ShouldSerializeInt16 ()
        {
            Assert.That(_serializer.Serialize(Int16Value), Is.EquivalentTo(Int16Bytes));
        }

        [Test]
        public void ShouldSerializeInt32 ()
        {
            Assert.That(_serializer.Serialize(Int32Value), Is.EquivalentTo(Int32Bytes));
        }

        [Test]
        public void ShouldSerializeUShortToInt32 ()
        {
            Assert.That(_serializer.Serialize((ushort)50000), Is.EquivalentTo(new byte[] { UbJsonCommon.Int32Marker, 0x00, 0x00, 0xC3, 0x50 }));
        }

        [Test]
        public void ShouldSerializeInt64 ()
        {
            Assert.That(_serializer.Serialize(Int64Value), Is.EquivalentTo(Int64Bytes));
        }

        [Test]
        public void ShouldSerializeUIntToInt64 ()
        {
            Assert.That(_serializer.Serialize((uint)3123456789), Is.EquivalentTo(new byte[] { UbJsonCommon.Int64Marker, 0x00, 0x00, 0x00, 0x00, 0xBA, 0x2C, 0x2B, 0x15 }));
        }

        [Test]
        public void ShouldSerializeSmallLongToInt8 ()
        {
            Assert.That(_serializer.Serialize((long)Int8Value), Is.EquivalentTo(Int8Bytes));
        }

        [Test]
        public void ShouldSerializeBigEndianBytes ()
        {
            // http://msdn.microsoft.com/en-us/library/system.bitconverter.aspx
            //  -- remarks on byte order / endianness with examples
            Assert.That(_serializer.Serialize(1234567890), Is.EquivalentTo(new byte[] { UbJsonCommon.Int32Marker, 0x49, 0x96, 0x02, 0xD2 }));
            Assert.That(_serializer.Serialize(12345678), Is.EquivalentTo(new byte[] { UbJsonCommon.Int32Marker, 0x00, 0xBC, 0x61, 0x4E }));
        }

        [Test]
        public void ShouldSerializeFloat32 ()
        {
            Assert.That(_serializer.Serialize(Float32Value), Is.EquivalentTo(Float32Bytes));
        }

        [Test]
        public void ShouldSerializeFloat64 ()
        {
            Assert.That(_serializer.Serialize(Float64Value), Is.EquivalentTo(Float64Bytes));
        }

        [Test]
        public void ShouldSerializeSmallCharToChar ()
        {
            Assert.That(_serializer.Serialize(SmallCharValue), Is.EquivalentTo(SmallCharBytes));
        }

        [Test]
        public void ShouldSerializeBigCharToInt16 ()
        {
            Assert.That(_serializer.Serialize(BigCharValue), Is.EquivalentTo(BigCharBytes));
        }

        [Test]
        public void ShouldSerializeString ()
        {
            Assert.That(_serializer.Serialize(StringValue), Is.EquivalentTo(StringBytes));
        }

        [Test]
        public void ShouldSerializeULongToPrecision ()
        {
            Assert.That(_serializer.Serialize(ULongValue), Is.EquivalentTo(ULongBytes));
        }

        [Test]
        public void ShouldSerializeDecimalToPrecision ()
        {
            Assert.That(_serializer.Serialize(DecimalValue), Is.EquivalentTo(DecimalBytes));
        }

        [Test]
        public void ShouldSerializeConsistentDictionaryToStcObject ()
        {
            var dict = new Dictionary<object, object>();
            dict.Add(Int8Value, Int64Value);

            var bytes = new List<byte>();
            bytes.Add(UbJsonCommon.ObjectStartMarker);
            bytes.AddRange(Int8Bytes);
            bytes.AddRange(Int64Bytes);
            bytes.Add(UbJsonCommon.ObjectEndMarker);

            Assert.That(_serializer.Serialize(dict), Is.EquivalentTo(bytes));
        }

        [Test]
        public void ShouldSerializeConsistentDictionaryOfFixedTypeToStcObject ()
        {
            var dict = new Dictionary<sbyte, long>();
            dict.Add(Int8Value, Int64Value);

            var bytes = new List<byte>();
            bytes.Add(UbJsonCommon.ObjectStartMarker);
            bytes.AddRange(Int8Bytes);
            bytes.AddRange(Int64Bytes);
            bytes.Add(UbJsonCommon.ObjectEndMarker);

            Assert.That(_serializer.Serialize(dict), Is.EquivalentTo(bytes));
        }

        [Test]
        public void ShouldSerializeConsistentDictionaryOfFixedTypeToStcObjectOfSmallestFitType ()
        {
            var dict = new Dictionary<sbyte, long>();
            dict.Add(Int8Value, Int8Value);

            var bytes = new List<byte>();
            bytes.Add(UbJsonCommon.ObjectStartMarker);
            bytes.AddRange(Int8Bytes);
            bytes.AddRange(Int8Bytes);
            bytes.Add(UbJsonCommon.ObjectEndMarker);

            Assert.That(_serializer.Serialize(dict), Is.EquivalentTo(bytes));
        }

        [Test]
        public void ShouldSerializeDictionary ()
        {
            var dict = new Dictionary<object, object>();
            dict.Add(Int8Value, Int64Value);
            dict.Add(Int8Value + 1, Int32Value);

            var bytes = new List<byte>();
            bytes.Add(UbJsonCommon.ObjectStartMarker);
            bytes.AddRange(Int8Bytes);
            bytes.AddRange(Int64Bytes);
            bytes.AddRange(Int8Bytes);
            bytes[1 + Int8Bytes.Length + Int64Bytes.Length + 1] += 1;
            bytes.AddRange(Int32Bytes);
            bytes.Add(UbJsonCommon.ObjectEndMarker);

            Assert.That(_serializer.Serialize(dict), Is.EquivalentTo(bytes));
        }

        [Test]
        public void ShouldSerializeDictionaryOfFixedType ()
        {
            var dict = new Dictionary<sbyte, long>();
            dict.Add(Int8Value, Int64Value);
            dict.Add(Int8Value + 1, Int32Value);

            var bytes = new List<byte>();
            bytes.Add(UbJsonCommon.ObjectStartMarker);
            bytes.AddRange(Int8Bytes);
            bytes.AddRange(Int64Bytes);
            bytes.AddRange(Int8Bytes);
            bytes[1 + Int8Bytes.Length + Int64Bytes.Length + 1] += 1;
            bytes.AddRange(Int32Bytes);
            bytes.Add(UbJsonCommon.ObjectEndMarker);

            Assert.That(_serializer.Serialize(dict), Is.EquivalentTo(bytes));
        }

        [Test]
        public void ShouldSerializeDictionaryOfDictionary ()
        {
            var dict = new Dictionary<object, object>();
            var dict2 = new Dictionary<object, object>();
            dict2.Add(Int8Value, Int64Value);
            dict2.Add(Int8Value + 1, Int32Value);
            dict.Add(Int8Value, dict2);

            var bytes = new List<byte>();
            bytes.Add(UbJsonCommon.ObjectStartMarker);
            bytes.AddRange(Int8Bytes);
            bytes.Add(UbJsonCommon.ObjectStartMarker);
            bytes.AddRange(Int8Bytes);
            bytes.AddRange(Int64Bytes);
            bytes.AddRange(Int8Bytes);
            bytes[2 + Int8Bytes.Length + Int8Bytes.Length + Int64Bytes.Length + 1] += 1;
            bytes.AddRange(Int32Bytes);
            bytes.Add(UbJsonCommon.ObjectEndMarker);
            bytes.Add(UbJsonCommon.ObjectEndMarker);

            Assert.That(_serializer.Serialize(dict), Is.EquivalentTo(bytes));
        }
    }
}
