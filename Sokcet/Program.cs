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
            var msg = Message.CreateAuthorizeMessage();
            var msgJson = JsonConvert.SerializeObject(msg);
            var data = Packet.MakeSendPacket(msgJson);

            int sendLength = data.Length;
            tcpStream.Write(data, 0, data.Length);

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

            var msg = Message.CreateAuthorizeMessage();
            var msgJson = JsonConvert.SerializeObject(msg);
            var pacet = new Packet();
            var data = Packet.MakeSendPacket(msgJson);

            socket.Send(data);
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
            var str = Packet.ParceReceivedPacket(receiveBytes);
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

    }
}
