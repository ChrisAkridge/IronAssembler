using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronAssembler.Data;
using NLog;

namespace IronAssembler
{
    public static class Translator
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #region Regexes
        // Matches a register containing a pointer with an offset (*eax+0x2DE, *ecx-0x3FD)
        // Match an *, an e, one or more letters, a single + or -, an 0x, then one or more hex digits
        private const string RegisterWithHexadecimalOffsetRegex = @"\*e[a-z]+(\+|-)0x[0-9A-Fa-f]+";

        // Matches a memory address (mem:0x2030, mem:0xFDE2)
        // Matches (case-insensitively) mem:0x, then one or more hex digits
        private const string MemoryAddressRegex = @"(?i)mem:0x[0-9A-Z]+";

        // Matches a hexadecimal number (0x20495, 0x1040A)
        // Matches (case-insensitively) 0x, then one or more hex digits
        private const string HexadecimalNumberRegex = @"(?i)0x[0-9A-Z]+";

        // Matches a floating point number (single(346), double(3.1415962))
        // Matches (case-insensitively), single or double, then (, then either a digit, period, and
        //	one or more digits, OR matches one or more digits, then a )
        private const string FloatingPointNumberRegex = @"(?i)(single|double)\(([0-9]\.[0-9]+|[0-9]+)\)";
        #endregion

        public static IList<string> TranslateFile(string inputFile) => TranslateFile(IO.SplitInputByLine(inputFile));

        public static IList<string> TranslateFile(IList<string> inputFileLines)
        {
            logger.Trace($"Translating file of {inputFileLines.Count} lines");

            inputFileLines = RemoveComments(inputFileLines);
            var translatedLines = new List<string>(inputFileLines.Count);

            foreach (string line in inputFileLines)
            {
                if (string.IsNullOrWhiteSpace(line)) { continue; }
                
                var tokens = line.SplitInstructionLine();

                if (tokens[0].IsLabelLine())
                {
                    // this is a label, skip
                    translatedLines.Add(line);
                    continue;
                }

                var lineBuilder = new StringBuilder();
                int i = tokens.Length - 1;
                while (!tokens[i].IsSizeOperand() && !InstructionTable.TryLookup(tokens[i], out _))
                {
                    string translatedOperand = TranslateOperand(tokens[i]);
                    lineBuilder.Insert(0, " " + translatedOperand);

                    i--;
                }

                while (i >= 0)
                {
                    lineBuilder.Insert(0, " " + tokens[i]);
                    i--;
                }
                translatedLines.Add(lineBuilder.ToString().TrimStart());
            }

            translatedLines = (List<string>)TranslateFloatingPointLiterals(translatedLines);
            translatedLines = (List<string>)BuildStringTableFromProgram(translatedLines);

            return translatedLines;
        }

        private static IList<string> RemoveComments(IEnumerable<string> lines)
        {
            var linesWithoutComments = new List<string>();

            foreach (string line in lines)
            {
                if (line.StartsWith("#", StringComparison.Ordinal)) { linesWithoutComments.Add(""); }
                else if (line.Contains("#"))
                {
                    int sharpIndex = line.IndexOf('#');
                    string lineWithoutComment = line.Substring(0, sharpIndex).Trim();
                    linesWithoutComments.Add(lineWithoutComment);
                }
                else { linesWithoutComments.Add(line); }
            }

            return linesWithoutComments;
        }

        private static string TranslateOperand(string operand)
        {
            if (IsRegisterWithHexadecimalOffset(operand))
            {
                operand = TranslateRegisterWithHexadecimalOffset(operand);
            }
            else if (IsMemoryAddress(operand))
            {
                operand = TranslateMemoryAddress(operand);
            }
            else if (IsHexadecimalNumericLiteral(operand))
            {
                operand = TranslateHexadecimalNumericLiteral(operand);
            }
            return operand;
        }

        private static bool IsRegisterWithHexadecimalOffset(string operand) =>
            operand.EntireStringMatchesRegex(RegisterWithHexadecimalOffsetRegex);

        private static string TranslateRegisterWithHexadecimalOffset(string operand)
        {
            char offsetSignChar = (operand.Contains("+")) ? '+' : '-';
            var operandParts = operand.Split(offsetSignChar);

            long offset = (long)operandParts[1].Substring(2).ParseAddress();
            if (offsetSignChar == '-') { offset = -offset; }
            if (offset < int.MinValue || offset > int.MaxValue)
            {
                throw new TranslationException($"A register offset must be in the range of +/-2.1 billion. The offset received was {offset}.");
            }

            return operandParts[0] + ((offsetSignChar == '+') ? "+" : "") + offset.ToString();
        }

        private static bool IsMemoryAddress(string operand) =>
            operand.EntireStringMatchesRegex(MemoryAddressRegex);

        private static string TranslateMemoryAddress(string operand)
        {
            operand = operand.ToLowerInvariant();
            var operandParts = operand.Split('x');

            string addressPart = operandParts[1].PadLeft(16, '0');
            return "0x" + addressPart;
        }

        private static bool IsHexadecimalNumericLiteral(string operand) =>
            operand.EntireStringMatchesRegex(HexadecimalNumberRegex);

        private static string TranslateHexadecimalNumericLiteral(string operand)
        {
            string numberString = operand.Substring(2);

            if (!ulong.TryParse(numberString, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var number))
            {
                throw new TranslationException($"The hexadecimal number {operand} couldn't be parsed.");
            }

            return number.ToString();
        }

        private static bool IsFloatingPointLiteral(string operand) => operand.EntireStringMatchesRegex(FloatingPointNumberRegex);

        private static IList<string> TranslateFloatingPointLiterals(IList<string> lines)
        {
            var translatedLines = new List<string>(lines.Count);

            foreach (string line in lines)
            {
                if (line.IsLabelLine() || line.ContainsStringLiteral() || line.IsOperandlessInstruction())
                {
                    translatedLines.Add(line);
                    continue;
                }

                var tokens = line.SplitInstructionLine();

                bool sizeTokenOriginallyPresent = false;
                string sizeToken = null;
                if (tokens[1].IsSizeOperand())
                {
                    sizeToken = tokens[1].ToLowerInvariant();
                    sizeTokenOriginallyPresent = true;
                }

                for (int i = (sizeToken != null) ? 2 : 1; i < tokens.Length; i++)
                {
                    if (!IsFloatingPointLiteral(tokens[i])) { continue; }

                    var operandParts = tokens[i].ToLowerInvariant().Split('(');
                    string literalText = operandParts[1].Substring(0, operandParts[1].Length - 1);
                        
                    switch (operandParts[0])
                    {
                        case "single":
                            tokens[i] = FloatToUIntBitwiseString(literalText);
                            break;
                        case "double":
                            tokens[i] = DoubleToULongBitwiseString(literalText);
                            break;
                    }

                    if (sizeToken != null)
                    {
                        switch (operandParts[0])
                        {
                            case "single" when sizeToken != "dword":
                                throw new TranslationException($"A single-precision floating point literal was used when the instruction size is not DWORD.");
                            case "double" when sizeToken != "qword":
                                throw new TranslationException($"A double-precision floating point literal was used when the instruction size is not QWORD.");
                        }
                    }
                    else
                    {
                        sizeToken = (operandParts[0] == "single") ? "DWORD" : "QWORD";
                    }
                }

                string resultLine = tokens[0] + " ";
                resultLine += sizeToken?.ToUpperInvariant() + " ";
                resultLine += string.Join(" ", (sizeTokenOriginallyPresent) ? tokens.Skip(2) : tokens.Skip(1));
                translatedLines.Add(resultLine);
            }
            return translatedLines;
        }

        private static string FloatToUIntBitwiseString(string floatLiteral)
        {
            if (!float.TryParse(floatLiteral, out var literal))
            {
                throw new TranslationException($"The floating point literal {floatLiteral} is not valid.");
            }

            uint literalBits = (uint)(BitConverter.ToInt32(BitConverter.GetBytes(literal), 0));
            return literalBits.ToString();
        }

        private static string DoubleToULongBitwiseString(string doubleLiteral)
        {
            if (!double.TryParse(doubleLiteral, out var literal))
            {
                throw new TranslationException($"The floating point literal {doubleLiteral} is not valid.");
            }

            ulong literalBits = (ulong)BitConverter.DoubleToInt64Bits(literal);
            return literalBits.ToString();
        }

        private static IList<string> BuildStringTableFromProgram(IList<string> lines)
        {
            var stringsTable = new List<string>();
            var linesWithLiterals = new List<int>();

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];

                var quoteIndices = new List<int>();

                for (int j = 0; j < line.Length; j++)
                {
                    if (line[j] != '\"') { continue; }

                    if (j == 0) { throw new TranslationException($"A string literal cannot start an instruction."); }
                    else { quoteIndices.Add(j); }
                }

                if (!quoteIndices.Any()) { continue; }
                else { linesWithLiterals.Add(i); }
                
                if (quoteIndices.Count % 2 != 0) { throw new TranslationException($"Some string literals are not properly terminated.\r\n\t{line}"); }

                for (int quoteIndex = 0; quoteIndex < quoteIndices.Count; quoteIndex += 2)
                {
                    int firstQuoteIndex = quoteIndices[quoteIndex];
                    int secondQuoteIndex = quoteIndices[quoteIndex + 1];
                    int literalLength = secondQuoteIndex - firstQuoteIndex;

                    string literal = line.Substring(firstQuoteIndex, literalLength + 1);
                    if (!stringsTable.Contains(literal))
                    {
                        stringsTable.Add(literal);
                    }
                }
            }

            foreach (var lineIndex in linesWithLiterals)
            {
                for (int i = 0; i < stringsTable.Count; i++)
                {
                    lines[lineIndex] = lines[lineIndex].Replace(stringsTable[i], "str:" + i.ToString());
                }

                var tokens = lines[lineIndex].SplitInstructionLine();
                if (tokens[1].ToLowerInvariant() != "qword" && tokens[0] != "hwcall")
                {
                    lines[lineIndex] = tokens[0]
                        + " "
                        + "QWORD "
                        + string.Join(" ", tokens.Skip(1));
                }
            }

            return lines.Concat(BuildStringsTable(stringsTable)).ToList();
        }

        private static IEnumerable<string> BuildStringsTable(IEnumerable<string> stringsTable)
        {
            var stringsTableLines = new List<string> {"strings:"};

            stringsTableLines.AddRange(stringsTable.Select((t, i) => i.ToString() + ": " + t));

            return stringsTableLines;
        }
    }
}
