using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Service.Core
{

#region AscExceptions
    public class AscClientBaseException : Exception {
        public AscClientBaseException(string message) : base(message) { }
        public AscClientBaseException(string message, Exception innerException) : base(message, innerException) { }
    } 

    public class AscConnectionException : AscClientBaseException
    {
        public AscConnectionException(string message) : base(message) { }
        public AscConnectionException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class AscSendException : AscClientBaseException
    {
        public AscSendException(string message) : base(message) { }
        public AscSendException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class AscResponseException : AscClientBaseException
    {
        public AscResponseException(string message) : base(message) { }
        public AscResponseException(string message, Exception innerException) : base(message, innerException) { }
    }
#endregion

    public class AscClient// : IDisposable
    {
        // private uint _messageQueueIndex;

        //public uint MessageQueueIndex
        //{
        //    get => _messageQueueIndex++;
        //    protected set => _messageQueueIndex = value;
        //}

        public class StateObject
        {
            public Socket workSocket = null;
            public const int BufferSize = 2000000;//1024;
            public bool isConfirmReceived = false;
            public byte[] buffer = new byte[BufferSize];
        }

        private StateObject state;

        public const byte START = 0xC0;
        public const byte END = 0xC1;

        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);

        private List<byte> response { get; set; }
        private Socket client { get; set; }
        private List<byte> autorizeReceived { get; set; }

        public bool IsStarted => client.Connected;
        private readonly AscConfig config;
        public int Tries => config.Tries;
        private uint reqestId = 1;
        private ILogger log;
        public readonly string Name;

        public AscClient(string name, AscConfig config, ILogger logger)
        {
            this.log = logger;
            this.Name = name;
            this.config = config;
        }

        public bool StartClient()
        {
            try
            {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, true);
                client.NoDelay = true; // ���������� ��������� �� ������ ����������. �� ���������� �� � �����.
                client.BeginConnect(config.IpAdress, config.Port, ConnectCallback, client);
                if (!connectDone.WaitOne(config.DelayMsConnect))
                    throw new AscConnectionException("�� ������� ���������� ���������� � �������� ������� ��������");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
               
                string m = $"{Name} �� ������� ���������� ����������";
                log.LogError(m);
                return false;
            }
            return true;
        }



        public void StopClient()
        {
            if (client != null)
            {
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                client = null;
            }
        }

        /// <summary>
        /// �������� ������ �� ASC �������
        /// </summary>
        /// <typeparam name="T">��� ������������ ������</typeparam>
        /// <param name="message">��������� ��� �������</param>
        /// <returns>������ null � ������ ������ ����� ����� ���� �</returns>
        public T Reqvest<T>(Message message)
        {
            if (client == null)
                StartClient();

            if (client == null)
                throw new AscConnectionException("�� ������� ���������� ���������� � ��������");

            if (!client.Connected)
                throw new AscConnectionException("���������� � �������� �����������");

            this.response = new List<byte>();

            // ��������� ��������� ��� ��������
            var msg = new Message[] { message };
            var msgJson = JsonConvert.SerializeObject(msg);
            var data = Packet.MakeSendPacket(msgJson, reqestId);

            // ���������� ���������
            Send(client, data);

            // ���������� ���������� ��������
            if (!sendDone.WaitOne(config.DelayMsSend))
                throw new AscSendException("�������� �������� ���������� �������� ������");
            sendDone.Reset();

            // �������� ������ �� �������
            Receive(client);

           
            // ���������� ���� ������ ����� ���������
            if (!receiveDone.WaitOne(config.DelayMsRequest))
                throw new AscSendException("�������� �������� ��������� ������ �� �������");
            receiveDone.Reset();

            

            // ���� ������ ������ ����� � �������������� �������� ����� �� ��� ������
            if (response.Count <= Packet.MAX_CONFIRM_PACK_SIZE)
            {
                Receive(client);
                if (!receiveDone.WaitOne(5000))
                {
                    throw new AscSendException("�������� �������� ��������� ������ �� �������");
                }
                receiveDone.Reset();
            }
            
            var packets = Packet.SplitToPackets(response);

            // 0 - ����� ������������
            // 1 - ����� ����� �� ������
            // ���������� ������������� ��� �������� ������
            Send(client, Packet.GetConfirmationPackBytes(reqestId).ToArray());
            if (!sendDone.WaitOne(config.DelayMsSend))
                throw new AscSendException("�������� �������� ���������� �������� ������������ ������");
            sendDone.Reset();
          

            if (packets.Count != 2) return default(T);
            reqestId++; // ������� ������� ���������

            var packet = Packet.ParceReceivedPacket(packets[1]);
            var rMsg = JsonConvert.DeserializeObject<Message[]>(packet)[0];
            var res = JsonConvert.DeserializeObject<T>(rMsg.ParameterWeb);
         
            return res;
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket) ar.AsyncState;
                client.EndConnect(ar);
                connectDone.Set();
                if (!connectDone.Set())
                    throw new Exception("�� ������� ���������� ���������� � ��������");
            }
            catch (Exception e)
            {
                Console.WriteLine("ConnectCallback error");
                log.LogError("ConnectCallback error", e.Message, e);
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                StateObject state = new StateObject();
                state.workSocket = client;
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
            }catch(Exception e)
            {
                Console.WriteLine($"!!Receive!! {Name} - reqestId={reqestId}   ������ {e.Message}");
                log.LogError($"!!Receive!! {Name} - reqestId={reqestId}   ������ {e.Message}", e);
            }
           
        }

        
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject) ar.AsyncState;
                Socket socket = state.workSocket;
                if (socket == null)
                    return;

                // �������� ������ �� ������� ������
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
                        socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
                    }
                    else
                    {
                        receiveDone.Set();
                    }
                }
                else
                {
                    response.AddRange(state.buffer);
                    socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"!!ReceiveCallback!! {Name} - reqestId={reqestId}   ������ {e.Message}");
                log.LogError($"!!ReceiveCallback!! {Name} - reqestId={reqestId}   ������ {e.Message}", e);
            }
        }

        private void Send(Socket client, byte[] byteData)
        {
            try
            {
                client.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, client);
            }
            catch(Exception e)
            {
                Console.WriteLine($"!!Send!! {Name} - reqestId={reqestId}   ������ {e.Message}");
                log.LogError($"!!Send!! {Name} - reqestId={reqestId}   ������ {e.Message}", e);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket) ar.AsyncState;
                if (client == null) return;
                int bytesSent = client.EndSend(ar);
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine($"!!SendCallback!! {Name} - reqestId={reqestId}   ������ {e.Message}");
                log.LogError($"!!SendCallback!! {Name} - reqestId={reqestId}   ������ {e.Message}", e);
            }
        }

        //public void Dispose()
        //{
        //    StopClient();
        //}
    }
}