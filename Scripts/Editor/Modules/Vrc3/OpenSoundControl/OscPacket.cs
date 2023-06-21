#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3.OpenSoundControl
{
    public class OscPacket
    {
        public static IEnumerable<Message> GetMessages(byte[] oscData) => oscData[0] == '#' ? ParseBundle(oscData)._messages : new[] { Message.ParseMessage(oscData) };

        private readonly ulong _timeTag;
        private readonly IList<Message> _messages;

        internal OscPacket(ulong timeTag, IList<Message> messages)
        {
            _timeTag = timeTag;
            _messages = messages;
        }

        public byte[] GetBytes()
        {
            const string bundle = "#bundle";
            var intTagLen = AlignedStringLength(bundle);
            var tag = SetULong(_timeTag);

            var msgList = _messages.Select(message => message.GetBytes()).ToList();

            var i = 0;
            var output = new byte[intTagLen + tag.Length + msgList.Sum(x => x.Length + 4)];
            Encoding.ASCII.GetBytes(bundle).CopyTo(output, i);
            i += intTagLen;
            tag.CopyTo(output, i);
            i += tag.Length;

            foreach (var msg in msgList)
            {
                var size = SetInt(msg.Length);
                size.CopyTo(output, i);
                i += size.Length;
                msg.CopyTo(output, i);
                i += msg.Length;
            }

            return output;
        }

        private static OscPacket ParseBundle(byte[] bundle)
        {
            var messages = new List<Message>();
            var index = 0;
            var bundleTagString = Encoding.ASCII.GetString(SubArray(bundle, ref index, 8));
            var timeULong = GetULong(bundle, ref index);
            if (bundleTagString != "#bundle\0") throw new Exception("Not a bundle");
            while (index < bundle.Length) messages.Add(Message.ParseMessage(SubArray(bundle, ref index, GetInt(bundle, ref index))));
            return new OscPacket(timeULong, messages);
        }

        private static string GetAddress(byte[] msg, ref int index)
        {
            int i;
            for (i = index; i < msg.Length; i += 4)
            {
                if (msg[i] != ',') continue;
                if (i == 0) return "";
                break;
            }

            if (i >= msg.Length) throw new Exception("No comma found in OSC packet!");
            var addressString = Encoding.ASCII.GetString(SubArray(msg, ref index, i));
            return addressString.Replace("\0", "");
        }

        private static IEnumerable<char> GetTypes(byte[] msg, ref int index)
        {
            int i;
            for (i = index + 4; i < msg.Length; i += 4)
                if (msg[i - 1] == 0)
                    break;
            return Encoding.ASCII.GetChars(SubArray(msg, ref index, i - index));
        }

        private static string GetString(byte[] msg, ref int index)
        {
            int i;
            for (i = index + 4; i - 1 < msg.Length; i += 4)
                if (msg[i - 1] == 0)
                    break;
            return Encoding.ASCII.GetString(SubArray(msg, ref index, i - index)).Replace("\0", "");
        }

        private static int GetInt(IReadOnlyList<byte> msg, ref int index) => GetInternalInt(msg, index += 4);

        private static int GetInternalInt(IReadOnlyList<byte> msg, int index) => (msg[index - 4] << 24) + (msg[index - 3] << 16) + (msg[index - 2] << 8) + (msg[index - 1] << 0);

        private static byte[] GetBlob(byte[] msg, ref int index) => SubArray(msg, ref index, GetInt(msg, ref index));

        private static ulong GetULong(IReadOnlyList<byte> msg, ref int index) => GetInternalULong(msg, index += 8);

        private static ulong GetInternalULong(IReadOnlyList<byte> msg, int index) => ((ulong)msg[index - 8] << 56) + ((ulong)msg[index - 7] << 48) + ((ulong)msg[index - 6] << 40) + ((ulong)msg[index - 5] << 32) + ((ulong)msg[index - 4] << 24) + ((ulong)msg[index - 3] << 16) + ((ulong)msg[index - 2] << 8) + ((ulong)msg[index - 1] << 0);

        private static float GetFloat(IReadOnlyList<byte> msg, ref int index)
        {
            var reversed = new byte[4];
            reversed[3] = msg[index];
            reversed[2] = msg[index + 1];
            reversed[1] = msg[index + 2];
            reversed[0] = msg[index + 3];
            index += 4;
            return BitConverter.ToSingle(reversed, 0);
        }

        private static long GetLong(IReadOnlyList<byte> msg, ref int index)
        {
            var var = new byte[8];
            var[7] = msg[index];
            var[6] = msg[index + 1];
            var[5] = msg[index + 2];
            var[4] = msg[index + 3];
            var[3] = msg[index + 4];
            var[2] = msg[index + 5];
            var[1] = msg[index + 6];
            var[0] = msg[index + 7];
            index += 8;
            return BitConverter.ToInt64(var, 0);
        }

        private static double GetDouble(IReadOnlyList<byte> msg, ref int index)
        {
            var var = new byte[8];
            var[7] = msg[index];
            var[6] = msg[index + 1];
            var[5] = msg[index + 2];
            var[4] = msg[index + 3];
            var[3] = msg[index + 4];
            var[2] = msg[index + 5];
            var[1] = msg[index + 6];
            var[0] = msg[index + 7];
            index += 8;
            return BitConverter.ToDouble(var, 0);
        }

        private static char GetChar(IReadOnlyList<byte> msg, ref int index) => (char)msg[(index += 4) - 1];

        private static byte[] SetInt(int value) => BitConverter.GetBytes(value).Reverse().ToArray();

        private static byte[] SetLong(long value) => BitConverter.GetBytes(value).Reverse().ToArray();

        private static byte[] SetFloat(float value) => BitConverter.GetBytes(value).Reverse().ToArray();

        private static byte[] SetULong(ulong value) => BitConverter.GetBytes(value).Reverse().ToArray();

        private static byte[] SetDouble(double value) => BitConverter.GetBytes(value).Reverse().ToArray();

        private static byte[] SetString(string value)
        {
            var intLen = value.Length + (4 - value.Length % 4);
            if (intLen <= value.Length) intLen += 4;
            var msg = new byte[intLen];
            Encoding.ASCII.GetBytes(value).CopyTo(msg, 0);
            return msg;
        }

        private static byte[] SetBlob(byte[] value)
        {
            var msg = new byte[value.Length + 4 + (4 - (value.Length + 4) % 4)];
            SetInt(value.Length).CopyTo(msg, 0);
            value.CopyTo(msg, 4);
            return msg;
        }

        private static byte[] SetChar(char value)
        {
            var output = new byte[4];
            output[0] = 0;
            output[1] = 0;
            output[2] = 0;
            output[3] = (byte)value;
            return output;
        }

        private static T[] SubArray<T>(T[] sourceArray, ref int sourceIndex, int length)
        {
            var destinationArray = new T[length];
            Array.Copy(sourceArray, sourceIndex, destinationArray, 0, length);
            sourceIndex += length;
            return destinationArray;
        }

        private static int AlignedStringLength(string val)
        {
            var intLen = val.Length + (4 - val.Length % 4);
            return intLen <= val.Length ? intLen + 4 : intLen;
        }

        public static DateTime TimeTagToDateTime(ulong val) => val == 1 ? DateTime.Now : DateTime.Parse("1900-01-01 00:00:00").AddSeconds((uint)(val >> 32)).AddSeconds(TimeTagToFraction(val));

        private static double TimeTagToFraction(ulong val) => val == 1 ? 0.0 : (double)(uint)(val & 0x00000000FFFFFFFF) / 0xFFFFFFFF;

        public class Message
        {
            public readonly string Address;
            public readonly IList<object> Arguments;

            public Message(string address, IList<object> arguments)
            {
                Address = address;
                Arguments = arguments;
            }

            public Message(string address, object arg) : this(address, new[] { arg })
            {
            }

            public byte[] GetBytes()
            {
                var lists = new List<(IList<object>, int)>();

                var parts = new List<byte[]>();
                var currentList = Arguments;

                var typeString = ",";
                var i = 0;
                while (i < currentList.Count)
                {
                    switch (currentList[i])
                    {
                        case null:
                            typeString += "N";
                            break;
                        case int intArg:
                            typeString += "i";
                            parts.Add(SetInt(intArg));
                            break;
                        case string stringArg:
                            typeString += "s";
                            parts.Add(SetString(stringArg));
                            break;
                        case byte[] byteArg:
                            typeString += "b";
                            parts.Add(SetBlob(byteArg));
                            break;
                        case long longArg:
                            typeString += "h";
                            parts.Add(SetLong(longArg));
                            break;
                        case ulong ulongArg:
                            typeString += "t";
                            parts.Add(SetULong(ulongArg));
                            break;
                        case char charArg:
                            typeString += "c";
                            parts.Add(SetChar(charArg));
                            break;
                        case bool boolArg:
                            typeString += boolArg ? "T" : "F";
                            break;
                        case float floatArg:
                            var isFloatInfinity = float.IsPositiveInfinity(floatArg);
                            if (!isFloatInfinity) parts.Add(SetFloat(floatArg));
                            typeString += !isFloatInfinity ? "f" : "I";
                            break;
                        case double doubleArg:
                            var isDoubleInfinity = double.IsPositiveInfinity(doubleArg);
                            if (!isDoubleInfinity) parts.Add(SetDouble(doubleArg));
                            typeString += !isDoubleInfinity ? "d" : "I";
                            break;
                        case IList<object> objectArray:
                            typeString += "[";
                            lists.Add((currentList, i));
                            currentList = objectArray;
                            i = 0;
                            continue;
                        default: throw new Exception($"Unable to transmit values of type {currentList[i].GetType()}");
                    }

                    i++;
                    if (ReferenceEquals(currentList, Arguments) || i != currentList.Count) continue;

                    var intIndex = lists.Count - 1;
                    typeString += "]";
                    currentList = lists[intIndex].Item1;
                    i = lists[intIndex].Item2 + 1;
                    lists.RemoveAt(intIndex);
                }

                var addressLen = string.IsNullOrEmpty(Address) ? 0 : AlignedStringLength(Address);
                var typeLen = AlignedStringLength(typeString);
                var output = new byte[addressLen + typeLen + parts.Sum(x => x.Length)];

                i = 0;
                Encoding.ASCII.GetBytes(Address).CopyTo(output, i);
                i += addressLen;
                Encoding.ASCII.GetBytes(typeString).CopyTo(output, i);
                i += typeLen;

                foreach (var part in parts)
                {
                    part.CopyTo(output, i);
                    i += part.Length;
                }

                return output;
            }

            public static Message ParseMessage(byte[] msg)
            {
                var lists = new List<List<object>>();
                var arguments = new List<object>();

                var index = 0;
                var addressString = GetAddress(msg, ref index);
                var charEnumerable = GetTypes(msg, ref index);

                var commaParsed = false;

                foreach (var charType in charEnumerable)
                {
                    if (charType == ',' && !commaParsed)
                    {
                        commaParsed = true;
                        continue;
                    }

                    switch (charType)
                    {
                        case '\0':
                            break;
                        case 'i':
                            arguments.Add(GetInt(msg, ref index));
                            break;
                        case 'f':
                            arguments.Add(GetFloat(msg, ref index));
                            break;
                        case 's':
                            arguments.Add(GetString(msg, ref index));
                            break;
                        case 'b':
                            arguments.Add(GetBlob(msg, ref index));
                            while (index % 4 != 0) index++;
                            break;
                        case 'h':
                            arguments.Add(GetLong(msg, ref index));
                            break;
                        case 'd':
                            arguments.Add(GetDouble(msg, ref index));
                            break;
                        case 'c':
                            arguments.Add(GetChar(msg, ref index));
                            break;
                        case 'T':
                            arguments.Add(true);
                            break;
                        case 'F':
                            arguments.Add(false);
                            break;
                        case 'N':
                            arguments.Add(null);
                            break;
                        case 'I':
                            arguments.Add(double.PositiveInfinity);
                            break;
                        case '[':
                            lists.Add(arguments);
                            arguments = new List<object>();
                            break;
                        case ']':
                            var intIndex = lists.Count - 1;
                            var upList = lists[intIndex];
                            upList.Add(arguments);
                            lists.RemoveAt(intIndex);
                            arguments = upList;
                            break;
                        default: throw new Exception($"OSC type tag '{charType}' is unknown.");
                    }
                }

                return new Message(addressString, arguments);
            }
        }
    }
}
#endif