using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pvirtech.QyRound
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Device
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string device_name; /*!< name of the nic */
    }
}
