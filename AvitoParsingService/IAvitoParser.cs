using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace AvitoParsingService
{
    [ServiceContract]
    public interface IAvitoParser
    {
        #region IsActive
        
        /// <summary>
        /// проверка работоспособности
        /// </summary>
        /// <returns> TRUE </returns>
        [OperationContract]
        bool IsActive();

        #endregion

        #region Working Method

        /// <summary>
        /// непосредственно парсинг
        /// </summary>
        /// <param name="p1"> параметр 1 </param>
        /// <param name="p2"> параметр 2 </param>
        /// <param name="etc"> параметр etc </param>
        /// <returns> TRUE or FALSE </returns>
        [OperationContract]
        bool Parsing(string parsingString);

        #endregion
    }
}
