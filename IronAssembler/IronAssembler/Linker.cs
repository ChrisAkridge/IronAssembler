﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronAssembler.Data;
using NLog;

namespace IronAssembler
{
	internal static class Linker
	{
		private const uint MagicNumber = 0x45584549; // "IEXE"
		private const uint SpecificationVersion = 0x00010001;
		private const uint AssemblerVersion = 0x00010001;
		private const ulong HeaderSize = 28UL;

		private static Logger logger = LogManager.GetCurrentClassLogger();

		internal static byte[] LinkFile(AssembledFile file, ParsedStringTable table)
		{
			var blockAddresses = GetBlockAddresses(file.Blocks);
			var rewrittenBlocks = RewritePlaceholderAddresses(file.Blocks, blockAddresses);
			var blockBytes = EmitBlocks(rewrittenBlocks);
			var header = EmitHeader((ulong)blockBytes.Length);
			var stringsTable = AssembleStringsTable(table, (ulong)blockBytes.Length + HeaderSize);

			return header.Concat(blockBytes.Concat(stringsTable)).ToArray();
		}

		private static IDictionary<string, ulong> GetBlockAddresses(IReadOnlyList<AssembledBlock> blocks)
		{
			var addresses = new Dictionary<string, ulong>(blocks.Count);

			ulong blockSizeSum = HeaderSize;
			for (int i = 0; i < blocks.Count; i++)
			{
				var block = blocks[i];
				addresses.Add(block.Name, blockSizeSum);
				blockSizeSum += block.BlockSizeInBytes;
			}

			return addresses;
		}

		private static IList<AssembledBlock> RewritePlaceholderAddresses(
			IReadOnlyList<AssembledBlock> blocks, IDictionary<string, ulong> blockAddresses)
		{
			var rewrittenBlocks = new List<AssembledBlock>(blocks.Count);

			foreach (var block in blocks)
			{
				var rewrittenInstructions = new List<AssembledInstruction>(block.Instructions.Count);

				foreach (var instruction in block.Instructions)
				{
					if (instruction.Operand1Label == null && instruction.Operand2Label == null
						&& instruction.Operand3Label == null)
					{
						rewrittenInstructions.Add(instruction);
						continue;
					}

					var rewrittenBytes = instruction.Bytes.ToArray();

					if (instruction.Operand1Label != null)
					{
						RewritePlaceholderAddress(rewrittenBytes, 0xCCCC_CCCC_CCCC_CCCC,
							blockAddresses[instruction.Operand1Label]);
					}
					if (instruction.Operand2Label != null)
					{
						RewritePlaceholderAddress(rewrittenBytes, 0xDDDD_DDDD_DDDD_DDDD,
							blockAddresses[instruction.Operand2Label]);
					}
					if (instruction.Operand3Label != null)
					{
						RewritePlaceholderAddress(rewrittenBytes, 0xEEEE_EEEE_EEEE_EEEE,
							blockAddresses[instruction.Operand3Label]);
					}

					rewrittenInstructions.Add(new AssembledInstruction(rewrittenBytes));
				}
				rewrittenBlocks.Add(new AssembledBlock(block.Name, rewrittenInstructions, block.BlockSizeInBytes));
			}

			return rewrittenBlocks;
		}

		private static byte[] EmitBlocks(IList<AssembledBlock> blocks)
		{
			var bytes = new List<byte>(blocks.Sum(b => (int)b.BlockSizeInBytes));

			foreach (var block in blocks)
			{
				foreach (var instruction in block.Instructions)
				{
					for (int i = 0; i < instruction.Bytes.Count; i++)
					{
						bytes.Add(instruction.Bytes[i]);
					}
				}
			}

			return bytes.ToArray();
		}

		private static byte[] AssembleStringsTable(ParsedStringTable table, ulong stringsTableAddress)
		{
			var utf8Strings = table.Strings.Select(s => Encoding.UTF8.GetBytes(s)).ToList();

			int numberOfStrings = table.Strings.Count;
			var addresses = new List<ulong>(numberOfStrings);

			ulong sumOfStringSizes = 0UL;
			foreach (var utf8String in utf8Strings)
			{
				addresses.Add(sumOfStringSizes);
				sumOfStringSizes += (ulong)utf8String.Length + 4UL;
			}

			List<byte> bytes = new List<byte>(addresses.Count * 8 + 4);
			bytes.WriteIntLittleEndian((uint)numberOfStrings);

			for (int i = 0; i < addresses.Count; i++)
			{
				ulong stringAddress = addresses[i] +    // The address of the string among the strings
					stringsTableAddress +               // The address of the very start of the string table
					4UL +                               // The size of the number of strings
					(ulong)(addresses.Count * 8);       // The sizes of all the pointers
				bytes.WriteLongLittleEndian(stringAddress);
			}

			foreach (var utf8String in utf8Strings)
			{
				bytes.WriteIntLittleEndian((uint)utf8String.Length);
				bytes.AddRange(utf8String);
			}

			return bytes.ToArray();
		}

		private static byte[] EmitHeader(ulong instructionSizeInBytes)
		{
			var bytes = new List<byte>(28);
			bytes.WriteIntLittleEndian(MagicNumber);
			bytes.WriteIntLittleEndian(SpecificationVersion);
			bytes.WriteIntLittleEndian(AssemblerVersion);
			bytes.WriteLongLittleEndian(HeaderSize);    // address of the first instruction
			bytes.WriteLongLittleEndian(HeaderSize + instructionSizeInBytes);
			return bytes.ToArray();
		}

		private static void RewritePlaceholderAddress(byte[] oldBytes, ulong substitution,
			ulong address)
		{
			int replacementIndex = oldBytes.IndexOfSequence(substitution);
			oldBytes[replacementIndex] = (byte)(address & 0xFF);
			oldBytes[replacementIndex + 1] = (byte)((address & 0xFF00) >> 8);
			oldBytes[replacementIndex + 2] = (byte)((address & 0xFF0000) >> 16);
			oldBytes[replacementIndex + 3] = (byte)((address & 0xFF000000) >> 24);
			oldBytes[replacementIndex + 4] = (byte)((address & 0xFF00000000) >> 32);
			oldBytes[replacementIndex + 5] = (byte)((address & 0xFF0000000000) >> 40);
			oldBytes[replacementIndex + 6] = (byte)((address & 0xFF000000000000) >> 48);
			oldBytes[replacementIndex + 7] = (byte)((address & 0xFF00000000000000) >> 56);
		}
	}
}