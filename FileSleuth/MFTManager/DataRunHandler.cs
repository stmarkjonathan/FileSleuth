using FileSleuth.FileManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static FileSleuth.Constants;
using static FileSleuth.Enums;
using static FileSleuth.MFTManager.MFTMethods;
using static FileSleuth.Structs;

namespace FileSleuth.MFTManager
{
    public class DataRunHandler
    {
        MFTFile mftFile;
        NonResidentAttributeHeader mftDataAttribute;
        ulong bytesPerCluster;
        int dataAttributePosition;
        int dataRunPosition;
        RunHeader dataRun;
        public DataRunHandler(MFTFile mftFile)
        {
            this.mftFile = mftFile;
            bytesPerCluster = (ulong)(mftFile.bootSector.bytesPerSector * mftFile.bootSector.sectorsPerCluster);

            mftDataAttribute = FindNonResidentDataAttribute(mftFile.mftData,
                mftFile.mftFileRecord.firstAttributeOffset,
                out int dataAttributePosition);

            this.dataAttributePosition = dataAttributePosition;

            dataRunPosition = dataAttributePosition + mftDataAttribute.dataRunsOffset;

            dataRun = BytesToStruct<RunHeader>
                (mftFile.mftData[dataRunPosition..^0]);   
        }

        private NonResidentAttributeHeader FindNonResidentDataAttribute(byte[] mftFile, int filePosition, out int dataAttributePosition)
        {

            AttributeHeader attribute = BytesToStruct<AttributeHeader>(mftFile[filePosition..^0]);

            NonResidentAttributeHeader dataAttribute = new NonResidentAttributeHeader();

            dataAttributePosition = 0;

            while (attribute.attributeType != (uint)AttributeTypes.EndMarker)
            {
                if (attribute.attributeType == (uint)AttributeTypes.Data)
                {
                    dataAttribute = BytesToStruct<NonResidentAttributeHeader>(mftFile[filePosition..^0]);
                    dataAttributePosition = filePosition;
                }

                filePosition += (int)attribute.length;

                attribute = BytesToStruct<AttributeHeader>(mftFile[filePosition..^0]);
            }

            if (dataAttribute.Equals(default(NonResidentAttributeHeader)))
            {
                throw new Exceptions.DataAttributeNotFoundException();
            }

            return dataAttribute;
        }

        public List<(byte[] mftBuffer, ulong filesToLoad)> EnumerateDataRuns()
        {
            List<(byte[] mftBuffer, ulong filesToLoad)> dataRunData = new List<(byte[] mftBuffer, ulong filesToLoad)>();
            ulong clusterNum = 0;
            byte[] mftBuffer = new byte[MFT_FILES_PER_BUFFER * MFT_FILE_SIZE];

            while ((dataRunPosition - dataAttributePosition) < mftDataAttribute.length
                && dataRun.GetRunLength() > 0)
            {

                CalculateLengthAndOffset(dataRun, dataRunPosition, mftFile.mftData, out ulong length, out ulong offset);

                clusterNum += offset;
                ulong filesRemaining = length * bytesPerCluster / MFT_FILE_SIZE;
                ulong bufferPosition = 0;

                while (filesRemaining > 0)
                {

                    ulong filesToLoad = MFT_FILES_PER_BUFFER;

                    if (filesRemaining < MFT_FILES_PER_BUFFER)
                        filesToLoad = filesRemaining;

                    ReadHandle(mftFile.parentVolume.volumeHandle, 
                        mftBuffer,
                        (long)(clusterNum * bytesPerCluster + bufferPosition),
                        (uint)(filesToLoad * MFT_FILE_SIZE));

                    bufferPosition += filesToLoad * MFT_FILE_SIZE;
                    filesRemaining -= filesToLoad;

                    dataRunData.Add(ValueTuple.Create((byte[])mftBuffer.Clone(), filesToLoad));
                }

                dataRunPosition += dataRun.GetRunLength() + dataRun.GetRunOffset() + 1;
                dataRun = BytesToStruct<RunHeader>(mftFile.mftData[dataRunPosition..^0]);

            }

            return dataRunData; 
        }

        /// <summary>
        /// Extracts and returns the length and offset values of a data run
        /// </summary>
        /// <param name="dataRun"></param>
        /// <param name="dataRunPosition"></param>
        /// <param name="mftFile"></param>
        /// <param name="length"></param>
        /// <param name="offset"></param>
        private void CalculateLengthAndOffset(RunHeader dataRun, int dataRunPosition, byte[] mftFile, out ulong length, out ulong offset)
        {
            length = 0;
            offset = 0;

            for (int i = 0; i < dataRun.GetRunLength(); i++)
            {
                length |= (ulong)(mftFile[dataRunPosition + 1 + i]) << (i * 8);
            }

            for (int i = 0; i < dataRun.GetRunOffset(); i++)
            {
                offset |= (ulong)(mftFile[dataRunPosition + 1 + dataRun.GetRunLength() + i]) << (i * 8);
            }

            if ((offset & ((ulong)1 << (dataRun.GetRunOffset() * 8 - 1))) > 0)
            {
                for (int i = dataRun.GetRunOffset(); i < 8; i++)
                {
                    offset |= (ulong)0xFF << (i * 8);
                }
            }
        }
    }

}
