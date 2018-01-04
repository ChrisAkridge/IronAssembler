using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronAssembler.Data;
using NLog;

namespace IronAssembler
{
	internal static class Assembler
	{
		private const int MaximumInstructionLength = 27;
		private static Logger logger = LogManager.GetCurrentClassLogger();

		#region Operand Matching Regexes
		// Matches a memory address of the form 0x0011223344556677.
		// Match an 0x followed by exactly 16 characters in [0-9A-Fa-f]
		private const string MemoryAddressRegex = @"0x[0-9A-Fa-f]{16}";

		// Matches a memory pointer address of the form *0x0011223344556677.
		// Basically the same as above, except an asterisk comes first.
		private const string MemoryPointerAddressRegex = @"\*0x[0-9A-Fa-f]{16}";

		// Matches a processor register.
		// Just checks if the string is the name of any register.
		private const string RegisterRegex = @"eax|ebx|ecx|edx|eex|efx|egx|ehx|ebp|esp|eip|eflags|erp";

		// Matches a processor register with a pointer.
		// Does the same as above, except it looks for a * first.
		private const string RegisterPointerRegex = @"\*(eax|ebx|ecx|edx|eex|efx|egx|ehx|ebp|esp|eip|eflags|erp)";

		// Matches a processor register with a pointer and offset.
		// Does the same as above, except it looks for a + or - and then one or more digits.
		private const string RegisterPointerWithOffsetRegex = @"\*(eax|ebx|ecx|edx|eex|efx|egx|ehx|ebp|esp|eip|eflags|erp)[+|-]\d+";

		// Matches a numeric literal.
		// Matches one or more digits.
		private const string NumericLiteralRegex = @"\d+";

		// Matches a string table entry.
		// Matches str: then one or more digits.
		private const string StringTableEntryRegex = @"(str:)\d+";

		// Matches a label.
		// Matches any string starting with [A-Za-z_], then with zero or more [A-Za-z0-9_]
		private const string LabelRegex = @"[A-Za-z_][A-Za-z0-9_]*";
		#endregion

		internal static AssembledFile AssembleFile(ParsedFile file)
		{
			var blocks = new List<AssembledBlock>(file.Blocks.Count);

			foreach (var block in file.Blocks)
			{
				blocks.Add(AssembleBlock(block));
			}

			return new AssembledFile(blocks);
		}

		private static AssembledBlock AssembleBlock(ParsedBlock block)
		{
			logger.Trace($"Assembling block {block.Name}");
			var instructions = new List<AssembledInstruction>(block.Instructions.Count);

			foreach (var instruction in block.Instructions)
			{
				instructions.Add(AssembleInstruction(instruction));
			}

			ulong blockSizeInBytes = (ulong)instructions.Sum(i => i.Bytes.Count);
			logger.Trace($"Assembled block {block.Name} with {instructions.Count} instructions, totalling {blockSizeInBytes} bytes");

			return new AssembledBlock(block.Name, instructions, blockSizeInBytes);
		}

		private static AssembledInstruction AssembleInstruction(ParsedInstruction instruction)
		{
			List<byte> bytes = new List<byte>(MaximumInstructionLength);
			byte flagsByte = 0;

			var info = InstructionTable.Lookup(instruction.Mnemonic);
			bytes.WriteShortLittleEndian(info.Opcode);

			flagsByte |= (byte)((int)instruction.Size << 6);

			string[] operandLabels = new string[3];
			if (instruction.Operand1Text != null)
			{
				var type = GetOperandType(instruction.Operand1Text);
				AssembleOperand(bytes, instruction.Operand1Text, type, instruction.Size,
					0, out operandLabels[0]);

				flagsByte |= (byte)(GetFlagsBitsFromOperandType(type) << 4);
			}
			if (instruction.Operand2Text != null)
			{
				var type = GetOperandType(instruction.Operand2Text);
				AssembleOperand(bytes, instruction.Operand2Text, type, instruction.Size,
					1, out operandLabels[1]);

				flagsByte |= (byte)(GetFlagsBitsFromOperandType(type) << 2);
			}
			if (instruction.Operand3Text != null)
			{
				var type = GetOperandType(instruction.Operand3Text);
				AssembleOperand(bytes, instruction.Operand3Text, type, 
					(instruction.Mnemonic != "movln") ? instruction.Size : OperandSize.DWord,
					2,  out operandLabels[2]);

				flagsByte |= GetFlagsBitsFromOperandType(type);
			}

			if (info.NeedsFlags)
			{
				bytes.Insert(2, flagsByte);
			}

			return new AssembledInstruction(bytes, operandLabels[0], operandLabels[1], operandLabels[2]);
		}

		#region Instruction Assembler Helpers
		private static OperandType GetOperandType(string operand)
		{
			if (operand.EntireStringMatchesRegex(MemoryAddressRegex))
			{ 
				return OperandType.MemoryAddress; 
			}
			else if (operand.EntireStringMatchesRegex(MemoryPointerAddressRegex))
			{
				return OperandType.MemoryPointerAddress;
			}
			else if (operand.EntireStringMatchesRegex(RegisterRegex))
			{
				return OperandType.Register;
			}
			else if (operand.EntireStringMatchesRegex(RegisterPointerRegex))
			{
				return OperandType.RegisterWithPointer;
			}
			else if (operand.EntireStringMatchesRegex(RegisterPointerWithOffsetRegex))
			{
				return OperandType.RegisterWithPointerAndOffset;
			}
			else if (operand.EntireStringMatchesRegex(NumericLiteralRegex))
			{
				return OperandType.NumericLiteral;
			}
			else if (operand.EntireStringMatchesRegex(StringTableEntryRegex))
			{
				return OperandType.StringTableEntry;
			}
			else if (operand.EntireStringMatchesRegex(LabelRegex))
			{
				return OperandType.Label;
			}
			else
			{
				throw new AssemblerException($"The operand {operand} could not have its type determined.");
			}
		}

		private static byte GetFlagsBitsFromOperandType(OperandType type)
		{
			switch (type)
			{
				case OperandType.MemoryAddress:
				case OperandType.MemoryPointerAddress:
				case OperandType.Label:
					return 0;
				case OperandType.Register:
				case OperandType.RegisterWithPointer:
				case OperandType.RegisterWithPointerAndOffset:
					return 1;
				case OperandType.NumericLiteral:
					return 2;
				case OperandType.StringTableEntry:
					return 3;
				default:
					throw new AssemblerException("The method was provided with an invalid operand type.");
			}
		}

		private static void AssembleOperand(IList<byte> bytes, string operand, OperandType type, 
			OperandSize size, int operandIndex, out string labelName)
		{
			labelName = null;
			switch (type)
			{
				case OperandType.MemoryAddress:
					AssembleMemoryAddress(bytes, operand);
					break;
				case OperandType.MemoryPointerAddress:
					AssembleMemoryPointerAddress(bytes, operand);
					break;
				case OperandType.Register:
					AssembleRegister(bytes, operand);
					break;
				case OperandType.RegisterWithPointer:
					AssembleRegisterWithPointer(bytes, operand);
					break;
				case OperandType.RegisterWithPointerAndOffset:
					AssembleRegisterWithPointerAndOffset(bytes, operand);
					break;
				case OperandType.NumericLiteral:
					AssembleNumericLiteral(bytes, operand, size);
					break;
				case OperandType.StringTableEntry:
					AssembleStringTableEntry(bytes, operand);
					break;
				case OperandType.Label:
					AssembleLabel(bytes, operandIndex);
					labelName = operand;
					break;
				default:
					throw new AssemblerException("The method was provided with an invalid operand type.");
			}
		}

		private static void AssembleMemoryAddress(IList<byte> bytes, string operand)
		{
			ulong address = operand.Substring(2).ParseAddress();

			if (address >= 0x8000_0000_0000_0000)
			{
				throw new AssemblerException($"No memory address may be above 0x8000000000000000.");
			}

			bytes.WriteLongLittleEndian(address);
		}

		private static void AssembleMemoryPointerAddress(IList<byte> bytes, string operand)
		{
			ulong address = operand.Substring(3).ParseAddress();

			if (address >= 0x8000_0000_0000_0000)
			{
				throw new AssemblerException($"No memory address may be above 0x8000000000000000.");
			}

			address |= 0x8000_0000_0000_0000;
			bytes.WriteLongLittleEndian(address);
		}

		private static void AssembleRegister(IList<byte> bytes, string operand)
		{
			bytes.Add(operand.ParseRegister());
		}

		private static void AssembleRegisterWithPointer(IList<byte> bytes, string operand)
		{
			byte register = operand.Substring(1).ParseRegister();

			bytes.Add((byte)(register | 0x80));
		}

		private static void AssembleRegisterWithPointerAndOffset(IList<byte> bytes,
			string operand)
		{
			int offset;
			char offsetSignChar = (operand.Contains("+")) ? '+' : '-';
			var parts = operand.Split(offsetSignChar);

			string registerName = parts[0].Substring(1);
			string offsetString = parts[1];

			byte register = registerName.ParseRegister();

			if (!int.TryParse(offsetString, out offset))
			{
				throw new AssemblerException($"The offset {offsetString} is not valid.");
			}

			if (offsetSignChar == '-') { offset = -offset; }

			bytes.Add((byte)(register | 0xC0));
			bytes.WriteIntLittleEndian((uint)offset);
		}

		private static void AssembleNumericLiteral(IList<byte> bytes, string operand,
			OperandSize size)
		{
			ulong literal;
			if (!ulong.TryParse(operand, out literal))
			{
				throw new AssemblerException($"The operand {literal} is not valid.");
			}

			switch (size)
			{
				case OperandSize.Byte:
					bytes.Add((byte)literal);
					break;
				case OperandSize.Word:
					bytes.WriteShortLittleEndian((ushort)literal);
					break;
				case OperandSize.DWord:
					bytes.WriteIntLittleEndian((uint)literal);
					break;
				case OperandSize.QWord:
					bytes.WriteLongLittleEndian(literal);
					break;
				case OperandSize.Default:
				default:
					throw new AssemblerException("The method was called with an invalid size.");
			}
		}

		private static void AssembleStringTableEntry(IList<byte> bytes, string operand)
		{
			uint tableIndex;
			if (!uint.TryParse(operand.Split(':')[1], out tableIndex))
			{
				throw new AssemblerException($"{operand} is not a valid index into the string table.");
			}

			bytes.WriteIntLittleEndian(tableIndex);
		}

		private static void AssembleLabel(IList<byte> bytes, int operandIndex)
		{
			switch (operandIndex)
			{
				case 0:
					bytes.WriteLongLittleEndian(0xCCCC_CCCC_CCCC_CCCC);
					break;
				case 1:
					bytes.WriteLongLittleEndian(0xDDDD_DDDD_DDDD_DDDD);
					break;
				case 2:
					bytes.WriteLongLittleEndian(0xEEEE_EEEE_EEEE_EEEE);
					break;
				default:
					break;
			}
		}
		#endregion
	}
}
