using Pvirtech.QyRound.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pvirtech.QyRound.Models
{
    public class CcdRecordModel
    {
        public string TaskName { get; set; }
        public string RecordName { get; set; }
        public string TotalSize { get; set; }
        public EagleData_CcdRecord_Id id { get; set; }
        public EagleData_Record_Id record_id  { get; set; }
       public UInt32 task_index { get; set; }
        public UInt32 record_index { get; set; }
        public UInt32 data_size { get; set; }
        public UInt32 head_size { get; set; }
        public UInt32 frame_number { get; set; }

        private string _startTime;
        public string start_time {
            get
            { return _startTime; }
            set
            {
                _startTime = value;
                try
                {
                    StartTimeText = DateTime.ParseExact(_startTime, "yyyyMMddHHmmss", null).ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch
                {
                    StartTimeText = "1900-01-01 00:00:00";
                }
               
            }
              

    }//[MAX_TIME_BUF_LEN];   /*!< The start time. Format:  YYYYMMDDHHMMSS (eg.20160104120000)*/

        private string _endTime;
        public string end_time
        {
            get
            { return _endTime; }
            set
            {
                _endTime = value;
                try
                {
                    EndTimeText = DateTime.ParseExact(_endTime, "yyyyMMddHHmmss", null).ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch 
                {
                    EndTimeText = "1900-01-01 00:00:00";
                }
               
            }
        }
         
        public DATA_SOURCE_TYPE data_source_type { get; set; }  /*!< Type of the data source */
        public PIXEL_SAMPLING_TYPE pixel_sampling_type { get; set; }    /*!< Type of the pixel sampling */
        public UInt32 disk_bitmap { get; set; }
        public string StartTimeText { get; set; }    
        public string EndTimeText { get; set; }
    }
}
