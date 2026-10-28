using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FileSleuth
{
    public static class Structs
    {

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BootSector
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] jump;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
            public string name;
            public ushort bytesPerSector;
            public byte sectorsPerCluster;
            public ushort reservedSectors;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] unused0;
            public ushort unused1;
            public byte media;
            public ushort unused2;
            public ushort sectorsPerTrack;
            public ushort headsPerCylinder;
            public uint hiddenSectors;
            public uint unused3;
            public uint unused4;
            public ulong totalSectors;
            public ulong mftStart;
            public ulong mftMirrorStart;
            public uint clustersPerFileRecord;
            public uint clustersPerIndexBlock;
            public ulong serialNumber;
            public uint checksum;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 426)]
            public byte[] bootLoader;
            public ushort bootSignature;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct FileRecordHeader
        {
            public uint magicNum;
            public ushort updateSequenceOffset;
            public ushort updateSequenceSize;
            public ulong logSequenceNum;
            public ushort sequenceNum;
            public ushort hardLinkCount;
            public ushort firstAttributeOffset;
            public ushort flags;
            public uint realSize;
            public uint allocatedSize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            byte[] baseRecordNumber;
            public ushort baseSequenceNum;
            public ushort nextAttributeID;
            public ushort unused;
            public uint recordNum;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct AttributeHeader
        {
            public uint attributeType;
            public uint length;
            public byte nonResident;
            public byte nameLength;
            public ushort nameOffset;
            public ushort flags;
            public ushort attributeID;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ResidentAttributeHeader
        {
            public uint attributeType;
            public uint length;
            public byte nonResident;
            public byte nameLength;
            public ushort nameOffset;
            public ushort flags;
            public ushort attributeID;
            public uint attributeLength;
            public ushort attributeOffset;
            public byte indexed;
            public byte unused;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct NonResidentAttributeHeader
        {
            public uint attributeType;
            public uint length;
            public byte nonResident;
            public byte nameLength;
            public ushort nameOffset;
            public ushort flags;
            public ushort attributeID;
            public ulong firstCluster;
            public ulong lastCluster;
            public ushort dataRunsOffset;
            public ushort compressionUnit;
            public uint unused;
            public ulong attributeAllocated;
            public ulong attributeSize;
            public ulong streamDataSize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RunHeader
        {
            /*
             * Data Run Headers contain two 4-bit variables 
             * a length variable - length of data run in bytes
             * an offset variable - how much the data run is offset from the previous run
             */
            public byte contents;
            public int GetRunOffset() { return (contents & 0xF0) >> 4; }

            public int GetRunLength() { return contents & 0x0F; }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct FileNameAttributeHeader
        {
            public uint attributeType;
            public uint length;
            public byte nonResident;
            public byte nameLength;
            public ushort nameOffset;
            public ushort flags;
            public ushort attributeID;
            public uint attributeLength;
            public ushort attributeOffset;
            public byte indexed;
            public byte unused;
            // parentRecordNumber is supposed to be a 6 byte long value,
            // but since C# doesn't have bitfields, we opt for a 6 byte array
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] parentRecordNumber;
            public ushort parentSequenceNumber;
            public ulong creationTime;
            public ulong modificationTime;
            public ulong metadataModificationTime;
            public ulong readTime;
            public ulong allocatedSize;
            public ulong realSize;
            public uint fileProperties;
            public uint reparse;
            public byte fileNameLength;
            public byte namespaceType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public char[] namePlaceholder;

            public ulong GetParentRecordNumber()
            {
                byte[] array = new byte[8];

                Array.Copy(parentRecordNumber, array, 6);

                return BitConverter.ToUInt64(array, 0);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct AttributeListEntry
        {
            public uint attributeType;
            public ushort attributeSize;
            public byte attributeNameLength;
            public byte attributeNameOffset;
            public ulong vcn;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            byte[] baseRecordNumber;
            public ushort sequenceNumber;
            public ushort attributeId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public char[] namePlaceholder;

            public ulong GetBaseRecordNumber()
            {
                byte[] array = new byte[8];

                Array.Copy(baseRecordNumber, array, 6);

                return BitConverter.ToUInt64(array, 0);
            }
        }
    }
}
