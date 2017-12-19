using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pvirtech.QyRound.SDK
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct eagle_trigger_frequency_config
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] frequency;//[EAGLE_MAX_CAMERA_NUM]=4;
    }
}
