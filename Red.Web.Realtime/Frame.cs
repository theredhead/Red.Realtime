using System;
using System.Linq;
using System.Text;

namespace Red.Web.Realtime
{
	public class Frame
	{
		private byte[] _bytes;
		private byte[] _dataBytes;
		private byte[] _maskBytes;
		private byte[] _decodedData;


		public Frame(byte[] bytes)
		{
			_bytes = bytes;

			int lengthIndicator = _bytes[1] & 127;
			switch (lengthIndicator)
			{
				case 126:
					_maskBytes = Slice(bytes, 4, 8);
					_dataBytes = Slice(bytes, 8);
					break;
				case 127:
					_maskBytes = Slice(bytes, 10, 14);
					_dataBytes = Slice(bytes, 14);
					break;
				default:
					_maskBytes = Slice(bytes, 2, 6);
					_dataBytes = Slice(bytes, 6);
					break;
			}
		}

		public bool IsFinalFrame
		{
			get
			{
				return (_bytes[0] & 128) == 128;
			}
		}

		public long Length { get { return _dataBytes.Length; } }


		private static T[] Slice<T>(T[] data, int index, int length = -1)
		{
			if (length == -1)
				length = data.Length - index;

			T[] result = new T[length];
			Array.Copy(data, index, result, 0, length);
			return result;
		}
						               
		public byte[] DecodedData
		{
			get
			{
				if (_decodedData == null)
				{
					_decodedData = _dataBytes.Select((b, ix) =>
									{
										return (byte)(b ^ _maskBytes[ix % 4]);
									}).ToArray();
				}
				return _decodedData;
			}
		}

		public string MessageText { get { return Encoding.UTF8.GetString(DecodedData); } }
	}
}
