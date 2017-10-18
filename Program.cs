using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Test
{
    class Program
    {

        static void Main(string[] args)
        {
            //IstWork();
            var client = new AsynchronousClient();
            client.StartClient();
            Console.ReadKey();
        }


        private static void TcpClient()
        {
            var client = new TcpClient();
            int port = 1336; // порт сервера
            var address = IPAddress.Parse("192.168.1.46"); // адрес сервера
            var endPoint = new IPEndPoint(address, port);
            client.Connect(endPoint);

            NetworkStream tcpStream = client.GetStream();
            var msg = Message.GetAuthorizeMessage();
            var msgJson = JsonConvert.SerializeObject(msg);
            var pacet = new Packet();
            var data = pacet.MakeSendPacket(msgJson);

            int sendLength = data.Length;
            tcpStream.Write(data, 0, data.Length);
            Thread.Sleep(1000);

            byte[] bytes = new byte[client.ReceiveBufferSize];
            int bytesRead = tcpStream.Read(bytes, 0, client.ReceiveBufferSize);

            byte[] res = new byte[bytesRead];
            Array.Copy(bytes, 12, res, 0, bytesRead - 12);
          


            // Строка, содержащая ответ от сервера
            string returnData = Encoding.UTF8.GetString(res);
            Console.WriteLine(returnData);
        }

        private static void IstWork()
        {
            // адрес и порт сервера, к которому будем подключаться
            int port = 1336; // порт сервера
            IPAddress address = IPAddress.Parse("127.0.0.1");//IPAddress.Parse("192.168.1.46"); // адрес сервера
            IPEndPoint ipPoint = new IPEndPoint(address, port);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Disable the Nagle Algorithm for this tcp socket.
            //socket.NoDelay = true;

            // подключаемся к удаленному хосту
            socket.Connect(ipPoint);

            var msg = Message.GetAuthorizeMessage();
            var msgJson = JsonConvert.SerializeObject(msg);
            var pacet = new Packet();
            var data = pacet.MakeSendPacket(msgJson);

            socket.Send(data);
            Thread.Sleep(500);

            // получаем ответ
           
            var receiveBytes = new List<byte>();

            //Messages(socket, data, receiveBytes);

            //Messages(socket, data, receiveBytes);

            var bufer = new byte[1024]; // буфер для ответа
            do
            {
                socket.Receive(bufer);
                receiveBytes.AddRange(bufer);
                //if (bufer.Any(x => x == Packet.END)) break;
                //Thread.Sleep(50);
            }
            while (socket.Available > 0);


            //string str = BitConverter.ToString(receiveBytes.ToArray());
            var str = pacet.ParceReceivedPacket(receiveBytes);
            Console.WriteLine(str);

            File.WriteAllText("123", str);
            // закрываем сокет
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        private static void Messages(Socket socket, byte[] data, List<byte> receiveBytes)
        {
            var bufer = new byte[1024]; // буфер для ответа
            do
            {
                socket.Receive(bufer);
                receiveBytes.AddRange(bufer);
            }
            while (socket.Available > 0);
        }

        


        // var msg = Message.GetFakeObject();
        // string msgJson = JsonConvert.SerializeObject(msg);

        // Littel endian
        //var str = File.ReadAllText("./json.txt", Encoding.UTF8);

        //byte[] msgBytes = Encoding.UTF8.GetBytes(str);

        // string hex = BitConverter.ToString(msgBytes);
        //Console.WriteLine(hex);

        //Console.WriteLine(str);

        //var webSocketOptions = new WebSocketOptions()
        //{
        //    KeepAliveInterval = TimeSpan.FromSeconds(120),
        //    ReceiveBufferSize = 4 * 1024
        //};
        //app.UseWebSockets(webSocketOptions);
        //Console.ReadKey();

        //var ba = new byte[5];
        //ba[0] = 0x3c;
        //ba[1] = 0xb8;
        //ba[2] = 0x64;
        //ba[3] = 0x18;
        //ba[4] = 0xca;

        //var res = Crc.GetCrc(ba);

        //var message = Message.GetAuthorizeMessage();
        //var messageJson = JsonConvert.SerializeObject(message);
        //var res = Encoding.UTF8.GetBytes(messageJson);
        //File.WriteAllText("test.txt", BitConverter.ToString(res));

        //var str = BitConverter.ToString(res);
        //byte[] bytesA = str.Split('-')
        //    .Select(x => byte.Parse(x, NumberStyles.HexNumber))
        //    .ToArray();

    }
}
