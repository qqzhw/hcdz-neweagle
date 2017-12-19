using System;

namespace Pvirtech.QyRound.Commons
{
	public class CCITTCRC16
	{
		private ushort m_InitialValue = 0;

		public ushort InitialValue
		{
			get
			{
				return this.m_InitialValue;
			}
			set
			{
				this.m_InitialValue = value;
			}
		}

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
			int tmpValue = (int)this.InitialValue;
			for (int i = 0; i < data.Length; i++)
			{
				tmpValue ^= (int)data[i] << 8;
				for (int j = 0; j < 8; j++)
				{
					if ((tmpValue & 32768) != 0)
					{
						tmpValue = (tmpValue << 1 ^ 4129);
					}
					else
					{
						tmpValue <<= 1;
					}
				}
			}
			this.HighByte = (byte)(tmpValue & 65280);
			this.LowByte = (byte)(tmpValue & 255);
			return (ushort)tmpValue;
		}
	}
}
