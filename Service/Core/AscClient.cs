using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            public const int BufferSize = 1000000;
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
                reqestId = 1;
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.NoDelay = true; // Отправлять сообщение на сервер немедленно. Не накапливая их в буфер.
                client.BeginConnect(config.IpAdress, config.Port, ConnectCallback, client);
                if (!connectDone.WaitOne(config.DelayMsConnect))
                    throw new AscConnectionException("Не удалось установить соединение с сервером таймаут превышен");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
               
                string m = $"{Name} не удалось установить соединение";
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
        /// Получает данные от ASC сервера
        /// </summary>
        /// <typeparam name="T">Тип возвращаемых данных</typeparam>
        /// <param name="message">Сообщение для сервера</param>
        /// <returns>вернет null в случаи ошибки иначе обьет типа Т</returns>
        public T Reqvest<T>(Message message)
        {
            Console.WriteLine("1 - Начало");

            string fName = Directory.GetCurrentDirectory() + "/f.txt";
            if(File.Exists(fName))
                File.Delete(fName);

            if (client == null)
                StartClient();

            Console.WriteLine("2 - Клиент запущен");

            if (client == null)
                throw new AscConnectionException("Не удалось установить соединение с сервером");

            if (!client.Connected)
                throw new AscConnectionException("Соединение с сервером отсутствует");

            this.response = new List<byte>();

            // Формируем сообщение для отправки
            Console.WriteLine("3 - Формируем сообщение для отправки");

            var msg = new Message[] { message };
            var msgJson = JsonConvert.SerializeObject(msg);
            var data = Packet.MakeSendPacket(msgJson, reqestId);

            Console.WriteLine("4 - Отправляем запрос");

            // Отправляем сообщение
            Send(client, data);

            // Дожидаемся завершения отправки
            if (!sendDone.WaitOne(config.DelayMsSend))
                throw new AscSendException("Превышен интервал завершения отправки данных");
            sendDone.Reset();
            Console.WriteLine("5 - завершение отправки");

            // Получаем данные от сервера подтверждение
            ReceiveConfirm(client);
            Console.WriteLine("6 - Получаем данные от сервера подтверждение");

            // Дожидаемся пока данные будут полученны
            if (!receiveDone.WaitOne(config.DelayMsRequest))
                throw new AscSendException("Превышен интервал получения данных от сервера");
            receiveDone.Reset();
            Console.WriteLine("7 - подтверждение полученно");


            //получаем ответ на наш запрос
            Console.WriteLine("8 - получаем ответ на наш запрос");
            Receive(client);
            if (!receiveDone.WaitOne(config.DelayMsRequest))
                throw new AscSendException("Превышен интервал получения данных от сервера");
            
            receiveDone.Reset();
            Console.WriteLine("9 - данные на запрос полученны");

            // Отправляем подтверждение что получили данные
            Send(client, Packet.GetConfirmationPackBytes(reqestId).ToArray());
            if (!sendDone.WaitOne(config.DelayMsSend))
                throw new AscSendException("Превышен интервал завершения отправки подверждения данных");
            sendDone.Reset();

            Console.WriteLine("14 - Потверждение отправленно");
            reqestId++; // Двигаем счетчик сообщений

            string str = BitConverter.ToString(response.ToArray());
            File.WriteAllText(fName, str, Encoding.UTF8);

            string packet = String.Empty;
            try
            {
                packet = Packet.ParceReceivedPacket(response);
            }
            catch (Exception e)
            {
                Console.WriteLine(BitConverter.ToString(response.ToArray()));
                return default(T);
            }

            Console.WriteLine("Packet");
            var rMsg = JsonConvert.DeserializeObject<Message[]>(packet)[0];
            Console.WriteLine("rMsg");
            var res = JsonConvert.DeserializeObject<T>(rMsg.ParameterWeb);
            Console.WriteLine("res");

            Console.WriteLine("12 - Запрос успешно выполнен");
            Console.WriteLine("13 - Отправляем подтверждение серверу ");

            


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

        private void ReceiveConfirm(Socket client)
        {
            try
            {
                StateObject state = new StateObject();
                state.workSocket = client;
                client.BeginReceive(state.buffer, 0, Packet.MAX_CONFIRM_PACK_SIZE, 0, ReceivePacketCallback, state);
            }
            catch (Exception e)
            {
                Console.WriteLine($"!!Receive!! {Name} - reqestId={reqestId}   ошибка {e.Message}");
                log.LogError($"!!Receive!! {Name} - reqestId={reqestId}   ошибка {e.Message}", e);
            }
        }

        private void ReceivePacketCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket socket = state.workSocket;
                int bytesRead = socket.EndReceive(ar);
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
                Console.WriteLine($"!!ReceiveCallback!! {Name} - reqestId={reqestId}   ошибка {e.Message}");
                log.LogError($"!!ReceiveCallback!! {Name} - reqestId={reqestId}   ошибка {e.Message}", e);
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
                Console.WriteLine($"!!Receive!! {Name} - reqestId={reqestId}   ошибка {e.Message}");
                log.LogError($"!!Receive!! {Name} - reqestId={reqestId}   ошибка {e.Message}", e);
            }
           
        }

        
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject) ar.AsyncState;
                Socket socket = state.workSocket;
                // Проверка сокета на наличие данных
                int bytesRead = socket.EndReceive(ar);
                if (bytesRead > 0 && bytesRead == StateObject.BufferSize)
                {
                    response.AddRange(state.buffer);
                    socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
                }
                else
                {
                    response.AddRange(state.buffer);
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"!!ReceiveCallback!! {Name} - reqestId={reqestId}   ошибка {e.Message}");
                log.LogError($"!!ReceiveCallback!! {Name} - reqestId={reqestId}   ошибка {e.Message}", e);
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
                Console.WriteLine($"!!Send!! {Name} - reqestId={reqestId}   ошибка {e.Message}");
                log.LogError($"!!Send!! {Name} - reqestId={reqestId}   ошибка {e.Message}", e);
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
                Console.WriteLine($"!!SendCallback!! {Name} - reqestId={reqestId}   ошибка {e.Message}");
                log.LogError($"!!SendCallback!! {Name} - reqestId={reqestId}   ошибка {e.Message}", e);
            }
        }

        //public void Dispose()
        //{
        //    StopClient();
        //}
    }
}