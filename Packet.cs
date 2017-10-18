using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;
using Xunit;

namespace Test
{

    public static class ArrayExt
    {
        public static int IndexOf(this byte[] arr, byte value)
        {
            return Array.IndexOf(arr, value);
        }
    }

    public class Packet
    {
        public const byte START = 0xC0;
        public const byte END = 0xC1;
        public const uint REQEST_ID = 1;
        public const short COREVERVION = 1;

        public const int CONFIRM_PACK_SIZE = 12;


        private void DeleteConfirmPacket(List<byte> data)
        {
            int endIndex = data.IndexOf(END);
            data.RemoveRange(0, endIndex + 1);
        }

        private void DeleteExcessBytes(List<byte> data)
        {
            int end = data.IndexOf(END) + 1;
            data.RemoveRange(end, data.Count - end);
        }

        public string ParceReceivedPacket(List<byte> data)
        {
            DeleteConfirmPacket(data);
            DeleteExcessBytes(data);
            var packet = BackChangeBytes(data);

            if (!Crc.IsEqualCheckSum(packet))
                throw new PacketParceException("crc eror");
            int startIndex = sizeof(uint) + sizeof(short);
            int length = packet.Count - startIndex;
            var res = packet.GetRange(startIndex, length);
            return Encoding.UTF8.GetString(res.ToArray());
        }

        List<byte> BackChangeBytes(List<byte> data)
        {
            int index = 1;
            var res = new List<byte>();;
            var ln = data.Count - 1;
            while (index < ln)
            {
                byte bt = data[index];
                if (bt == 0xDB)
                {
                    byte bt2 = data[index + 1];
                    if (bt2 == 0xDC) res.Add(START);
                    else if (bt2 == 0xDD) res.Add(0xDB);
                    else if (bt2 == 0xDE) res.Add(END);
                    else throw new PacketParceException("translit error");
                    index++;
                }
                else
                {
                    res.Add(data[index]);
                }
                index++;
            }
            return res;
        }

        public byte[] MakeSendPacket(string json)
        {
            var res = new List<byte>();

            res.AddRange(BitConverter.GetBytes(COREVERVION).Reverse());
            res.AddRange(BitConverter.GetBytes(REQEST_ID).Reverse());

            var jsonBytes = Encoding.UTF8.GetBytes(json);
            res.AddRange(jsonBytes);

            uint crc = Crc.GetCrc(res);
            var crcBytes = BitConverter.GetBytes(crc).Reverse();
            res.AddRange(crcBytes);

            FowardChangeBytes(res);

            res.Insert(0, START);
            res.Add(END);

            return res.ToArray();
        }

        void FowardChangeBytes(List<byte> data)
        {
            int index = 0;
            int length = data.Count;
            while (index < length)
            {
                byte bt = data[index];
                if (bt == START)
                {
                    data.RemoveAt(index);
                    data.Insert(index, 0xDC);
                    data.Insert(index, 0xDB);
                    index += 1;
                }
                else if (bt == 0xDB)
                {
                    data.RemoveAt(index);
                    data.Insert(index, 0xDD);
                    data.Insert(index, 0xDB);
                    index += 1;
                }
                else if (bt == END)
                {
                    data.RemoveAt(index);
                    data.Insert(index, 0xDE);
                    data.Insert(index, 0xDB);
                    index += 1;
                }
                index++;
            }
        }

    }

    public class PacketParceException : Exception
    {
        public PacketParceException(string message) : base(message)
        {
            
        }
    }
}
