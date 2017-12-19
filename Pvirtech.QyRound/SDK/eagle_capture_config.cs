using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pvirtech.QyRound.SDK
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct eagle_capture_config
    {
        public ushort channel_no;  /*!< [in] camera index from 1 to 4 */
        public ushort head_data_size;/*!< padding head data size */
        public uint channel_data_size;/*!< pure data size */
        public ushort capture_status;/*!< 0:init; 1:capturing; 2:recording; 3:paused see ECaptureStatusType*/
    }
    
}
