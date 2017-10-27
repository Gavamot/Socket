using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Test
{

    // State object for receiving data from remote device.
    public class StateObject
    {
        public Socket workSocket = null;

        public const int BufferSize = 1024;

        public bool isConfirmReceived = false;
      
        public byte[] buffer = new byte[BufferSize];
    }

    public class AsynchronousClient : IDisposable
    {
        // The port number for the remote device.
        private const int port = 1336;
        private const string ip = "127.0.0.1";

        public const byte START = 0xC0;
        public const byte END = 0xC1;

        private uint _messageQueueIndex;

        public uint MessageQueueIndex
        {
            get => _messageQueueIndex++;
            protected set => _messageQueueIndex = value;
        }


        private ManualResetEvent connectDone =
            new ManualResetEvent(false);

        private ManualResetEvent sendDone =
            new ManualResetEvent(false);

        private ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        // The response from the remote device.
        private List<byte> response { get; set; }

        private Socket client { get; set; }
        private List<byte> AutorizeReceived { get; set; }
        private IPEndPoint remoteEP { get; set; }

        public List<byte> Reqvest(Message message)
        {
            
            if (client == null || !client.Connected)
                throw new Exception("Соединение с сервером отсутствует");
            // Ответ на запрос
            this.response = new List<byte>();

            // Cбрасываем индентификаторы событий
            sendDone.Reset();
            receiveDone.Reset(); 
            
            // Формируем сообщение для отправки
            var msg = new List<Message> { message };
            var msgJson = JsonConvert.SerializeObject(msg);
            var data = Packet.MakeSendPacket(msgJson);

            // Отправляем сообщение
            Send(client, data);
            // Дожидаемся завершения отправки
            sendDone.WaitOne();
            
            // Получаем данные от сервера
            Receive(client);

            // Дожидаемся пока данные будут полученны
            receiveDone.WaitOne();

            // Если пришел только пакет с подтверждением получаем ответ на наш запрос
            if(response.Count <= Packet.MAX_CONFIRM_PACK_SIZE) { 
                receiveDone.Reset();
                Receive(client);
                receiveDone.WaitOne();
            }

            var packets = Packet.SplitToPackets(response);

            // 1 - пакет подтверждеие
            // 2 - пакет ответ на запрос
            // Отправляем подтверждение что получили данные
            Send(client, packets[0].ToArray());

            return packets[1];
        }

        public bool IsStarted => client.Connected;

        public AsynchronousClient()
        {
            IPAddress ipAddress = IPAddress.Parse(ip);
            remoteEP = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.NoDelay = true;
        }

        public void StartClient()
        {
            // Connect to a remote device.
            try
            {
                MessageQueueIndex = 2;
                // Connect to the remote endpoint.
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();
                
                AutorizeReceived = Reqvest(Message.CreateAuthorizeMessage());
                Console.WriteLine("Авторизационные данные полученны");

                //string json1 = Packet.ParceReceivedPacket(req);
                //AutorizeReceived = Reqvest(client, Message.CreateGiveIve50ArchiveCodesInfoMessage(2));

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void RestartClient()
        {
            StopClient();
            StartClient();
        }

        public void StopClient()
        {
            if (client != null)
            {
                try
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    // ignored
                }
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket) ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private StateObject state;

        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Счетчик пакетов в запросе их должно быть 2 подтверждеие и ответ
        /// </summary>
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject) ar.AsyncState;
                Socket socket = state.workSocket;
                // Проверка сокета на наличие данных
               

                int bytesRead = socket.EndReceive(ar);
                if (bytesRead < StateObject.BufferSize)
                {
                    int countBytes = state.buffer.IndexOf(END) + 1;
                    if (countBytes == 0) return;
                    var buffer = new byte[countBytes];
                    Array.Copy(state.buffer, buffer, countBytes);
                    response.AddRange(buffer);

                    if (socket.Available > 0)
                    {
                        socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(ReceiveCallback), state);
                    }
                    else
                    {
                        receiveDone.Set();
                    }
                }
                else
                {
                    response.AddRange(state.buffer);
                    socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Send(Socket client, byte[] byteData)
        {
            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket) ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }



        public void Dispose()
        {
            StopClient();
        }
    }
}