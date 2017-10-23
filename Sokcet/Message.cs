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
        public string Data { get; set; }
        public static int GetMessageType(EASCMessagePath mp, EASCMessage mt)
        {
            return (int)mp | (int)mt;
        }

        public static Message CreateAuthorizeMessage()
        {
            return new Message
            {
                ClassID = 2,
                ClassName = "CASCMessage",
                MessageType = GetMessageType(EASCMessagePath.eOnlyFoward, EASCMessage.eAutorizationMessage),
                ObjectGuid = new Guid("4c6498c7-ebb1-4249-b9c9-7cc28d01dc9d"),
                Operation = (int)EASCOperation.eNoAccessOperation,
                Parameters = new Dictionary<int, string>
                {
                    {0, "Admin"}, // Login - Admin
                    {1, "Nz����"}, // Password - Admin
                },
                RootObject = true,
            };
        }

        public static Message CreateGiveIve50ArchiveCodesInfoMessage(uint reqvestId)
        {
            return new Message
            {
                ClassID = 2,
                ClassName = "CASCMessage",
                MessageType = 29,
                ObjectGuid = new Guid("4c6498c7-ebb1-4249-b9c9-7cc28d01dc91"),
                Operation = 20,
                Parameters = new Dictionary<int, string>
                {
                    {0, reqvestId.ToString()},
                    {1, QDate.GetString(new DateTime(2017, 10, 18))},
                    {2, QDate.GetString(new DateTime(2017, 10, 19))}
                },
                RootObject = true
                
            };
        }
    }
}
