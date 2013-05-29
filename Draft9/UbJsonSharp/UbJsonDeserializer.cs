using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace com.rogusdev.UbJsonSharp
{
    public class UbJsonDeserializer : IUbJsonDeserializer
    {
        public object Deserialize (byte[] bytes)
        {
            var pos = 0;
            return Deserialize(bytes, ref pos);
        }

        public object Deserialize (byte[] bytes, ref int pos)
        {
            var type = bytes[pos++];  // account for having just read the type byte

            switch (type)
            {
                case UbJsonCommon.NoopMarker:
                    // the no-op message is purely for the communication protocol and should be handled at that layer, not here
                    throw new InvalidOperationException("No-op has no data representation!");

                case UbJsonCommon.NullMarker:
                case UbJsonCommon.TrueMarker:
                case UbJsonCommon.FalseMarker:
                case UbJsonCommon.Int8Marker:
                case UbJsonCommon.UInt8Marker:
                case UbJsonCommon.Int16Marker:
                case UbJsonCommon.Int32Marker:
                case UbJsonCommon.Int64Marker:
                case UbJsonCommon.Float32Marker:
                case UbJsonCommon.Float64Marker:
                case UbJsonCommon.PrecisionMarker:
                case UbJsonCommon.StringMarker:
                case UbJsonCommon.CharMarker:
                case UbJsonCommon.ArrayStartMarker:
                case UbJsonCommon.ArrayEndMarker:
                case UbJsonCommon.ObjectStartMarker:
                case UbJsonCommon.ObjectEndMarker:
                    var val = Deserialize(type, bytes, ref pos);
                    return UpdateCurrentContainer(val, bytes, ref pos);

                default:
                    throw new InvalidDataException(string.Format("Unrecognized UBJSON value type: {0} = {1}", bytes[0], bytes));
            }
        }

        private object Deserialize (byte type, byte[] bytes, ref int pos)
        {
            object val;
            int len;  // for those types that have variable length

            switch (type)
            {
                case UbJsonCommon.NullMarker:
                    return null;
                case UbJsonCommon.TrueMarker:
                    return true;
                case UbJsonCommon.FalseMarker:
                    return false;
                case UbJsonCommon.Int8Marker:
                    val = (sbyte)bytes[pos];
                    pos++;
                    break;
                case UbJsonCommon.UInt8Marker:
                    val = (byte)bytes[pos];
                    pos++;
                    break;
                case UbJsonCommon.Int16Marker:
                    val = (short)GetBigEndianNumber(bytes, UbJsonCommon.Int16Size, pos);
                    pos += UbJsonCommon.Int16Size;
                    break;
                case UbJsonCommon.Int32Marker:
                    val = (int)GetBigEndianNumber(bytes, UbJsonCommon.Int32Size, pos);
                    pos += UbJsonCommon.Int32Size;
                    break;
                case UbJsonCommon.Int64Marker:
                    val = (long)GetBigEndianNumber(bytes, UbJsonCommon.Int64Size, pos);
                    pos += UbJsonCommon.Int64Size;
                    break;
                case UbJsonCommon.Float32Marker:
                    val = GetBigEndianFloat(bytes, pos);
                    pos += UbJsonCommon.Float32Size;
                    break;
                case UbJsonCommon.Float64Marker:
                    val = GetBigEndianDouble(bytes, pos);
                    pos += UbJsonCommon.Float64Size;
                    break;
                case UbJsonCommon.PrecisionMarker:
                    len = GetLength(bytes, ref pos);
                    val = Convert.ToDecimal(Encoding.UTF8.GetString(bytes, pos, len));
                    pos += len;
                    break;
                case UbJsonCommon.StringMarker:
                    len = GetLength(bytes, ref pos);
                    val = Encoding.UTF8.GetString(bytes, pos, len);
                    pos += len;
                    break;
                case UbJsonCommon.CharMarker:
                    val = (char)bytes[pos];
                    pos++;
                    break;

                case UbJsonCommon.ArrayStartMarker:
                    PushCurrentContainer(new List<object>(), null);
                    // updating the final list is done on the closing marker
                    return Deserialize(bytes, ref pos);
                case UbJsonCommon.ArrayEndMarker:
                    val = _list;
                    PopCurrentContainer();
                    break;
                case UbJsonCommon.ObjectStartMarker:
                    PushCurrentContainer(null, new Dictionary<string, object>());
                    // updating the final dictionary is done on the closing marker
                    return Deserialize(bytes, ref pos);
                case UbJsonCommon.ObjectEndMarker:
                    val = _dict;
                    PopCurrentContainer();
                    break;

                default:
                    throw new InvalidOperationException("Getting here means the internal usage of this function is busted");
            }

            return val;
        }

        #region container maintenance

        // currently active container we're adding to
        //  without a generic type on IList/IDictionary, we can allow using value types as well as reference types for STC array/object
        private IList _list;
        private IDictionary _dict;
        private object _dictKey;

        // stack of containers we might be working on underneath
        private readonly Stack<IList> _lists = new Stack<IList>();
        private readonly Stack<IDictionary> _dicts = new Stack<IDictionary>();
        private readonly Stack<object> _dictKeys = new Stack<object>();

        private void PushCurrentContainer (IList list, IDictionary dict)
        {
            _lists.Push(_list);
            _list = list;

            _dicts.Push(_dict);
            _dictKeys.Push(_dictKey);
            _dict = dict;
            _dictKey = null;
        }

        private void PopCurrentContainer ()
        {
            _list = _lists.Count > 0 ? _lists.Pop() : null;

            _dict = _dicts.Count > 0 ? _dicts.Pop() : null;
            _dictKey = _dictKeys.Count > 0 ? _dictKeys.Pop() : null;
        }

        private object UpdateCurrentContainer (object val, byte[] bytes, ref int pos)
        {
            if (_list != null)
            {
                _list.Add(val);
                // keep deserializing this list
                return Deserialize(bytes, ref pos);
            }

            if (_dict != null)
            {
                if (_dictKey != null)
                {
                    _dict.Add(_dictKey.ToString(), val);
                    _dictKey = null;
                }
                else _dictKey = val;

                // keep deserializing this dictionary
                return Deserialize(bytes, ref pos);
            }

            // no list or dictionary being processed, this value is terminal, so send it back already
            return val;
        }

        #endregion

        #region processing data values

        private int GetLength (byte[] bytes, ref int pos)
        {
            long length;  // so we can make sure a valid length was given
            var type = bytes[pos++];  // get next type marker to get just value without putting it in current list or dictionary
            var value = Deserialize(type, bytes, ref pos);

            if (value is sbyte) length = (sbyte)value;
            else if (value is short) length = (short)value;
            else if (value is int) length = (int)value;
            else if (value is long) length = (long)value;
            else throw new InvalidCastException(string.Format("Unrecognized length size type: {0}!", value));

            // NOTE: this means only byte lengths up to max signed int32 byte length strings are accepted, not max signed int64, which is ostensiably part of the official UBJSON spec!
            //  http://stackoverflow.com/questions/3106945/are-c-sharp-strings-and-other-net-apis-limited-to-2gb-in-size
            if (length > int.MaxValue)
                throw new InvalidDataException(".NET only allows strings that are up to 2gb number of characters, so to be safe, this library maxes out at that number of bytes");

            return (int)length;
        }

        // TODO -- run time tests to figure out which is faster: BitConverter.ToInt* plus Array.Reverse or manually byte manipulating?
        private static long GetBigEndianNumber (byte[] bytes, byte len, int start)
        {
            var num = 0L;
            for (var i = 0; i < len; i++)
                num += (long)bytes[start + i] << (UbJsonCommon.BitsInByte * (len - i - 1));
            return num;
        }

        private static float GetBigEndianFloat (byte[] bytes, int pos)
        {
            var value = BitConverter.ToSingle(bytes, pos);

            if (BitConverter.IsLittleEndian)
            {
                var b = BitConverter.GetBytes(value);
                Array.Reverse(b);
                return BitConverter.ToSingle(b, 0);
            }

            return value;
        }

        private static double GetBigEndianDouble (byte[] bytes, int pos)
        {
            var value = BitConverter.ToDouble(bytes, pos);

            if (BitConverter.IsLittleEndian)
            {
                var b = BitConverter.GetBytes(value);
                Array.Reverse(b);
                return BitConverter.ToDouble(b, 0);
            }

            return value;
        }

        #endregion
    }
}
