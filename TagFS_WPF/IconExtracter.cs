using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;

namespace TagsFS_WPF {
    class IconExtracter {
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO {
        public IntPtr hIcon;
        public IntPtr iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };

    class Win32 {
        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0;    // 'Large icon
        public const uint SHGFI_SMALLICON = 0x1;    // 'Small icon

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath,
                                  uint dwFileAttributes,
                                  ref SHFILEINFO psfi,
                                  uint cbSizeFileInfo,
                                  uint uFlags);
    }


    public class IconHelper {


        public Bitmap GetIcon16(string fName) {
            SHFILEINFO shinfo = new SHFILEINFO();

            IntPtr hImgSmall = Win32.SHGetFileInfo(fName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                                            Win32.SHGFI_ICON | Win32.SHGFI_SMALLICON);
            try {
                Bitmap result = (Icon.FromHandle(shinfo.hIcon)).ToBitmap();
                return result;
            }
            catch {
                Bitmap result = new Bitmap("file.png");
                return result;
            }
        }
        public Bitmap GetIcon32(string fName) {
            SHFILEINFO shinfo = new SHFILEINFO();

            IntPtr hImgLarge = Win32.SHGetFileInfo(fName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                                            Win32.SHGFI_ICON | Win32.SHGFI_LARGEICON);

            try {
                Bitmap result = Icon.FromHandle(shinfo.hIcon).ToBitmap();
                return result;
            }
            catch {
                Bitmap result = new Bitmap("file.png");
                return result;
            }
        }

        public List<Icon> GetIcons(string fName) {
            var result = new List<Icon>();

            IntPtr hImgSmall;    //the handle to the system image list
            IntPtr hImgLarge;    //the handle to the system image list
            SHFILEINFO shinfo = new SHFILEINFO();

            hImgSmall = Win32.SHGetFileInfo(fName, 0, ref shinfo,
                                           (uint)Marshal.SizeOf(shinfo),
                                            Win32.SHGFI_ICON |
                                            Win32.SHGFI_SMALLICON);

            {
                Icon myIcon = Icon.FromHandle(shinfo.hIcon);
                result.Add(myIcon);
            }

            hImgLarge = Win32.SHGetFileInfo(fName, 0,
                                      ref shinfo, (uint)Marshal.SizeOf(shinfo),
                                      Win32.SHGFI_ICON | Win32.SHGFI_LARGEICON);
            {
                Icon myIcon = Icon.FromHandle(shinfo.hIcon);
                result.Add(myIcon);
            }
            return result;
        }
    }
}
