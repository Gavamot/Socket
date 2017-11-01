using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Test
{
    public class Message
    {
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public int MessageType { get; set; }
        public Guid ObjectGuid { get; set; }
        public List<Object> Objects { get; set; } = new List<object>();
        public int Operation { get; set; }
        public Dictionary<int, string> Parameters { get; set; } = new Dictionary<int, string>();
        public string ParameterWeb { get; set; }
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
                    {0, "web"}, // Login - Admin
                    {1, "Nz����"}, // Password - Admin
                },

                RootObject = true,
            };
        }

        public static Message CreateTestMessage()
        {
            return new Message
            {
                ClassID = 2,
                ClassName = "CASCMessage",
                MessageType = GetMessageType(EASCMessagePath.eOnlyFoward, EASCMessage.eWebGetBrigadesInfo),
                ObjectGuid = new Guid("4c6498c7-ebb1-4249-b9c9-7cc28d01dc91"),
                Operation = (int)EASCOperation.eNoAccessOperation,
                Parameters = new Dictionary<int, string>
                {

                },
                ParameterWeb = JsonConvert.SerializeObject(new List<int>
                {
                   1, 2, 3
                }),
                RootObject = true

            };
        }
    }
}
