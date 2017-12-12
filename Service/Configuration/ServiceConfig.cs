using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Configuration
{
    public class ServiceConfig
    {
        /// <summary>
        /// Частота обновлления кэша 
        /// </summary>
        public int DelayBetweenReqvestMs { get; set; }

        /// <summary>
        /// Размер получаемого сообщения
        /// </summary>
        public int BufferSizeBytes { get; set; }

        /// <summary>
        /// Таймают на отправку запроса
        /// </summary>
        public int TimeoutRequestMs { get; set; }

        /// <summary>
        /// Таймаут на получение данных
        /// </summary>
        public int TimeoutSendMs { get; set; }

        /// <summary>
        /// Время за которое сервер обработать запрос и отправить пакет
        /// </summary>
        public int ResivedTimeMs { get; set; }
    }
}
