using System;

namespace Pvirtech.QyRound.Commons
{
	public class CRC16
	{
		private ushort m_InitialValue = 65535;

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

		public ushort ExecuteCheck(byte[] data)
		{
			int tmpValue = (int)this.InitialValue;
			for (int i = 0; i < data.Length; i++)
			{
				tmpValue ^= (int)data[i];
				for (int j = 0; j < 8; j++)
				{
					if (1 == (tmpValue & 1))
					{
						tmpValue >>= 1;
						tmpValue ^= 40961;
					}
					else
					{
						tmpValue >>= 1;
					}
				}
			}
			this.HighByte = (byte)((tmpValue & 65280) >> 8);
			this.LowByte = (byte)(tmpValue & 255);
			return (ushort)tmpValue;
		}
	}
}
