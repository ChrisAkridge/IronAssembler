using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronAssembler.Data;
using NLog;

namespace IronAssembler
{
    internal static class Linker
    {
        public const uint MagicNumber = 0x45584549; // "IEXE"
        public const uint SpecificationVersion = 0x00020000;
        public const uint AssemblerVersion = 0x00020000;
        public const ulong HeaderSize = 20UL;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        internal static byte[] LinkFile(AssembledFile file, ParsedStringTable table)
        {
            var blockAddresses = GetBlockAddresses(file.Blocks, file.SizeOfGlobalVariableBlock,
                out ulong blocksGlobalsHeaderSize);
            
            var stringsTableAddress = blocksGlobalsHeaderSize;
            var stringAddresses =
                GetStringAddresses(table, stringsTableAddress);
            var rewrittenBlocks = RewritePlaceholderAddresses(file.Blocks, blockAddresses, stringAddresses);
            var blockBytes = EmitBlocks(rewrittenBlocks);
            var header = EmitHeader(file.SizeOfGlobalVariableBlock);
            var globals = new byte[file.SizeOfGlobalVariableBlock];

            var stringsTable = AssembleStringsTable(table, stringsTableAddress);

            return header.Concat(globals).Concat(blockBytes.Concat(stringsTable)).ToArray();
        }

        private static IDictionary<string, ulong> GetBlockAddresses(IReadOnlyList<AssembledBlock> blocks,
            int sizeOfGlobals, out ulong blocksGlobalsHeaderSize)
        {
            var addresses = new Dictionary<string, ulong>(blocks.Count);

            ulong blockSizeSum = HeaderSize + (ulong)sizeOfGlobals;
            foreach (var block in blocks)
            {
                addresses.Add(block.Name, blockSizeSum);
                blockSizeSum += block.BlockSizeInBytes;
            }

            blocksGlobalsHeaderSize = blockSizeSum;
            return addresses;
        }

        private static Dictionary<int, ulong> GetStringAddresses(ParsedStringTable stringTable,
            ulong stringsStartAt)
        {
            var addresses = new Dictionary<int, ulong>(stringTable.Strings.Count);
            ulong currentAddress = stringsStartAt;

            for (var i = 0; i < stringTable.Strings.Count; i++)
            {
                var stringEntry = stringTable.Strings[i];
                int utf8ByteCount = 4 + Encoding.UTF8.GetByteCount(stringEntry);
                addresses.Add(i, currentAddress);
                currentAddress += (ulong)utf8ByteCount;
            }

            return addresses;
        }

        private static IList<AssembledBlock> RewritePlaceholderAddresses(IReadOnlyCollection<AssembledBlock> blocks,
            IDictionary<string, ulong> blockAddresses,
            IDictionary<int, ulong> stringAddresses)
        {
            var rewrittenBlocks = new List<AssembledBlock>(blocks.Count);

            foreach (var block in blocks)
            {
                var rewrittenInstructions = new List<AssembledInstruction>(block.Instructions.Count);

                foreach (var instruction in block.Instructions)
                {
                    if (instruction.Operand1Label == null
                        && instruction.Operand2Label == null
                        && instruction.Operand3Label == null
                        && instruction.Operand1StringIndex < 0
                        && instruction.Operand2StringIndex < 0
                        && instruction.Operand3StringIndex < 0)
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

                    if (instruction.Operand1StringIndex >= 0)
                    {
                        ulong placeholder = 0xAAAAAAAA00000000UL | (ulong)instruction.Operand1StringIndex;
                        RewritePlaceholderAddress(rewrittenBytes, placeholder, stringAddresses[instruction.Operand1StringIndex]);
                    }

                    if (instruction.Operand2StringIndex >= 0)
                    {
                        ulong placeholder = 0xAAAAAAAA00000000UL | (ulong)instruction.Operand2StringIndex;

                        RewritePlaceholderAddress(rewrittenBytes, placeholder,
                            stringAddresses[instruction.Operand2StringIndex]);
                    }

                    if (instruction.Operand3StringIndex >= 0)
                    {
                        ulong placeholder = 0xAAAAAAAA00000000UL | (ulong)instruction.Operand3StringIndex;

                        RewritePlaceholderAddress(rewrittenBytes, placeholder,
                            stringAddresses[instruction.Operand3StringIndex]);
                    }

                    rewrittenInstructions.Add(new AssembledInstruction(rewrittenBytes));
                }
                rewrittenBlocks.Add(new AssembledBlock(block.Name, rewrittenInstructions, block.BlockSizeInBytes));
            }

            return rewrittenBlocks;
        }

        private static IEnumerable<byte> EmitBlocks(IList<AssembledBlock> blocks)
        {
            var bytes = new List<byte>(blocks.Sum(b => (int)b.BlockSizeInBytes));

            foreach (var instruction in blocks.SelectMany(block => block.Instructions))
            {
                bytes.AddRange(instruction.Bytes);
            }

            return bytes.ToArray();
        }

        private static IEnumerable<byte> AssembleStringsTable(ParsedStringTable table, ulong stringsTableAddress)
        {
            var utf8Strings = table.Strings.Select(s => Encoding.UTF8.GetBytes(s)).ToList();
            var bytes = new List<byte>();
            
            foreach (var utf8String in utf8Strings)
            {
                bytes.WriteIntLittleEndian((uint)utf8String.Length);
                bytes.AddRange(utf8String);
            }

            return bytes.ToArray();
        }

        private static IEnumerable<byte> EmitHeader(int globalsSize)
        {
            var bytes = new List<byte>(20);
            bytes.WriteIntLittleEndian(MagicNumber);
            bytes.WriteIntLittleEndian(SpecificationVersion);
            bytes.WriteIntLittleEndian(AssemblerVersion);
            bytes.WriteLongLittleEndian(HeaderSize + (ulong)globalsSize);    // address of the first instruction
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
