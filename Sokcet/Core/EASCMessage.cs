using System;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    public enum EASCMessage
    {
        /// <summary>
        /// неизвестное сообщение
        /// </summary>
        eUnknownMessage = 0,               
        /// <summary>
        /// сообщение авторизации ( Клиент --> Cервер )
        /// </summary>
        eAutorizationMessage = 1,                       
        /// <summary>
        /// сообщение об отказе в авторизации ( Сервер --> Клиент )
        /// </summary>
        eAutorizationFailedMessage = 2,                 
        /// <summary>
        /// сообщение об успешной авторизации ( Сервер --> Клиент )
        /// </summary>
        eAutorizationSuccessMessage = 3,                
        /// <summary>
        ///  сообщение об изменениях в объектах ядра ( любые направления )
        /// </summary>
        eChangeASCObjectsMessage = 4,                   
        /// <summary>
        ///  сообщение - дай доступные com-порты ( Клиент --> Cервер )
        /// </summary>
        eGiveAvailableComPortsMessage = 5,             
        /// <summary>
        /// сообщение - получи доступные com-порты ( Сервер --> Клиент )
        /// </summary>
        eGetAvailableComPortsMessage = 6,
        /// <summary>
        /// сообщение - добавить устройство ( Клиент --> Сервер --> Клиенты)
        /// </summary>
        eAddDeviceMessage = 7,                         
        /// <summary>
        /// сообщение - изменить состояние опроса ( Клиент --> Сервер --> Клиенты )
        /// </summary>
        eChangeInterviewStateMessage = 8,           
        /// <summary>
        /// сообщение - удалить устройство ( Клиент --> Сервер --> Клиенты )
        /// </summary>
        eDeleteDeviceMessage = 9,                      
        /// <summary>
        /// сообщение - установить время на устройстве ( Клиент --> Cервер )
        /// </summary>
        eSetTimeMessage = 10,                           
        /// <summary>
        ///  сообщение - установить настройки каналов на устройстве	( Клиент --> Cервер )
        /// </summary>
        eSetChannelsSettingsMessage = 11,              
        /// <summary>
        /// сообщение - установить коды на устройстве ( Клиент --> Сервер )
        /// </summary>
        eSetCodesMessage = 12,                         
        /// <summary>
        /// сообщение - изменить каналы у устройства ( Сервер --> Клиенты )
        /// </summary>
        eChangeChannelHolderMessage = 13,               
        /// <summary>
        /// сообщение - вызвать бригаду ( Клиент --> Сервер )
        /// </summary>
        eCallBrigadeMessage = 14,                
        /// <summary>
        /// сообщение - добавить подразделение ( Клиент --> Сервер --> Клиенты )
        /// </summary>
        eAddSubdivisionMessage = 15,                
        /// <summary>
        ///  сообщение - удалить подразделение ( Клиент --> Сервер --> Клиенты )
        /// </summary>
        eDeleteSubdivisionMessage = 16,         
        /// <summary>
        /// сообщение - добавить бригаду ( Клиент --> Сервер --> Клиенты )
        /// </summary>
        eAddBrigadeMessage = 17,                
        /// <summary>
        ///  сообщение - удалить бригаду ( Клиент --> Сервер --> Клиенты )
        /// </summary>
        eDeleteBrigadeMessage = 18,                    
        /// <summary>
        ///  сообщение - добавить работу ( Клиент --> Сервер --> Клиенты )
        /// </summary>
        eAddWorkMessage = 19,                        
        /// <summary>
        /// сообщение - удалить работу ( Клиент --> Сервер --> Клиенты )
        /// </summary>
        eDeleteWorkMessage = 20,                      
        /// <summary>
        /// сообщение - добавить месторождение ( Клиент --> Сервер --> Клиенты )
        /// </summary>
        eAddFieldMessage = 21,                
        /// <summary>
        /// сообщение - удалить месторождение ( Клиент --> Сервер --> Клиенты )
        /// </summary>
        eDeleteFieldMessage = 22,                   
        /// <summary>
        ///  сообщение - добавить куст ( Клиент --> Сервер --> Клиенты )
        /// </summary>
        eAddClusterMessage = 23,           
        /// <summary>
        /// сообщение - удалить куст ( Клиент --> Сервер --> Клиенты )
        /// </summary>
        eDeleteClusterMessage = 24,                 
        /// <summary>
        ///  сообщение - добавить скважину ( Клиент --> Сервер --> Клиенты )
        /// </summary>
        eAddWellMessage = 25,                 
        /// <summary>
        /// сообщение - удалить скважину ( Клиент --> Сервер --> Клиенты )
        /// </summary>
        eDeleteWellMessage = 26,                    
        /// <summary>
        ///  сообщение - редактировать координаты ( Клиент --> Сервер --> Клиенты )
        /// </summary>
        eEditCoordinatesOfGeoObjectMessage = 27,     

        /// <summary>
        ///  cообщение - изменить информацию о трендах ( Клиент --> Сервер )
        /// </summary>
        eChangeTrendingInfoMessage = 28,               
        /// <summary>
        /// сообщение - получить из архива информацию о кодах для ИВЭ-50 ( Клиент --> Сервер )
        /// </summary>
        eGiveIve50ArchiveCodesInfoMessage = 29,       
        /// <summary>
        /// сообщение - отдать информацию о кодах для ИВЭ-50 ( Сервер --> Клиент )
        /// </summary>
        eGetIve50ArchiveCodesInfoMessage = 30,          
        /// <summary>
        ///  сообщение - получить из архива информацию о каналах ИВЭ-50	( Клиент --> Сервер )
        /// </summary>
        eGiveIve50ArchiveChannelsInfoMessage = 31,      
        /// <summary>
        ///  сообщение - отдать информацию о каналах для ИВЭ-50 ( Сервер --> Клиент )
        /// </summary>
        eGetIve50ArchiveChannelsInfoMessage = 32,    
        /// <summary>
        /// сообщение - установить коды, обозн-щие отсутствие геообъекта ( Клиент --> Сервер --> Клиенты )
        /// </summary>
        eSetCodesIndicatingAbsenceOfGeoObjectMessage = 33,
        /// <summary>
        /// сообщение - добавить ИВЭ-50 кандидата ( Сервер --> Клиенты )
        /// </summary>
        eAddIve50DeviceCandidateMessage = 34,           
        /// <summary>
        /// сообщение - удалить ИВЭ-50 кандидата  Сервер --> Клиенты )
        /// </summary>
        eDeleteIve50DeviceCandidateMessage = 35,       
        /// <summary>
        ///  сообщение - добавить аккаунт ( Клиент --> Сервер --> Клиенты )
        /// </summary>
        eAddAccountMessage = 36,                      
        /// <summary>
        /// сообщение - удалить аккаунт	( Клиент --> Сервер --> Клиенты )
        /// </summary>
        eDeleteAccountMessage = 37,                 
        /// <summary>
        ///  сообщение - изменить таблицу доступа ( Клиент --> Сервер --> Клиенты )
        /// </summary>
        eChangeAccessTableMessage = 38,
        /// <summary>
        /// сообщение - не удалось удалить подразделение ( Сервер --> Клиент )
        /// </summary>
        eFailedDeleteSubdivisionMessage = 39,           
        /// <summary>
        ///  сообщение - получить медиа-файл ( Клиент --> Сервер )
        /// </summary>
        eGiveMediaFileMessage = 40,                  
        /// <summary>
        /// сообщение - нет такого медиа-файла ( Сервер --> Клиент )
        /// </summary>
        eNoMediaFileMessage = 41,                     
        /// <summary>
        /// сообщение - отдать содержимое медиа-файла ( Сервер --> Клиент )
        /// </summary>
        eGetMediaFileMessage = 42,                      
        /// <summary>
        ///  сообщение - инициирована загрузка данных ( Клиент --> Сервер )
        /// </summary>
        eLoadDataInitMessage = 43,                     
        /// <summary>
        /// сообщение - загрузить данные ( Клиент --> Сервер )
        /// </summary>                                           
        eLoadDataArcMessage = 44,
        /// <summary>
        /// сообщение - прекратить загрузку данных ( Клиент --> Сервер )
        /// </summary>
        eLoadDataStopMessage = 45,                  
        /// <summary>
        /// сообщение - загрузить данные JSON ( Клиент --> Сервер )
        /// </summary>
        eLoadDataJsonMessage = 46,                      
        /// <summary>
        /// сообщение - загрузить видео	( Клиент --> Сервер )
        /// </summary>
        eLoadVideoMessage = 47,                       
        /// <summary>
        ///  сообщение - удалить данные	( Клиент --> Сервер --> Клиенты )
        /// </summary>
        eDeleteDataMessage = 48,                     
        /// <summary>
        /// сообщение - удалить бригаду из дерева кодов в архивах ( Клиент --> Сервер --> Клиенты )
        /// </summary>
        eDeleteBrigadeFromTreeMessage = 49,          
        /// <summary>
        /// сообщение - загрузить данные asc3 ( Клиент --> Сервер )
        /// </summary>
        eLoadDataAsc3Message = 50,     
        /// <summary>
        /// сообщение - инициированна выдача медиа-файла ( Сервер --> Клиент )
        /// </summary>
        eGetMediaFileInitMessage = 51,

        /// <summary>
        /// сообщение - получить информацию по бригадам ( Сервер --> Клиент )
        /// </summary>
        eWebGetBrigadesInfo = 100
    }
}
