using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Xml.Schema;
using Service.Configuration;

namespace Service.Core
{

    public class AscClient// : IDisposable
    {
        public class StateObject
        {
            public StateObject(int bufferSize)
            {
                BufferSize = bufferSize;
                Buffer = new byte[bufferSize];
                Result = new List<byte>();
            }

            public Socket WorkSocket { get; set; }
            public int BufferSize { get; set; }
            public List<byte> Result { get; set; }
            public byte[] Buffer { get; set; }
        }

        private StateObject state;

        public const byte START = 0xC0;
        public const byte END = 0xC1;

        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);

        //private List<byte> response { get; set; }

        public Socket socket { get; set; }
        //private List<byte> autorizeReceived { get; set; }

        public bool IsStarted => socket.Connected;
        //private readonly ServiceConfig config;

        private uint reqestId = 1;
        private ILogger log;
        private EndPoint endPoint;
        private int timeoutConnectMs = 3000;
        private ServiceConfig config = null;


        public AscClient(ConnectionSettings settings, ILogger logger)
        {
            this.endPoint = new DnsEndPoint(settings.IpAdress, settings.Port);
            timeoutConnectMs = settings.TimeoutConnectMs;
            this.log = logger;
        }

        public bool StartClient()
        {
            try
            {
                reqestId = 1;

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.NoDelay = true; // Отправлять сообщение на сервер немедленно. Не накапливая их в буфер.
                socket.BeginConnect(endPoint, ConnectCallback, socket);
                if (!connectDone.WaitOne(this.timeoutConnectMs))
                    throw new AscConnectionException("Не удалось установить соединение с сервером таймаут превышен");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                string m = $"Не удалось установить соединение";
                log.LogError(m);
                return false;
            }
            return true;
        }

        public void StopClient()
        {
            if (socket != null)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception e)
                {
                    
                }
                socket = null;
            }
        }

        private void SendAndWait(byte[] data)
        {
            Send(socket, data);
            if (!sendDone.WaitOne(config.TimeoutSendMs))
                throw new AscSendException("TimeoutSendMs");
            sendDone.Reset();
        }

        private List<byte> ReciveAndWait(int countBytes = 0)
        {
            Receive(socket, countBytes);
            if (!receiveDone.WaitOne(config.TimeoutRequestMs))
                throw new AscSendException("TimeoutRequestMs");
            receiveDone.Reset();
            return state.Result.ToList();
        }


        private byte[] CreateByteArrayByMessage(Message message)
        {
            var msg = new Message[] { message };
            var msgJson = JsonConvert.SerializeObject(msg);
            byte[] res = Packet.MakeSendPacket(msgJson, reqestId);
            return res;
        }

        /// <summary>
        /// Установка конфигурации и проверки. 
        /// </summary>
        /// <param name="conf"></param>
        private void InitReqvest(ServiceConfig conf)
        {
            this.config = conf;
            if (socket == null) StartClient();
            if (socket == null) throw new AscConnectionException("socket == null");
            if (!socket.Connected) throw new AscConnectionException("!socket.Connected");
            sendDone.Reset();
            receiveDone.Reset();
        }


        /// <summary>
        /// Получает данные от ASC сервера
        /// </summary>
        /// <typeparam name="T">Тип возвращаемых данных</typeparam>
        /// <param name="message">Сообщение для сервера</param>
        /// <returns>вернет null в случаи ошибки иначе обьет типа Т</returns>
        public T Reqvest<T>(Message message, ServiceConfig conf)
        {
            InitReqvest(conf);

            // Формируем запрос для получения данных с сервера
            byte[] data = CreateByteArrayByMessage(message);
            // Отправляем запрос для получения данных с сервера
            SendAndWait(data);

            // Получаем подтверждение
            List<byte> responce1 = ReciveAndWait(24);
            List<byte> responce2 = ReciveAndWait();
            //List<byte> responce = responce1.Concat(responce2).ToList();

            //log.LogError(BitConverter.ToString(responce.ToArray()));

            SendAndWait(Packet.GetConfirmationPackBytes(reqestId));
            reqestId++;

            //log.LogError(BitConverter.ToString(responce));

            string packet;
            try
            {
                packet = Packet.ParceReceivedPacket(responce2);
            }
            catch (Exception e)
            {
                log.LogError(e, e.Message);
                log.LogError(BitConverter.ToString(responce2.ToArray()));
                Console.WriteLine($"!!!!!!!!!!!!!!!!!!1 {e.Message}");
                return default(T);
            }

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
                    throw new Exception("Не удалось установить соединение с сервером");
            }
            catch (Exception e)
            {
                Console.WriteLine("ConnectCallback error");
                log.LogError("ConnectCallback error", e.Message, e);
            }
        }

        private void Receive(Socket client, int readBybesCount)
        {
            if (readBybesCount == 0)
                readBybesCount = config.BufferSizeBytes;
            try
            {
                state = new StateObject(config.BufferSizeBytes);
                state.WorkSocket = client;
                client.BeginReceive(state.Buffer, 0, readBybesCount, 0, ReceiveCallback, state);
            }catch(Exception e)
            {
                Console.WriteLine($"!!Receive!! - reqestId={reqestId}   ошибка {e.Message}");
                log.LogError($"!!Receive!! - reqestId={reqestId}   ошибка {e.Message}", e);
            }
           
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                state = (StateObject) ar.AsyncState;
                //Socket socket = state.WorkSocket;
                //int bytesRead = socket.EndReceive(ar);
                //responce = state.Buffer;
                //receiveDone.Set();
                //// Проверка сокета на наличие данных
                int bytesRead = socket.EndReceive(ar);
                if (bytesRead > 0 && bytesRead == config.BufferSizeBytes)
                {
                    state.Result.AddRange(state.Buffer);
                    state.Buffer = new byte[config.BufferSizeBytes];
                    if (!state.Buffer.Contains(Packet.END)) 
                        socket.BeginReceive(state.Buffer, 0, config.BufferSizeBytes, 0, ReceiveCallback, state);
                }
                else
                {
                    if (bytesRead == 0)
                    {
                        socket.BeginReceive(state.Buffer, 0, config.BufferSizeBytes, 0, ReceiveCallback, state);
                        return;
                    }
                    int endIndex = state.Buffer.IndexOf(Packet.END);
                    byte[] res = new byte[bytesRead];
                    Array.Copy(state.Buffer, res, bytesRead);
                    state.Result.AddRange(res);
                    if (endIndex == -1)
                        socket.BeginReceive(state.Buffer, 0, config.BufferSizeBytes, 0, ReceiveCallback, state);
                    else
                        receiveDone.Set();

                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"!!ReceiveCallback!! - reqestId={reqestId}   ошибка {e.Message}");
                log.LogError($"!!ReceiveCallback!! - reqestId={reqestId}   ошибка {e.Message}", e);
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
                Console.WriteLine($"!!Send!! - reqestId={reqestId}   ошибка {e.Message}");
                log.LogError($"!!Send!! - reqestId={reqestId}   ошибка {e.Message}", e);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket) ar.AsyncState;
                int bytesSent = client.EndSend(ar);
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine($"!!SendCallback!! - reqestId={reqestId}   ошибка {e.Message}");
                log.LogError($"!!SendCallback!! - reqestId={reqestId}   ошибка {e.Message}", e);
            }
        }
        
        //private void ReceiveConfirm(Socket client)
        //{
        //    try
        //    {
        //        StateObject state = new StateObject(config.BufferSizeBytes);
        //        state.WorkSocket = client;
        //        client.BeginReceive(state.Buffer, 0, Packet.MAX_CONFIRM_PACK_SIZE, 0, ReceivePacketCallback, state);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine($"!!Receive!! - reqestId={reqestId}   ошибка {e.Message}");
        //        log.LogError($"!!Receive!! - reqestId={reqestId}   ошибка {e.Message}", e);

        //    }
        //}

        //private void ReceivePacketCallback(IAsyncResult ar)
        //{
        //    try
        //    {
        //        StateObject state = (StateObject)ar.AsyncState;
        //        Socket socket = state.WorkSocket;
        //        int bytesRead = socket.EndReceive(ar);
        //        //Array.Copy(state.Buffer, responce, config.BufferSizeBytes);

        //        receiveDone.Set();
        //        //// Проверка сокета на наличие данных
        //        //int bytesRead = socket.EndReceive(ar);
        //        //if (bytesRead > 0)
        //        //{
        //        //    response.AddRange(state.buffer);
        //        //    socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
        //        //}
        //        //else
        //        //{
        //        //    response.AddRange(state.buffer);
        //        //    receiveDone.Set();
        //        //}
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine($"!!ReceiveCallback!! - reqestId={reqestId}   ошибка {e.Message}");
        //        log.LogError($"!!ReceiveCallback!! - reqestId={reqestId}   ошибка {e.Message}", e);

        //    }
        //}


    }

    #region AscExceptions
    public class AscClientBaseException : Exception
    {
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

}