using System.Collections.Generic;
using System.Linq;
using IronAssembler.Data;
using NLog;

namespace IronAssembler
{
    /// <summary>
    /// Converts parsed elements of an assembly file into assembled elements of bytes.
    /// </summary>
    internal static class Assembler
    {
        private const int MaximumInstructionLength = 27;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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

        /// <summary>
        /// Assembles a parsed file.
        /// </summary>
        /// <param name="file">The parsed file to assemble.</param>
        /// <returns>An assembled file containing each block and the string table assembled into bytes.</returns>
        internal static AssembledFile AssembleFile(ParsedFile file)
        {
            var blocks = new List<AssembledBlock>(file.Blocks.Count);
            blocks.AddRange(file.Blocks.Select(AssembleBlock));

            return new AssembledFile(blocks, file.SizeOfGlobalVariableBlock);
        }

        private static AssembledBlock AssembleBlock(ParsedBlock block)
        {
            logger.Trace($"Assembling block {block.Name}");
            var instructions = new List<AssembledInstruction>(block.Instructions.Count);
            instructions.AddRange(block.Instructions.Select(AssembleInstruction));

            ulong blockSizeInBytes = (ulong)instructions.Sum(i => i.Bytes.Count);
            logger.Trace($"Assembled block {block.Name} with {instructions.Count} instructions, totalling {blockSizeInBytes} bytes");

            return new AssembledBlock(block.Name, instructions, blockSizeInBytes);
        }

        private static AssembledInstruction AssembleInstruction(ParsedInstruction instruction)
        {
            var bytes = new List<byte>(MaximumInstructionLength);
            byte flagsByte = 0;

            var info = InstructionTable.Lookup(instruction.Mnemonic);
            
            // These instructions don't have a right operand but
            // we'll still emit a 00 for the right operand in the
            // flags byte because that's what IronArc expects
            var isUnaryLongInstruction = (info.Mnemonic == "incl") || (info.Mnemonic == "decl") || (info.Mnemonic == "bwnotl") || (info.Mnemonic == "lnotl"); 
            bytes.WriteShortLittleEndian(info.Opcode);

            flagsByte |= (byte)((int)instruction.Size << 6);

            var operandLabels = new string[3];
            int[] stringTableIndices =
            {
                -1, -1, -1
            };
            var operand1Size = info.ImplicitSizes.TryGet(0) ?? instruction.Size;
            var operand2Size = info.ImplicitSizes.TryGet(1) ?? instruction.Size;
            var operand3Size = info.ImplicitSizes.TryGet(2) ?? instruction.Size;
            
            if (instruction.Operand1Text != null)
            {
                var type = GetOperandType(instruction.Operand1Text);
                AssembleOperand(bytes, instruction.Operand1Text, type, operand1Size,
                    0, out operandLabels[0], out stringTableIndices[0]);

                flagsByte |= (byte)(GetFlagsBitsFromOperandType(type) << 4);
            }

            if (instruction.Operand2Text != null)
            {
                var type = GetOperandType(instruction.Operand2Text);
                AssembleOperand(bytes, instruction.Operand2Text, type, operand2Size,
                    1, out operandLabels[1], out stringTableIndices[1]);

                flagsByte |= (byte)(GetFlagsBitsFromOperandType(type) << 2);
            }

            if (instruction.Operand3Text != null)
            {
                var type = GetOperandType(instruction.Operand3Text);
                AssembleOperand(bytes, instruction.Operand3Text, type, 
                    (instruction.Mnemonic != "movln") ? operand3Size : OperandSize.DWord,
                    2, out operandLabels[2], out stringTableIndices[2]);

                flagsByte |= GetFlagsBitsFromOperandType(type);
            }
            else if (isUnaryLongInstruction)
            {
                // For incl, decl, bwnotl, and lnotl, there is no right operand, so the destination
                // operand becomes the second operand. However, we still need to emit two 00 bits
                // in the space of the second operand so that the destination type bits are always
                // in the lowest-order bits.

                flagsByte = (byte)((flagsByte & 0xF0) | ((flagsByte & 0x0F) >> 2));
            }

            if (info.NeedsFlags)
            {
                bytes.Insert(2, flagsByte);
            }

            return new AssembledInstruction(bytes,
                operandLabels[0], operandLabels[1], operandLabels[2],
                stringTableIndices[0], stringTableIndices[1], stringTableIndices[2]);
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
                    return 0;
                case OperandType.Register:
                case OperandType.RegisterWithPointer:
                case OperandType.RegisterWithPointerAndOffset:
                    return 1;
                case OperandType.Label:
                case OperandType.StringTableEntry:
                case OperandType.NumericLiteral:
                    return 2;
                default:
                    throw new AssemblerException("The method was provided with an invalid operand type.");
            }
        }

        private static void AssembleOperand(IList<byte> bytes, string operand, OperandType type, 
            OperandSize size, int operandIndex, out string labelName, out int stringTableIndex)
        {
            labelName = null;
            stringTableIndex = -1;
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
                    stringTableIndex = int.Parse(operand.Split(':')[1]);
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
                throw new AssemblerException("No memory address may be above 0x8000000000000000.");
            }

            bytes.WriteLongLittleEndian(address);
        }

        private static void AssembleMemoryPointerAddress(IList<byte> bytes, string operand)
        {
            ulong address = operand.Substring(3).ParseAddress();

            if (address >= 0x8000_0000_0000_0000)
            {
                throw new AssemblerException("No memory address may be above 0x8000000000000000.");
            }

            address |= 0x8000_0000_0000_0000;
            bytes.WriteLongLittleEndian(address);
        }

        private static void AssembleRegister(ICollection<byte> bytes, string operand)
        {
            bytes.Add(operand.ParseRegister());
        }

        private static void AssembleRegisterWithPointer(ICollection<byte> bytes, string operand)
        {
            byte register = operand.Substring(1).ParseRegister();

            bytes.Add((byte)(register | 0x80));
        }

        private static void AssembleRegisterWithPointerAndOffset(IList<byte> bytes,
            string operand)
        {
            char offsetSignChar = (operand.Contains("+")) ? '+' : '-';
            var parts = operand.Split(offsetSignChar);

            string registerName = parts[0].Substring(1);
            string offsetString = parts[1];

            byte register = registerName.ParseRegister();

            if (!int.TryParse(offsetString, out var offset))
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
            if (!ulong.TryParse(operand, out var literal))
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
            if (!uint.TryParse(operand.Split(':')[1], out var tableIndex))
            {
                throw new AssemblerException($"{operand} is not a valid index into the string table.");
            }

            bytes.WriteLongLittleEndian(0xAAAAAAAA00000000UL | tableIndex);
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
            }
        }
        #endregion
    }
}
