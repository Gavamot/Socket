using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Core
{
    public enum EASCMessagePath
    {
        /// <summary>
        /// сообщение передается только вперед, без возврата тому, кто инициировал посылку
        /// </summary>
        eOnlyFoward = 0x00000000,
        /// <summary>
        /// сообщение передается вперед, копируется и посылается назад, тому, кто послал его
        /// </summary>
        eFowardBack = 0x00100000,
        /// <summary>
        ///  сообщение передается вперед, копируется и рассылается всем, кому только можно (для рассылки всем клиентам)
        /// </summary>
        eFowardBackAll = 0x00200000,	
    }
}
