using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Service.Configuration;

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
        public class StateObject
        {
            public StateObject(int bufferSize)
            {
                BufferSize = bufferSize;
                Buffer = new byte[bufferSize];
            }

            public Socket WorkSocket { get; set; }
            public int BufferSize { get; set; }
            public bool IsConfirmReceived { get; set; }
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
        private byte[] responce;

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

        /// <summary>
        /// Получает данные от ASC сервера
        /// </summary>
        /// <typeparam name="T">Тип возвращаемых данных</typeparam>
        /// <param name="message">Сообщение для сервера</param>
        /// <returns>вернет null в случаи ошибки иначе обьет типа Т</returns>
        public T Reqvest<T>(Message message, ServiceConfig conf)
        {
            this.config = conf;
            responce = new byte[config.BufferSizeBytes];

            if (socket == null)
                StartClient();

            if (socket == null)
                throw new AscConnectionException("Не удалось установить соединение с сервером");

            if (!socket.Connected)
                throw new AscConnectionException("Соединение с сервером отсутствует");

            // Формируем сообщение для отправки
            var msg = new Message[] { message };
            var msgJson = JsonConvert.SerializeObject(msg);
            var data = Packet.MakeSendPacket(msgJson, reqestId);

            // Отправляем сообщение
            Send(socket, data);
            // Дожидаемся завершения отправки
            if (!sendDone.WaitOne(config.TimeoutSendMs))
                throw new AscSendException("Превышен интервал завершения отправки данных");
            sendDone.Reset();

            // Ждем пока придет 2 пакета
            Thread.Sleep(conf.ResivedTimeMs);

            Receive(socket);
            if (!receiveDone.WaitOne(config.TimeoutRequestMs))
                throw new AscSendException("Превышен интервал получения данных от сервера");
            
            receiveDone.Reset();

            // Отправляем подтверждение что получили данные
            Send(socket, Packet.GetConfirmationPackBytes(reqestId).ToArray());
            if (!sendDone.WaitOne(config.TimeoutSendMs))
                throw new AscSendException("Превышен интервал завершения отправки подверждения данных");
            sendDone.Reset();

            reqestId++; // Двигаем счетчик сообщений
            
            string packet;
            try
            {
                packet = Packet.ParceReceivedPacket(responce);
            }
            catch (Exception e)
            {
                log.LogError(e, e.Message);
                log.LogError(BitConverter.ToString(responce));
                Console.WriteLine($"!!!!!!!!!!!!!!!!!! {e.Message}");
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

        private void Receive(Socket client)
        {
            try
            {
                StateObject state = new StateObject(config.BufferSizeBytes);
                state.WorkSocket = client;
                client.BeginReceive(state.Buffer, 0, config.BufferSizeBytes, 0, ReceiveCallback, state);

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
                StateObject state = (StateObject) ar.AsyncState;
                Socket socket = state.WorkSocket;
                int bytesRead = socket.EndReceive(ar);
                responce = state.Buffer;
                receiveDone.Set();
                //// Проверка сокета на наличие данных
                //int bytesRead = socket.EndReceive(ar);
                //if (bytesRead > 0 && bytesRead == StateObject.BufferSize)
                //{
                //    //response.AddRange(state.buffer);
                //    socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
                //}
                //else
                //{
                //    //response.AddRange(state.buffer);
                //    receiveDone.Set();
                //}
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

        #region Old

        private void ReceiveConfirm(Socket client)
        {
            try
            {
                StateObject state = new StateObject(config.BufferSizeBytes);
                state.WorkSocket = client;
                client.BeginReceive(state.Buffer, 0, Packet.MAX_CONFIRM_PACK_SIZE, 0, ReceivePacketCallback, state);
            }
            catch (Exception e)
            {
                Console.WriteLine($"!!Receive!! - reqestId={reqestId}   ошибка {e.Message}");
                log.LogError($"!!Receive!! - reqestId={reqestId}   ошибка {e.Message}", e);
            }
        }

        private void ReceivePacketCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket socket = state.WorkSocket;
                int bytesRead = socket.EndReceive(ar);
                Array.Copy(state.Buffer, responce, config.BufferSizeBytes);
                receiveDone.Set();
                //// Проверка сокета на наличие данных
                //int bytesRead = socket.EndReceive(ar);
                //if (bytesRead > 0)
                //{
                //    response.AddRange(state.buffer);
                //    socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
                //}
                //else
                //{
                //    response.AddRange(state.buffer);
                //    receiveDone.Set();
                //}
            }
            catch (Exception e)
            {
                Console.WriteLine($"!!ReceiveCallback!! - reqestId={reqestId}   ошибка {e.Message}");
                log.LogError($"!!ReceiveCallback!! - reqestId={reqestId}   ошибка {e.Message}", e);
            }
        }

        #endregion
    }
}