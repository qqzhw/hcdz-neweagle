using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pvirtech.QyRound.SDK
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct EagleData_Record
    {
       public EagleData_Record_Id id; /*!< The identifier for the record */
       public  IntPtr ccd_record_list;   /*!< List of CCD records */
    }
}
