using System;

namespace RackSys.TestLab.Instrument
{
	public class XOR16
	{
		public byte HighByte
		{
			get;
			set;
		}

		public byte LowByte
		{
			get;
			set;
		}

		public  ushort ExecuteCheck(byte[] data)
		{
			int tmpValue = 0;
			for (int i = 0; i < data.Length; i++)
			{
				tmpValue ^= (int)data[i];
			}
			this.HighByte = (byte)(tmpValue & 65280);
			this.LowByte = (byte)(tmpValue & 255);
			return (ushort)tmpValue;
		}
	}
}
