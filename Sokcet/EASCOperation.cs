using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    /// <summary>
    ///  операции поддерживаемые в системе
    /// </summary>
    public enum EASCOperation
    {
        /// <summary>
        /// неопределенная операция
        /// </summary>
        eUndefinedOperation = 0,
        /// <summary>
        /// операция не требующая контроля доступа
        /// </summary>
        eNoAccessOperation = 1,
        /// <summary>
        ///  добавить устройство
        /// </summary>
        eAddDeviceOperation = 10,
        /// <summary>
        /// изменить координаты месторасположения устройства
        /// </summary>
        eChangeDeviceLocationOperation = 11,
        /// <summary>
        /// изменить состояние опроса устройства
        /// </summary>
        eChangeInterviewStateOperation = 12,    
        /// <summary>
        /// изменить параметры работы с устройством
        /// </summary>
        eEditDeviceOperation = 13,
        /// <summary>
        /// удалить устройство
        /// </summary>
        eDeleteDeviceOperation = 14,
        /// <summary>
        ///  установить время на устройстве
        /// </summary>
        eSetTimeOperation = 15,
        /// <summary>
        ///  установить настройки каналов на устройстве
        /// </summary>
        eSetChannelsSettingsOperation = 16,
        /// <summary>
        /// установить коды на устройстве
        /// </summary>
        eSetCodesOperation = 17,
        /// <summary>
        /// вызвать бригаду
        /// </summary>
        eCallBrigadeOperation = 18,
        /// <summary>
        /// изменить информацию в справочниках
        /// </summary>
        eEditDirectoriesOperation = 19,
        /// <summary>
        ///  просмотреть архив
        /// </summary>
        eBrowseArchiveOperation = 20,
        /// <summary>
        /// просмотреть тренды
        /// </summary>
        eBrowseTrendsOperation = 21,
        /// <summary>
        /// изменить информацию о пользователях
        /// </summary>
        eEditUsersOperation = 22,
        /// <summary>
        ///  просмотреть подключения ИВЭ-50 (TCP Сервер)
        /// </summary>
        eBrowseIVE50TcpServerConnectionsOperations = 23,
        /// <summary>
        ///  установить настройки сервера
        /// </summary>
        eSetServerSettingsOperations = 24,
        /// <summary>
        ///  загрузить данные
        /// </summary>
        eLoadDataOperation = 25,
        /// <summary>
        /// удалить данные из архивов за определённую дату по бригаде(устройству)
        /// </summary>
        eDeleteDataOperation = 26
    };
}
