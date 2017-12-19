using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pvirtech.QyRound.SDK
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct eagle_system_config
    {
        public byte data_source;  //1:front data interface; 2:back data interface
        public byte data_type;        //1:Aurora; 2:Rapid IO(SRIO)
        public byte warning_threshold;	// 0 - 100(%)
    }
}
