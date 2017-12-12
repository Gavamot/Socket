using System.Net;

namespace Service.Configuration
{
    /// <summary>
    /// Общие настройки приложения
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Порт на котором следует запускать приложение
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Настройки для подключение к ASC 3.0 
        /// </summary>
        public AscConfig AscConfig { get; set; }
    }
}