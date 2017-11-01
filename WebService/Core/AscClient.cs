using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace WebService
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

    public class AscClient : IDisposable
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

            public const int BufferSize = 1024;

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
        private AscConfig config;
        public int Tries => config.Tries;

        public AscClient(AscConfig config)
        {
            this.config = config;
        }

        public void StartClient()
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.NoDelay = true; // ���������� ��������� �� ������ ����������. �� ���������� �� � �����.
            client.BeginConnect(config.IpAdress, config.Port, ConnectCallback, client);
            if (!connectDone.WaitOne(config.DelayMsConnect))
                throw new AscConnectionException("�� ������� ���������� ���������� � ��������");
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
                catch (Exception e) {

                }
            }
        }

        public void RestartClient()
        {
            StopClient();
            StartClient();
        }

        public T Reqvest<T>(Message message)
        {
            if (client == null)
                StartClient();
            if (!client.Connected)
                throw new AscConnectionException("���������� � �������� �����������");

            // ����� �� ������
            this.response = new List<byte>();

            // C��������� ��������������� �������
            //sendDone.Reset();
            //receiveDone.Reset(); 

            // ��������� ��������� ��� ��������
            var msg = new List<Message> { message };
            var msgJson = JsonConvert.SerializeObject(msg);
            var data = Packet.MakeSendPacket(msgJson);

            // ���������� ���������
            Send(client, data);
            // ���������� ���������� ��������
            if (!sendDone.WaitOne(config.DelayMsSend))
                throw new AscSendException("�������� �������� ���������� �������� ������");

            // �������� ������ �� �������
            Receive(client);

            // ���������� ���� ������ ����� ���������
            if (!receiveDone.WaitOne(config.DelayMsRequest))
                throw new AscSendException("�������� �������� ��������� ������ �� �������");
        

            // ���� ������ ������ ����� � �������������� �������� ����� �� ��� ������
            if (response.Count <= Packet.MAX_CONFIRM_PACK_SIZE)
            {
                receiveDone.Reset();
                Receive(client);
                if (!receiveDone.WaitOne(config.DelayMsRequest))
                    throw new AscSendException("�������� �������� ��������� ������ �� �������");
            }
            
            var packets = Packet.SplitToPackets(response);
            // 0 - ����� ������������
            // 1 - ����� ����� �� ������
            // ���������� ������������� ��� �������� ������
            Send(client, packets[0].ToArray());
            var packet = Packet.ParceReceivedPacket(packets[1]);
            var rMsg = JsonConvert.DeserializeObject<Message[]>(packet)[0];
            var res = JsonConvert.DeserializeObject<T>(rMsg.ParameterWeb);
            return res;
        }


        private void ConnectCallback(IAsyncResult ar)
        {
            Socket client = (Socket) ar.AsyncState;
            client.EndConnect(ar);
            if (!connectDone.Set())
                throw new Exception("�� ������� ���������� ���������� � ��������");
        }

        private void Receive(Socket client)
        {
            StateObject state = new StateObject();
            state.workSocket = client;
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
        }

        /// <summary>
        /// ������� ������� � ������� �� ������ ���� 2 ������������ � �����
        /// </summary>
        private void ReceiveCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject) ar.AsyncState;
            Socket socket = state.workSocket;
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
                    socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
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

        private void Send(Socket client, byte[] byteData)
        {
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            Socket client = (Socket) ar.AsyncState;
            int bytesSent = client.EndSend(ar);
            sendDone.Set();
        }

        public void Dispose()
        {
            StopClient();
        }
    }
}