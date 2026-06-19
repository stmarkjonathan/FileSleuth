using Microsoft.Win32.SafeHandles;
using Windows.Win32;
using Windows.Win32.Storage.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FileSleuth.MFTManager
{
    public static class MFTMethods
    {


        /// <summary>
        /// Converts a chosen struct into an array of bytes
        /// </summary>
        /// <param name="str">The struct to be converted into an array of bytes</param>
        /// <returns></returns>
        public static byte[] StructToBytes<T>(T str)
        {
            if(str != null)
            {
                int size = Marshal.SizeOf(str);
                byte[] arr = new byte[size];

                IntPtr ptr = IntPtr.Zero;

                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(str, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);

                Marshal.FreeHGlobal(ptr);

                return arr;
            }
            else
            {
                throw new ArgumentNullException(nameof(str));
            }
            


        }

        /// <summary>
        /// Converts an array of bytes to a chosen struct.
        /// </summary>
        /// <param name="bytes">The array of bytes to be converted into a struct</param>
        /// <returns></returns>
        public static T BytesToStruct<T>(byte[] bytes) where T : new()
        {

            T str = new T();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = IntPtr.Zero;

            ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(bytes, 0, ptr, size);

            object? structure = Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);

            if (structure != null)
            {
                str = (T)structure;
                return str;
            }
            else
            {
                throw new ArgumentNullException(nameof(structure));
            }

            

            
        }

        /// <summary>
        /// Read bytes from <paramref name="handle"/> into <paramref name="buffer"/>, starting at <paramref name="from"/>,
        /// to <paramref name="count"/>.
        /// </summary>
        /// <param name="handle">A handle to the device that will be read.</param>
        /// <param name="buffer">The buffer that receives the read bytes from the device</param>
        /// <param name="from">The starting position of the file pointer for the device</param>
        /// <param name="count">The amount of bytes to be read from the device</param>
        public static unsafe void ReadHandle(SafeFileHandle handle, byte[] buffer, long from, uint count)
        {
            fixed (byte* bufferPtr = buffer)
            {
                uint bytesAccessed;

                PInvoke.SetFilePointerEx(handle, from, SET_FILE_POINTER_MOVE_METHOD.FILE_BEGIN);

                PInvoke.ReadFile((Windows.Win32.Foundation.HANDLE)handle.DangerousGetHandle(),
                    bufferPtr, count, &bytesAccessed, null);
            }
        }
    }
}
