using System.Net;

namespace Service.Configuration
{
    /// <summary>
    /// ����� ��������� ����������
    /// </summary>
    public class Config
    {
        /// <summary>
        /// ���� �� ������� ������� ��������� ����������
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// ��������� ��� ����������� � ASC 3.0 
        /// </summary>
        public AscConfig AscConfig { get; set; }
    }
}