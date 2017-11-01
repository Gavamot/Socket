﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;

namespace WebService
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
        public const int MAX_CONFIRM_PACK_SIZE = 20;

        public static readonly byte[] ConfirmationPackBytes = { 0xC0, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0xFB, 0xA5, 0xB8, 0x85, 0xC1 };

        public static int IndexOfConfirmationPack(byte[] data, int bytesRead)
        {
            return data.IndexOf(START) ;
        }

        private static void DeleteExcessBytes(List<byte> data)
        {
            int end = data.IndexOf(END) + 1;
            data.RemoveRange(end, data.Count - end);
        }

        static List<byte> BackChangeBytes(List<byte> data)
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

        static void FowardChangeBytes(List<byte> data)
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

        public static byte[] MakeSendPacket(string json)
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

        /// <summary>
        /// Делит полученные данные на пакеты
        /// </summary>
        /// <param name="data">Полученные данные</param>
        /// <returns>Пакеты</returns>
        public static List<List<byte>> SplitToPackets(List<byte> data)
        {
            var res = new List<List<byte>>();
            int startIndex = -1;
            for (int i = 0, max = data.Count; i < max; i++)
            {
                var b = data[i];
                if (b == START) startIndex = i;
                else if (startIndex != -1 && b == END)
                {
                    int counts = i + 1 - startIndex; 
                    if (counts >= CONFIRM_PACK_SIZE)
                        res.Add(data.GetRange(startIndex, i + 1 - startIndex));
                    startIndex = -1;
                } 
            }
            return res;
        }

        public static string ParceReceivedPacket(List<byte> data)
        {
            DeleteExcessBytes(data);
            var packet = BackChangeBytes(data);
            if (!Crc.IsEqualCheckSum(packet))
                throw new PacketParceException("crc eror");
            int startIndex = sizeof(uint) + sizeof(short);
            int length = packet.Count - startIndex;
            var res = packet.GetRange(startIndex, length);
            return Encoding.UTF8.GetString(res.ToArray());
        }

    }

    public class PacketParceException : Exception
    {
        public PacketParceException(string message) : base(message)
        {
            
        }
    }
}
