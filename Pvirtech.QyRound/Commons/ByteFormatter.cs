using System;
using System.Collections.Generic;
using System.Text;

namespace Pvirtech.QyRound.Commons
{
    public static class ByteFormatter
    {
        private const long KB = 1024;
        private const long MB = KB * 1024;
        private const long GB = MB * 1024;

        private const string BFormatPattern = "{0} b";
        private const string KBFormatPattern = "{0} KB";
        private const string MBFormatPattern = "{0} MB";
        private const string GBFormatPattern = "{0:F2} GB";
        private const string GBDataFormatPattern = "{0:F1}";
        public static string ToString(long size)
        {
            if (size < KB)
            {
                return String.Format(BFormatPattern, size);
            }
            else if (size >= KB && size < MB)
            {
                return String.Format(KBFormatPattern, size / 1024.0f);
            }
            else if (size >= MB && size < GB)
            {
                return String.Format(MBFormatPattern, size / MB);
            }
            else // size >= GB
            {
                return String.Format(GBFormatPattern, (double)size / GB);
            }
        }
        public static string ToDataString(long size)
        {
            if (size < KB)
            {
                return String.Format(BFormatPattern, size);
            }
            else if (size >= KB && size < MB)
            {
                return String.Format(KBFormatPattern, size / 1024.0f);
            }
            else if (size >= MB && size < GB)
            {
                return String.Format(MBFormatPattern, size / MB);
            }
            else // size >= GB
            {
                return String.Format(GBDataFormatPattern, (double)size / GB);
            }
        }
    }
}
