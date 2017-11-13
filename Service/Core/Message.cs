using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Service.Services;

namespace Service.Core
{
    public class Message
    {
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public int MessageType { get; set; }
        public string ObjectGuid { get; set; }
        public Object[] Objects { get; set; } = new Object[0];
        public int Operation { get; set; }
        public string[] Parameters { get; set; } = new string[0];
        public string ParameterWeb { get; set; }
        public bool RootObject { get; set; } = true;

        public static Message GetMessageromBytes(byte[] bytes)
        {
            string jsonStr = Encoding.UTF8.GetString(bytes);
            var resArr = JsonConvert.DeserializeObject<Message[]>(jsonStr);
            return resArr[0];
        }

        public static int GetMessageType(EASCMessagePath mp, EASCMessage mt)
        {
            return (int)mp | (int)mt;
        }

        public static Message CreateStandartMessage(EASCMessage messageType)
        {
            return new Message
            {
                ClassID = 2,
                ClassName = "CASCMessage",
                MessageType = Message.GetMessageType(EASCMessagePath.eOnlyFoward, messageType),
                ObjectGuid = "4c6498c7-ebb1-4249-b9c9-7cc28d01dc91",
                Operation = (int)EASCOperation.eNoAccessOperation,
                ParameterWeb = "",
                RootObject = true
            };
        }


        //public static Message CreateAuthorizeMessage()
        //{
        //    return new Message
        //    {
        //        ClassID = 2,
        //        ClassName = "CASCMessage",
        //        MessageType = GetMessageType(EASCMessagePath.eOnlyFoward, EASCMessage.eAutorizationMessage),
        //        ObjectGuid = "4c6498c7-ebb1-4249-b9c9-7cc28d01dc9d",
        //        Operation = (int)EASCOperation.eNoAccessOperation,
        //        //Parameters = new Dictionary<int, string>
        //        //{
        //        //    {0, "web"}, // Login - Admin
        //        //    {1, "Nz����"}, // Password - Admin
        //        //},

        //        RootObject = true,
        //    };
        //}

        //public static Message CreateAscGetBrigadesInfoMessage()
        //{
        //    return new Message
        //    {
        //        ClassID = 2,
        //        ClassName = "CASCMessage",
        //        MessageType = GetMessageType(EASCMessagePath.eOnlyFoward, EASCMessage.eWebGetBrigadesInfo),
        //        ObjectGuid = "4c6498c7-ebb1-4249-b9c9-7cc28d01dc91",
        //        Operation = (int)EASCOperation.eNoAccessOperation,
        //        ParameterWeb = JsonConvert.SerializeObject(new List<int>
        //            {
        //               1, 2, 3
        //            }),
        //        RootObject = true
        //    };
        //}
    }
}
