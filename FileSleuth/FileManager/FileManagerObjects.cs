using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static FileSleuth.Structs;

namespace FileSleuth.FileManager
{
    public static class FileManagerObjects
    {

        public class AttributeListInfo
        {

            public bool hasAttributeList { get; set; }
            public int attributeListPosition { get; set; }
            public ResidentAttributeHeader attributeListAttribute { get; set; }

            public AttributeListInfo()
            {
                hasAttributeList = false;
                attributeListPosition = 0;
                attributeListAttribute = new ResidentAttributeHeader();
            }
        }

    }
}
