using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace MISD.Client.Model
{
     /// <summary>
        /// Specifies the datatypes of indicator values.
        /// This is used for Database Storage.
        /// </summary>
        
    [DataContract]
    public enum  DataType
    {   
            [EnumMember]
            String = 0,
            [EnumMember]
            Float = 1,
            [EnumMember]
            Int = 2,
            [EnumMember]
            Byte = 3
        
    }
}
