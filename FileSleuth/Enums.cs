using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSleuth
{
    public static class Enums
    {
        public enum GenericAccessRights : uint
        {
            GENERIC_ALL = 0x10000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_READ = 0x80000000,
        }

        [Flags]
        public enum FileRecordFlags
        {
            InUse = 0x01,
            IsDirectory = 0x02,
            IsExtension = 0x04,
            SpecialIndex = 0x08
        }

        [Flags]
        public enum FileProperties
        {
            ReadOnly = 0x01,
            Hidden = 0x02,
            System = 0x04,
            Archive = 0x020,
            Device = 0x040,
            Normal = 0x080,
            Temporary = 0x0100,
            Sparse = 0x0200,
            ReparsePoint = 0x0400,
            Compressed = 0x0800,
            Offline = 0x1000,
            NotIndexed = 0x2000,
            Encrypted = 0x4000,

        }

        public enum AttributeTypes : uint
        {
            AttributeList = 0x20,
            FileName = 0x30,
            Data = 0x80,
            BitMap = 0xB0,
            EndMarker = 0xFFFFFFFF,
        }

    }
}
