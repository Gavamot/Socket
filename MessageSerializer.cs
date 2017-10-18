using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Test
{
    class MessageSerializer : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var message = value as Message;
            MessageQt messageQt = new MessageQt(message);
            var strBuilder = new StringBuilder("[");
            strBuilder.Append(JsonConvert.SerializeObject(messageQt));
            strBuilder.Append("]");
            writer.WriteRaw(strBuilder.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Message).IsAssignableFrom(objectType);
        }

        private class MessageQt
        {
            public MessageQt(Message message)
            {
                ClassID = message.ClassID;
                ClassName = message.ClassName;
                MessageType = message.MessageType;
                Operation = message.Operation;
                RootObject = message.RootObject;
                Objects = message.Objects;
                ObjectGuid =$"{{{message.ObjectGuid}}}" ;
                Parameters = message.Parameters.Select(x => new KeyValue(x.Key, x.Value)).ToList();
            }

            public int ClassID { get; set; }
            public string ClassName { get; set; }
            public int MessageType { get; set; }
            public string ObjectGuid { get; set; }
            public List<Object> Objects { get; set; } = new List<object>();
            public int Operation { get; set; }
            public List<KeyValue> Parameters { get; set; } = new List<KeyValue>();
            public bool RootObject { get; set; }
        }

        private class KeyValue
        {
            public KeyValue(int key, string value)
            {
                Key = key;
                Value = value;
            }

            public int Key { get; set; }
            public string Value { get; set; }
        }
    }
}
