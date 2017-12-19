using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pvirtech.QyRound.SDK
{
    public struct eagle_device_time
    {
        public UInt16 wYear;       // current year, 2017eg;
        public UInt16 wMouth;      // [1~12];
        public UInt16 wDay;        // [1~31];
        public UInt16 wHour;       // [0~23];
        public UInt16 wMinite; // [0~59];
        public UInt16 wSecond; // [0~59];
        public UInt16 wMillsecond;// [0~999];

    }
}
