using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Test
{
    [JsonConverter(typeof(MessageSerializer))]
    public class Message
    {
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public int MessageType { get; set; }
        public Guid ObjectGuid { get; set; }
        public List<Object> Objects { get; set; } = new List<object>();
        public int Operation { get; set; }
        public Dictionary<int, string> Parameters { get; set; } = new Dictionary<int, string>();
        public bool RootObject { get; set; }

        public static int GetMessageType(EASCMessagePath mp, EASCMessage mt)
        {
            return (int)mp | (int)mt;
        }

        public static Message GetAuthorizeMessage()
        {
            return new Message
            {
                ClassID = 2,
                ClassName = "CASCMessage",
                MessageType = GetMessageType(EASCMessagePath.eFowardBackAll, EASCMessage.eAutorizationMessage),
                ObjectGuid = new Guid("4c6498c7-ebb1-4249-b9c9-7cc28d01dc9d"),
                Operation = (int)EASCOperation.eNoAccessOperation,
                RootObject = true,
                Parameters = new Dictionary<int, string>
                {
                    {0, "AAAACgAAAAAKAEEAZABtAGkAbg=="}, // Login - Admin
                    {1, "AAAACgAAAAAMAE4Aev/9//3//f/9"}, // Password - Admin
                }
            };
        }
    }
}
