using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using com.rogusdev.UbJsonSharp;

namespace UbJsonSharpTests
{
    class UbJsonDeserializerTests
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
        private const decimal ULongValueAsDecimal = (decimal)ULongValue;
        private const decimal DecimalValue = 300.5m;
        private const char SmallCharValue = '*';

        private static readonly byte[]
            Int8Bytes = new byte[] { UbJsonCommon.Int8Marker, 42 },  // 42
            UInt8Bytes = new byte[] { UbJsonCommon.UInt8Marker, 200 },  // 200
            Int16Bytes = new byte[] { UbJsonCommon.Int16Marker, 0x7A, 0x02 },  // 31234
            Int32Bytes = new byte[] { UbJsonCommon.Int32Marker, 0x7E, 0x91, 0x61, 0x15 },  // 2123456789
            Int64Bytes = new byte[] { UbJsonCommon.Int64Marker, 0x7E, 0x9D, 0x07, 0x9C, 0x8F, 0x54, 0x5F, 0x15 },  // 9123456789123456789L
            Float32Bytes = new byte[] { UbJsonCommon.Float32Marker, 0x41, 0x46, 0x00, 0x00 },  // 12.375
            Float64Bytes = new byte[] { UbJsonCommon.Float64Marker, 0x40, 0x28, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00 },  // 12.375

            SmallCharBytes = new byte[] { UbJsonCommon.CharMarker, 0x2A },
            SmallCharStringBytes = new byte[] { UbJsonCommon.StringMarker, 0x69, 0x01, 0x2A },
            StringBytes = new byte[] { UbJsonCommon.StringMarker, 0x69, 0x05, 0x54, 0x65, 0x73, 0x74, 0x21 },  // "Test!"
            ULongBytes = new byte[] { UbJsonCommon.PrecisionMarker, 0x69, 0x13, 0x39, 0x32, 0x32, 0x33, 0x33, 0x37, 0x32, 0x30, 0x33, 0x36, 0x38, 0x35, 0x34, 0x37, 0x37, 0x35, 0x38, 0x30, 0x38 },
            DecimalBytes = new byte[] { UbJsonCommon.PrecisionMarker, 0x69, 0x05, 0x33, 0x30, 0x30, 0x2E, 0x35 },

        // FIXME -- add negatives for all number types as well!

            NullBytes = new[] { UbJsonCommon.NullMarker },
            NoopBytes = new[] { UbJsonCommon.NoopMarker },
            TrueBytes = new[] { UbJsonCommon.TrueMarker },
            FalseBytes = new[] { UbJsonCommon.FalseMarker };

        private IUbJsonDeserializer _deserializer;

        [SetUp]
        public void Setup ()
        {
            _deserializer = new UbJsonDeserializer();
        }

        #region "value type deserialization"
        
        [Test]
        public void ShouldDeserializeNull ()
        {
            Assert.That(_deserializer.Deserialize(NullBytes), Is.EqualTo(null));
        }

        [Test]
        public void ShouldDeserializeTrue ()
        {
            Assert.That(_deserializer.Deserialize(TrueBytes), Is.EqualTo(true));
        }

        [Test]
        public void ShouldDeserializeFalse ()
        {
            Assert.That(_deserializer.Deserialize(FalseBytes), Is.EqualTo(false));
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowOnDeserializeNoop ()
        {
            _deserializer.Deserialize(NoopBytes);
        }

        [Test]
        public void ShouldDeserializeInt8 ()
        {
            Assert.That(_deserializer.Deserialize(Int8Bytes), Is.EqualTo(Int8Value));
        }

        [Test]
        public void ShouldDeserializeNegativeInt8 ()
        {
            var bytes = new byte[Int8Bytes.Length];
            Int8Bytes.CopyTo(bytes, 0);
            bytes[1] = (byte)(256 - bytes[1]);
            Assert.That(_deserializer.Deserialize(bytes), Is.EqualTo(-Int8Value));
        }

        [Test]
        public void ShouldDeserializeUInt8 ()
        {
            Assert.That(_deserializer.Deserialize(UInt8Bytes), Is.EqualTo(UInt8Value));
        }

        [Test]
        public void ShouldDeserializeInt16 ()
        {
            Assert.That(_deserializer.Deserialize(Int16Bytes), Is.EqualTo(Int16Value));
        }

        [Test]
        public void ShouldDeserializeInt32 ()
        {
            Assert.That(_deserializer.Deserialize(Int32Bytes), Is.EqualTo(Int32Value));
        }
        
        [Test]
        public void ShouldDeserializeInt64 ()
        {
            Assert.That(_deserializer.Deserialize(Int64Bytes), Is.EqualTo(Int64Value));
        }

        [Test]
        public void ShouldDeserializeFloat32 ()
        {
            Assert.That(_deserializer.Deserialize(Float32Bytes), Is.EqualTo(Float32Value));
        }

        [Test]
        public void ShouldDeserializeFloat64 ()
        {
            Assert.That(_deserializer.Deserialize(Float64Bytes), Is.EqualTo(Float64Value));
        }

        [Test]
        public void ShouldDeserializeSmallCharToChar ()
        {
            Assert.That(_deserializer.Deserialize(SmallCharBytes), Is.EqualTo(SmallCharValue));
        }

        [Test]
        public void ShouldDeserializeString ()
        {
            Assert.That(_deserializer.Deserialize(StringBytes), Is.EqualTo(StringValue));
        }

        [Test]
        public void ShouldDeserializeULongToPrecision ()
        {
            Assert.That(_deserializer.Deserialize(ULongBytes), Is.EqualTo(ULongValueAsDecimal));
        }

        [Test]
        public void ShouldDeserializeDecimalToPrecision ()
        {
            Assert.That(_deserializer.Deserialize(DecimalBytes), Is.EqualTo(DecimalValue));
        }

        [Test]
        public void ShouldDeserializeZeroLenString ()
        {
            Assert.That(_deserializer.Deserialize(new byte[] { UbJsonCommon.StringMarker, 0x69, 0x00 }), Is.EqualTo(""));
        }

        [Test, ExpectedException(typeof(InvalidCastException))]
        public void ShouldThrowOnDeserializeNotLenString ()
        {
            _deserializer.Deserialize(new byte[] { UbJsonCommon.StringMarker, UbJsonCommon.CharMarker, 0x01, 0x41 });
        }

        [Test, ExpectedException(typeof(InvalidDataException))]
        public void ShouldThrowOnDeserializeTooLongString ()
        {
            _deserializer.Deserialize(new byte[] { UbJsonCommon.StringMarker, UbJsonCommon.Int64Marker, 0x7E, 0x9D, 0x07, 0x9C, 0x8F, 0x54, 0x5F, 0x15, 0x41 });
        }

        [Test, ExpectedException(typeof(InvalidDataException))]
        public void ShouldThrowOnDeserializeUnrecognizedType ()
        {
            _deserializer.Deserialize(new byte[] { 0x20 });
        }

        [Test]
        public void ShouldDeserializeStringThatIsActuallyNotTooLong ()
        {
            Assert.That(_deserializer.Deserialize(new byte[] { UbJsonCommon.StringMarker, UbJsonCommon.Int64Marker, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x41 }), Is.EqualTo("A"));
        }

        #endregion

        #region "container deserialization"

        [Test]
        public void ShouldDeserializeList ()
        {
            var list = new List<object>();
            list.Add(SmallCharValue);
            list.Add(Int64Value);

            var bytes = new List<byte>();
            bytes.Add(UbJsonCommon.ArrayStartMarker);
            bytes.AddRange(SmallCharBytes);
            bytes.AddRange(Int64Bytes);
            bytes.Add(UbJsonCommon.ArrayEndMarker);

            Assert.That(_deserializer.Deserialize(bytes.ToArray()), Is.EqualTo(list));
        }

        [Test]
        public void ShouldDeserializeEmptyList ()
        {
            var list = new List<object>();

            var bytes = new List<byte>();
            bytes.Add(UbJsonCommon.ArrayStartMarker);
            bytes.Add(UbJsonCommon.ArrayEndMarker);

            Assert.That(_deserializer.Deserialize(bytes.ToArray()), Is.EqualTo(list));
        }

        [Test]
        public void ShouldDeserializeListOfList ()
        {
            var list = new List<object>();
            var list2 = new List<object>();
            list2.Add(SmallCharValue);
            list.Add(list2);
            list.Add(Int64Value);

            var bytes = new List<byte>();
            bytes.Add(UbJsonCommon.ArrayStartMarker);
            bytes.Add(UbJsonCommon.ArrayStartMarker);
            bytes.AddRange(SmallCharBytes);
            bytes.Add(UbJsonCommon.ArrayEndMarker);
            bytes.AddRange(Int64Bytes);
            bytes.Add(UbJsonCommon.ArrayEndMarker);

            Assert.That(_deserializer.Deserialize(bytes.ToArray()), Is.EqualTo(list));
        }

        [Test]
        public void ShouldDeserializeListOfListOfList ()
        {
            var list = new List<object>();
            var list2 = new List<object>();
            var list3 = new List<object>();
            list3.Add(StringValue);
            list2.Add(SmallCharValue);
            list2.Add(list3);
            list.Add(list2);
            list.Add(Int64Value);

            var bytes = new List<byte>();
            bytes.Add(UbJsonCommon.ArrayStartMarker);
            bytes.Add(UbJsonCommon.ArrayStartMarker);
            bytes.AddRange(SmallCharBytes);
            bytes.Add(UbJsonCommon.ArrayStartMarker);
            bytes.AddRange(StringBytes);
            bytes.Add(UbJsonCommon.ArrayEndMarker);
            bytes.Add(UbJsonCommon.ArrayEndMarker);
            bytes.AddRange(Int64Bytes);
            bytes.Add(UbJsonCommon.ArrayEndMarker);

            Assert.That(_deserializer.Deserialize(bytes.ToArray()), Is.EqualTo(list));
        }

        [Test]
        public void ShouldDeserializeEmptyDictionary ()
        {
            var dict = new Dictionary<string, object>();

            var bytes = new List<byte>();
            bytes.Add(UbJsonCommon.ObjectStartMarker);
            bytes.Add(UbJsonCommon.ObjectEndMarker);

            Assert.That(_deserializer.Deserialize(bytes.ToArray()), Is.EqualTo(dict));
        }

        [Test]
        public void ShouldDeserializeDictionary ()
        {
            var dict = new Dictionary<string, object>();
            dict.Add(SmallCharValue.ToString(), Int64Value);

            var bytes = new List<byte>();
            bytes.Add(UbJsonCommon.ObjectStartMarker);
            bytes.AddRange(SmallCharBytes);  // really this should only be small char string -- but that's for draft 10! for now it's safe, so demo it
            bytes.AddRange(Int64Bytes);
            bytes.Add(UbJsonCommon.ObjectEndMarker);

            Assert.That(_deserializer.Deserialize(bytes.ToArray()), Is.EqualTo(dict));
        }

        [Test]
        public void ShouldDeserializeDictionaryOfDictionary ()
        {
            var dict = new Dictionary<string, object>();
            var dict2 = new Dictionary<string, object>();
            dict2.Add(SmallCharValue.ToString(), Int64Value);
            dict2.Add(StringValue, Int32Value);
            dict.Add(SmallCharValue.ToString(), dict2);

            var bytes = new List<byte>();
            bytes.Add(UbJsonCommon.ObjectStartMarker);
            bytes.AddRange(SmallCharStringBytes);
            bytes.Add(UbJsonCommon.ObjectStartMarker);
            bytes.AddRange(SmallCharStringBytes);
            bytes.AddRange(Int64Bytes);
            bytes.AddRange(StringBytes);
            bytes.AddRange(Int32Bytes);
            bytes.Add(UbJsonCommon.ObjectEndMarker);
            bytes.Add(UbJsonCommon.ObjectEndMarker);

            Assert.That(_deserializer.Deserialize(bytes.ToArray()), Is.EqualTo(dict));
        }

        [Test]
        public void ShouldDeserializeDictionaryOfDictionaryOfDictionary ()
        {
            var dict = new Dictionary<string, object>();
            var dict2 = new Dictionary<string, object>();
            var dict3 = new Dictionary<string, object>();
            dict3.Add(StringValue, Int64Value);
            dict2.Add(StringValue, dict3);
            dict2.Add(SmallCharValue.ToString(), Int32Value);
            dict.Add(SmallCharValue.ToString(), dict2);

            var bytes = new List<byte>();
            bytes.Add(UbJsonCommon.ObjectStartMarker);
            bytes.AddRange(SmallCharStringBytes);
            bytes.Add(UbJsonCommon.ObjectStartMarker);
            bytes.AddRange(StringBytes);
            bytes.Add(UbJsonCommon.ObjectStartMarker);
            bytes.AddRange(StringBytes);
            bytes.AddRange(Int64Bytes);
            bytes.Add(UbJsonCommon.ObjectEndMarker);
            bytes.AddRange(SmallCharStringBytes);
            bytes.AddRange(Int32Bytes);
            bytes.Add(UbJsonCommon.ObjectEndMarker);
            bytes.Add(UbJsonCommon.ObjectEndMarker);

            Assert.That(_deserializer.Deserialize(bytes.ToArray()), Is.EqualTo(dict));
        }

        #endregion

        #region "multi container deserialization"

        [Test]
        public void ShouldDeserializeListOfDictionary ()
        {
            var list = new List<object>();
            var dict = new Dictionary<string, object>();
            dict.Add(StringValue, Float32Value);
            list.Add(dict);
            list.Add(Int64Value);

            var bytes = new List<byte>();
            bytes.Add(UbJsonCommon.ArrayStartMarker);
            bytes.Add(UbJsonCommon.ObjectStartMarker);
            bytes.AddRange(StringBytes);
            bytes.AddRange(Float32Bytes);
            bytes.Add(UbJsonCommon.ObjectEndMarker);
            bytes.AddRange(Int64Bytes);
            bytes.Add(UbJsonCommon.ArrayEndMarker);

            Assert.That(_deserializer.Deserialize(bytes.ToArray()), Is.EqualTo(list));
        }

        [Test]
        public void ShouldDeserializeDictionaryOfList ()
        {
            var dict = new Dictionary<string, object>();
            var list = new List<object>();
            list.Add(Int64Value);
            dict.Add(SmallCharValue.ToString(), list);

            var bytes = new List<byte>();
            bytes.Add(UbJsonCommon.ObjectStartMarker);
            bytes.AddRange(SmallCharStringBytes);
            bytes.Add(UbJsonCommon.ArrayStartMarker);
            bytes.AddRange(Int64Bytes);
            bytes.Add(UbJsonCommon.ArrayEndMarker);
            bytes.Add(UbJsonCommon.ObjectEndMarker);

            Assert.That(_deserializer.Deserialize(bytes.ToArray()), Is.EqualTo(dict));
        }

        [Test]
        public void ShouldDeserializeDictionaryOfListOfList ()
        {
            var dict = new Dictionary<string, object>();
            var list = new List<object>();
            var list2 = new List<object>();
            list2.Add(Float32Value);
            list.Add(Int64Value);
            list.Add(list2);
            dict.Add(SmallCharValue.ToString(), list);

            var bytes = new List<byte>();
            bytes.Add(UbJsonCommon.ObjectStartMarker);
            bytes.AddRange(SmallCharStringBytes);
            bytes.Add(UbJsonCommon.ArrayStartMarker);
            bytes.AddRange(Int64Bytes);
            bytes.Add(UbJsonCommon.ArrayStartMarker);
            bytes.AddRange(Float32Bytes);
            bytes.Add(UbJsonCommon.ArrayEndMarker);
            bytes.Add(UbJsonCommon.ArrayEndMarker);
            bytes.Add(UbJsonCommon.ObjectEndMarker);

            Assert.That(_deserializer.Deserialize(bytes.ToArray()), Is.EqualTo(dict));
        }

        [Test]
        public void ShouldDeserializeDictionaryOfListOfDictionary ()
        {
            var dict = new Dictionary<string, object>();
            var dict2 = new Dictionary<string, object>();
            var list = new List<object>();
            dict2.Add(StringValue, Float32Value);
            list.Add(Int64Value);
            list.Add(dict2);
            dict.Add(SmallCharValue.ToString(), list);

            var bytes = new List<byte>();
            bytes.Add(UbJsonCommon.ObjectStartMarker);
            bytes.AddRange(SmallCharStringBytes);
            bytes.Add(UbJsonCommon.ArrayStartMarker);
            bytes.AddRange(Int64Bytes);
            bytes.Add(UbJsonCommon.ObjectStartMarker);
            bytes.AddRange(StringBytes);
            bytes.AddRange(Float32Bytes);
            bytes.Add(UbJsonCommon.ObjectEndMarker);
            bytes.Add(UbJsonCommon.ArrayEndMarker);
            bytes.Add(UbJsonCommon.ObjectEndMarker);

            Assert.That(_deserializer.Deserialize(bytes.ToArray()), Is.EqualTo(dict));
        }

        #endregion
    }
}
