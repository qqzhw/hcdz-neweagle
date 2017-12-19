using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pvirtech.QyRound.SDK
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public  struct eagle_reocrd_status
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public UInt32[]   record_time;            //record time since started, in ms;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public UInt32[] record_size_low_part;       //low part of current record size, in kilo bytes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public UInt32[] record_size_high_part;	//high part of current record size;
    }
}
