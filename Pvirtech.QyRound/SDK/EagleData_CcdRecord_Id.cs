using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pvirtech.QyRound.SDK
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct EagleData_CcdRecord_Id
    {
        public UInt32 device_id; /*!< Identifier for the device */
        public UInt32 ccd_serial;	/*!< The CCD serial, 1,2,3,4 from the first camera to the fourth */
    }
}
