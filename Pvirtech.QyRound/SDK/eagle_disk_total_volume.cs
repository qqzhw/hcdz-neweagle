using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pvirtech.QyRound.SDK
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct eagle_disk_total_volume
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public UInt32[] total_volume;		//max 8 disk per eagle device
    }
}
