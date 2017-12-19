using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pvirtech.QyRound.SDK
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct EagleData_Record_Id
    {
        //public EagleData_Record_Id(int s)
        //{
        //    start_time = 0;
        //    task_name = new char[64];
        //}

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]

        public string task_name;//[MAX_NAME_BUF_LEN]=64;    /*!< task name */

        public long start_time; /*!< Cameras start record at the same time will be classified into same record */
        
    }
}
