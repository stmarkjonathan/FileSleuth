using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell;

namespace FileSleuth.FileManager
{
    internal class FileIconHandler
    {
        Dictionary<string, BitmapSource> extensionIconSrcs;
        public FileIconHandler()
        {
           extensionIconSrcs = new Dictionary<string, BitmapSource>();
        }

        public void TestIcon(FileData fileInfo)
        {

            string filePath = fileInfo.FilePath;

            string pathExtension = Path.GetExtension(filePath);

            if(extensionIconSrcs.ContainsKey(pathExtension))
            {
                fileInfo.FileIcon = extensionIconSrcs[pathExtension];
                return;
            }

            IShellItemImageFactory shellImageFactory;

            SIZE iconSize = new SIZE(16, 16);

            DeleteObjectHBITMAPSafeHandle iconBitmap;

            //this is very memory heavy, maybe dictionary / cache aleayd seen icons
            //if hresult not succeed, use default
            HRESULT shellImageFactory_hResult = PInvoke.SHCreateItemFromParsingName(filePath, null, out shellImageFactory);

            if (shellImageFactory_hResult.Succeeded)
            {
                shellImageFactory.GetImage(iconSize, SIIGBF.SIIGBF_ICONONLY, out iconBitmap);

                BitmapSource iconSrc = Imaging.CreateBitmapSourceFromHBitmap(iconBitmap.DangerousGetHandle(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                iconSrc.Freeze();

                fileInfo.FileIcon = iconSrc;
                extensionIconSrcs[pathExtension] = iconSrc;

                iconBitmap.Close();
            }
        }
    }
}


