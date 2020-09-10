using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IronAssembler.Data;

namespace IronAssembler
{
    public static class Disassembler
    {
        private const uint MaximumSupportedSpecVersion = 0x00020000;
        private const uint MaximumSupportedAssemblerVersion = 0x00020000;
        private const int AddressColumnLength = 20; // 0x (2 chars), then the address (16 chars), then two spaces
        private const ulong HeaderSize = 20UL;
        internal const string IllegalInstruction = "?? ??";

        public static string DisassembleProgram(byte[] program, bool displayAddress = false, bool displayBytes = false)
        {
            var programBuilder = new StringBuilder();

            var addressLines = new List<string>();
            var instructionLines = new List<string>();
            var byteLines = new List<string>();

            using (var stream = new BinaryReader(new MemoryStream(program)))
            {
                uint magicNumber = stream.ReadUInt32();
                if (magicNumber != Linker.MagicNumber) { return "Not an IronArc program."; }

                uint specVersion = stream.ReadUInt32();
                if (specVersion > MaximumSupportedSpecVersion) { return "Program not supported."; }

                uint assemblerVersion = stream.ReadUInt32();
                if (assemblerVersion > MaximumSupportedAssemblerVersion) { return "Program not supported."; }

                ulong firstInstructionAddress = stream.ReadUInt64();
                uint globalsSize = (uint)(firstInstructionAddress - HeaderSize);
                ulong stringsTableAddress = stream.ReadUInt64();
                ulong instructionsLength = (stringsTableAddress - firstInstructionAddress);

                programBuilder.AppendLine($"globals: {globalsSize}");

                stream.BaseStream.Seek((long)firstInstructionAddress, SeekOrigin.Begin);

                programBuilder.AppendLine("main:");

                ulong instructionBytesProcessed = 0UL;
                while (instructionBytesProcessed < instructionsLength)
                {
                    addressLines.Add((displayAddress) ? stream.BaseStream.Position.FormatLongAsHex() : "");

                    instructionLines.Add(DisassembleInstruction(stream, out int instructionLength, out string instructionBytes));

                    byteLines.Add((displayBytes) ? instructionBytes : "");

                    instructionBytesProcessed += (ulong)instructionLength;
                }

                int instructionColumnLength = instructionLines.Max(i => i.Length) + 2;

                for (int i = 0; i < addressLines.Count; i++)
                {
                    if (displayAddress)
                    {
                        programBuilder.Append(addressLines[i].PadRight(AddressColumnLength, ' '));
                    }

                    programBuilder.Append(instructionLines[i].PadRight(instructionColumnLength, ' '));

                    if (displayBytes)
                    {
                        programBuilder.Append(byteLines[i]);
                    }

                    programBuilder.TrimEnd().AppendLine();
                }

                programBuilder.AppendLine(DisassembleStringTable(stream));
            }

            return programBuilder.ToString();
        }

        public static string DisassembleInstruction(byte[] memory, int offset, out int instructionLength,
            out string instructionBytes)
        {
            using (var stream = new BinaryReader(new MemoryStream(memory)))
            {
                stream.BaseStream.Seek(offset, SeekOrigin.Begin);
                return DisassembleInstruction(stream, out instructionLength, out instructionBytes);
            }
        }

        public static string DisassembleInstruction(BinaryReader memory, out int instructionLength,
            out string instructionBytes)
        {
            var disassembledInstructionBuilder = new StringBuilder();
            long oldStreamPosition = memory.BaseStream.Position;

            ushort opcode = memory.ReadUInt16();

            if (!InstructionTable.TryLookupByOpcode(opcode, out var info))
            {
                instructionLength = 2;
                instructionBytes = StreamBytesToString(memory, oldStreamPosition, 2);
                return IllegalInstruction;
            }

            bool hasFlagsByte = info.NeedsFlags;
            int operandCount = info.OperandCount;
            bool isUnaryLongInstruction = (info.Mnemonic == "incl") || (info.Mnemonic == "decl") || (info.Mnemonic == "bwnotl") || (info.Mnemonic == "lnotl");
            OperandSize? size = null;
            var operandTypes = new OperandType?[3];

            if (hasFlagsByte)
            {
                byte flagsByte = memory.ReadByte();
                if (info.NeedsSize)
                {
                    byte sizeBits = (byte)((flagsByte & 0xC0) >> 6);
                    size = (OperandSize)sizeBits;
                }

                for (int i = 0; i < operandCount; i++)
                {
                    if (isUnaryLongInstruction && i == 1) { i = 2; }
                    byte shiftAmount = (byte)((2 - i) * 2);
                    byte operandMask = (byte)(3 << shiftAmount);
                    byte operandBits = (byte)((flagsByte & operandMask) >> shiftAmount);
                    switch (operandBits)
                    {
                        case 0: operandTypes[i] = OperandType.MemoryAddress; break;
                        case 1: operandTypes[i] = OperandType.Register; break;
                        case 2: operandTypes[i] = OperandType.NumericLiteral; break;
                        case 3: operandTypes[i] = OperandType.StringTableEntry; break;
                        default: throw new InvalidOperationException($"Invalid operand type {operandBits}");
                    }
                }
            }

            disassembledInstructionBuilder.Append(info.Mnemonic + " ");
            if (size != null) { disassembledInstructionBuilder.Append(size.ToString().ToUpperInvariant() + " "); }

            foreach (var type in operandTypes.Where(type => type != null))
            {
                disassembledInstructionBuilder.Append(DisassembleOperand(memory, size, type.Value) + " ");
            }

            instructionLength = (int)(memory.BaseStream.Position - oldStreamPosition);
            instructionBytes = StreamBytesToString(memory, oldStreamPosition, instructionLength);

            return "\t" + disassembledInstructionBuilder.ToString().TrimEnd();
        }

        private static string StreamBytesToString(BinaryReader stream, long startingPosition, int count)
        {
            var bytes = new byte[count];
            stream.BaseStream.Seek(startingPosition, SeekOrigin.Begin);
            stream.Read(bytes, 0, count);
            return string.Join(" ", bytes.Select(b => b.ToString("X2")));
        }

        private static string DisassembleOperand(BinaryReader stream, OperandSize? size, OperandType type)
        {
            switch (type)
            {
                case OperandType.MemoryAddress:
                    return DisassembleMemoryAddress(stream);
                case OperandType.Register:
                    return DisassembleRegisterOperand(stream);
                case OperandType.NumericLiteral:
                    return DisassembleNumericLiteral(stream, size);
                case OperandType.StringTableEntry:
                    return DisassembleStringTableIndex(stream);
                default:
                    throw new InvalidOperationException($"Invalid operand type {type}.");
            }
        }

        private static string DisassembleMemoryAddress(BinaryReader stream)
        {
            ulong operand = stream.ReadUInt64();
            bool isPointer = (operand & 0x8000_0000_0000_0000) == 1;
            ulong address = (operand & 0x7FFF_FFFF_FFFF_FFFF);

            return ((isPointer) ? "*" : "") + "0x" + address.ToString("X16");
        }

        private static string DisassembleRegisterOperand(BinaryReader stream)
        {
            byte operand = stream.ReadByte();
            bool isPointer = (operand & 0x80) != 0;
            bool hasOffset = (operand & 0x40) != 0;
            var register = (Register)(operand & 0x3F);
            var offset = (hasOffset) ? stream.ReadInt32() : (int?)null;

            string registerString = ((isPointer) ? "*" : "") + register.ToString().ToLowerInvariant();
            string offsetString = "";

            if (!hasOffset) { return registerString + offsetString; }

            if (!isPointer) { return "~REGISTER HAS OFFSET WITHOUT BEING A POINTER~"; }
            offsetString = (offset > 0) ? ("+" + offset.ToString()) : (offset.ToString());

            return registerString + offsetString;
        }

        private static string DisassembleNumericLiteral(BinaryReader stream, OperandSize? size)
        {
            if (size == null)
            {
                // The only valid way to get here is if our instruction is movln, which uses
                // an implied size of QWORD.
                size = OperandSize.QWord;
            }

            switch (size.Value)
            {
                case OperandSize.Byte: return stream.ReadByte().ToString();
                case OperandSize.Word: return stream.ReadUInt16().ToString();
                case OperandSize.DWord: return stream.ReadUInt32().ToString();
                case OperandSize.QWord: return stream.ReadUInt64().ToString();
                case OperandSize.Default:
                default: throw new InvalidOperationException($"Invalid size {size}.");
            }
        }

        private static string DisassembleStringTableIndex(BinaryReader stream) =>
            "str:" + stream.ReadUInt32().ToString();

        private static string DisassembleStringTable(BinaryReader stream)
        {
            var stringTableBuilder = new StringBuilder();
            stringTableBuilder.AppendLine("strings:");

            while (stream.BaseStream.Length - stream.BaseStream.Position >= 4)
            {
                var fourBytes = stream.ReadBytes(4);
                string hexBytes = string.Join(" ", fourBytes.Select(x => x.ToString("X2")));
                string asciiBytes = Encoding.ASCII.GetString(fourBytes);
                stringTableBuilder.AppendLine($"{hexBytes} {asciiBytes}");
            }

            if (stream.BaseStream.Length - stream.BaseStream.Position == 3)
            {
                var threeBytes = stream.ReadBytes(3);
                string hexBytes = string.Join(" ", threeBytes.Select(x => x.ToString("X2"))) + "   ";
                string asciiBytes = Encoding.ASCII.GetString(threeBytes);
                stringTableBuilder.AppendLine($"{hexBytes} {asciiBytes}");
            }

            if (stream.BaseStream.Length - stream.BaseStream.Position == 2)
            {
                var twoBytes = stream.ReadBytes(2);
                string hexBytes = string.Join(" ", twoBytes.Select(x => x.ToString("X2"))) + "      ";
                string asciiBytes = Encoding.ASCII.GetString(twoBytes);
                stringTableBuilder.AppendLine($"{hexBytes} {asciiBytes}");
            }

            if (stream.BaseStream.Length - stream.BaseStream.Position == 1)
            {
                var oneByte = stream.ReadByte();
                string hexBytes = string.Join(" ", oneByte.ToString("X2")) + "         ";
                var asciiBytes = (char)oneByte;
                stringTableBuilder.AppendLine($"{hexBytes} {asciiBytes}");
            }

            return stringTableBuilder.ToString();
        }
    }
}
