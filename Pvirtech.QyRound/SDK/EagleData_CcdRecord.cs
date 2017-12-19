using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pvirtech.QyRound.SDK
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct EagleData_CcdRecord
    {
       		 
        public IntPtr next;  /*!< ccd record list node */
        public EagleData_CcdRecord_Id id;  /*!< Identifier for the ccd record */
        public EagleData_Record_Id record_id;  /*!< Identifier for the record */
        public UInt32 task_index;    /*!< Zero-based index of the task.It is the suffix number of TASKnnnn*/
        public UInt32 record_index;  /*!< Zero-based index of the record.It is the suffix number of RECDnnnn */
        public UInt32 data_size; /*!< The size of data size one frame */
        public UInt32 head_size; /*!< The size of head size one frame */
        public UInt32 frame_number;  /*!< The frame number */


        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string start_time;//[MAX_TIME_BUF_LEN];   /*!< The start time. Format:  YYYYMMDDHHMMSS (eg.20160104120000)*/
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string end_time;//[MAX_TIME_BUF_LEN]; /*!< The end time. Format: YYYYMMDDHHMMSS (eg.20160104120001)*/


        public DATA_SOURCE_TYPE data_source_type;  /*!< Type of the data source */
       public  PIXEL_SAMPLING_TYPE pixel_sampling_type;    /*!< Type of the pixel sampling */
       public UInt32 disk_bitmap;

    }
}
