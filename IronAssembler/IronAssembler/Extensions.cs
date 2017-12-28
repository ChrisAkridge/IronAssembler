using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IronAssembler
{
	internal static class Extensions
	{
		public static void WriteShortLittleEndian(this IList<byte> bytes, ushort data)
		{
			bytes.Add((byte)(data & 0xFF));
			bytes.Add((byte)(data >> 8));
		}

		public static void WriteIntLittleEndian(this IList<byte> bytes, uint data)
		{
			bytes.Add((byte)(data & 0xFF));
			bytes.Add((byte)((data & 0xFF00) >> 8));
			bytes.Add((byte)((data & 0xFF0000) >> 16));
			bytes.Add((byte)((data & 0xFF000000) >> 24));
		}

		public static void WriteLongLittleEndian(this IList<byte> bytes, ulong data)
		{
			bytes.Add((byte)(data & 0xFF));
			bytes.Add((byte)((data & 0xFF00) >> 8));
			bytes.Add((byte)((data & 0xFF0000) >> 16));
			bytes.Add((byte)((data & 0xFF000000) >> 24));
			bytes.Add((byte)((data & 0xFF00000000) >> 32));
			bytes.Add((byte)((data & 0xFF0000000000) >> 40));
			bytes.Add((byte)((data & 0xFF000000000000) >> 48));
			bytes.Add((byte)((data & 0xFF00000000000000) >> 56));
		}

		public static bool IsAllASCIILetters(this string s)
		{
			return s.All(c => (c >= 65 && c <= 90) || (c >= 97 && c <= 122));
		}

		public static bool EntireStringMatchesRegex(this string s, string regex)
		{
			return Regex.Match(s, regex).Value == s;
		}

		public static ulong ParseAddress(this string s)
		{
			ulong address;
			if (!ulong.TryParse(s, out address))
			{
				throw new AssemblerException($"The operand {s} is not a valid memory address.");
			}

			return address;
		}

		public static byte ParseRegister(this string s)
		{
			Register register;
			if (!Enum.TryParse(s, true, out register))
			{
				throw new AssemblerException($"The register {s} does not exist.");
			}

			return (byte)register;
		}
	}
}
