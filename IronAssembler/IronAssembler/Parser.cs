﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IronAssembler.Data;
using NLog;

namespace IronAssembler
{
    internal static class Parser
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal static ParsedFile ParseFile(IList<string> programLines)
        {
            logger.Trace($"Parsing file of {programLines.Count} line(s)");
            var labelLocations = GetLabelLocations(programLines, out int stringsLabelLocation,
                out int sizeOfGlobalVariableBlock);
            var blocks = ParseBlocks(programLines, labelLocations);
            var stringsTable = ParseStringsTable(programLines, stringsLabelLocation);

            return new ParsedFile(blocks, stringsTable, sizeOfGlobalVariableBlock);
        }

        private static Dictionary<string, int> GetLabelLocations(IList<string> lines, out int stringsLabelIndex,
            out int sizeOfGlobalVariableBlock)
        {
            var labelLocations = new Dictionary<string, int>();
            int stringsLabelLineIndex = -1;
            sizeOfGlobalVariableBlock = -1;

            if (!IsLabelLine(lines.First()))
            {
                throw new ParsingException("The assembly program does not begin with a label.", 0);
            }

            for (int i = 0; i < lines.Count(); i++)
            {
                string line = lines[i];

                if (!IsLabelLine(line)) { continue; }

                if (line.ToLowerInvariant().StartsWith("globals:", StringComparison.Ordinal))
                {
                    if (i != 0)
                    {
                        throw new ParsingException("The globals size label must be the first line of the program.", 0);
                    }
                    sizeOfGlobalVariableBlock = int.Parse(line.Split(' ').Last());
                    continue;
                }
                if (line.ToLowerInvariant() == "strings:")
                {
                    if (stringsLabelLineIndex != -1)
                    {
                        throw new ParsingException("The assembly program has multiple strings tables.", i);
                    }
                    stringsLabelLineIndex = i;
                }
                else
                {
                    string labelNameWithoutColon = line.Substring(0, line.Length - 1);
                    if (labelLocations.ContainsKey(labelNameWithoutColon))
                    {
                        throw new ParsingException($"The label {labelNameWithoutColon} is defined multiple times.", i);
                    }
                    labelLocations.Add(labelNameWithoutColon, i);
                }
            }

            logger.Trace($"Parsed {lines.Count} lines, found {labelLocations.Count} label(s)");
            stringsLabelIndex = stringsLabelLineIndex;

            if (sizeOfGlobalVariableBlock == -1)
            {
                throw new ParsingException("No globals size label was found.", 0);
            }

            return labelLocations;
        }

        private static IEnumerable<ParsedBlock> ParseBlocks(IList<string> lines, IDictionary<string, int> labelLocations)
        {
            var blocks = new List<ParsedBlock>();
            var currentBlockInstructions = new List<ParsedInstruction>();

            foreach (var kvp in labelLocations)
            {
                logger.Trace($"Parsing block {kvp.Key}");
                for (int i = kvp.Value + 1; i <= lines.Count; i++)
                {
                    string line = lines[i];
                    if (IsLabelLine(line))
                    {
                        if (i == kvp.Value + 1) { throw new ParsingException($"The block labelled {kvp.Key} has no instructions.", i); }
                        else
                        {
                            blocks.Add(new ParsedBlock(kvp.Key, currentBlockInstructions));
                            currentBlockInstructions = new List<ParsedInstruction>();
                            break;
                        }
                    }

                    currentBlockInstructions.Add(ParseInstruction(line, i));
                }
            }

            logger.Trace($"Parsed {labelLocations.Count} blocks");
            return blocks;
        }

        private static ParsedStringTable ParseStringsTable(IList<string> lines, int stringsLabelIndex)
        {
            logger.Trace("Parsing strings table");

            var parsedStrings = new List<string>();

            for (int i = stringsLabelIndex + 1; i < lines.Count; i++)
            {
                string line = lines[i];
                if (IsLabelLine(line))
                {
                    if (!parsedStrings.Any())
                    {
                        throw new ParsingException("The strings table had no entries.", i);
                    }

                    break;
                }

                int expectedStringTableIndex = i - (stringsLabelIndex + 1);
                string entry = ParseString(line, out var reportedStringTableIndex, i);
                if (reportedStringTableIndex != expectedStringTableIndex)
                {
                    throw new ParsingException($"A string in the table has the wrong index; expected {i} but got {reportedStringTableIndex}.", i);
                }
                parsedStrings.Add(entry);
            }

            logger.Trace($"Parsed strings table ({parsedStrings.Count} entries)");
            return new ParsedStringTable(parsedStrings);
        }

        private static ParsedInstruction ParseInstruction(string instructionLine, int lineNumber)
        {
            var parts = instructionLine.Split(new[]
                {
                    ' '
                }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToArray();

            if (!InstructionTable.TryLookup(parts[0].ToLowerInvariant(), out var info))
            {
                throw new ParsingException($"There is no instruction with the mnemonic {parts[0]}.", lineNumber);
            }

            var size = OperandSize.Default;
            if (info.NeedsSize)
            {
                if (parts.Length == 1)
                {
                    throw new ParsingException($"The {info.Mnemonic} instruction requires a size operand.", lineNumber);
                }
                var presumablySizeOperand = parts[1].ToLowerInvariant();
                if (!IsValidSize(presumablySizeOperand))
                {
                    throw new ParsingException($"The size {presumablySizeOperand} is invalid.", lineNumber);
                }
                size = (OperandSize)Enum.Parse(typeof(OperandSize), presumablySizeOperand, ignoreCase: true);
            }

            // movln doesn't take a size operand but has an implied size of QWORD
            // as both the source and destination operand (if well-formed) are pointers
            if (info.Mnemonic == "movln") { size = OperandSize.QWord; }

            int requiredPartCount = 1 + ((info.NeedsSize) ? 1 : 0) + info.OperandCount;
            if (parts.Length != requiredPartCount)
            {
                throw new ParsingException($"The {parts[0]} instruction requires {info.OperandCount} operand(s). It has {parts.Length - 1 - ((info.NeedsSize) ? 1 : 0)}.",
                    lineNumber);
            }

            string operand1 = null;
            string operand2 = null;
            string operand3 = null;

            if (info.OperandCount == 3) { operand3 = parts[(info.NeedsSize) ? 4 : 3]; }
            if (info.OperandCount >= 2) { operand2 = parts[(info.NeedsSize) ? 3 : 2]; }
            if (info.OperandCount >= 1) { operand1 = parts[(info.NeedsSize) ? 2 : 1]; }

            return new ParsedInstruction(parts[0], size, operand1, operand2, operand3);
        }

        private static string ParseString(string stringTableEntry, out int tableIndex, int lineNumber)
        {
            // To be a valid string table entry, it must consist of a number, colon, zero or more
            // whitespaces, and a sequence of text starting and ending with a double-quote.
            const string exceptionMessage = "A string table entry must take the form of \"0: \"some text here\"\".";

            // First, a colon must come before all double-quotes
            int firstColonIndex = stringTableEntry.IndexOf(':');
            int firstDoubleQuoteIndex = stringTableEntry.IndexOf('\"');

            if (firstDoubleQuoteIndex < firstColonIndex)
            {
                throw new ParsingException(exceptionMessage, lineNumber);
            }

            // Then, take the substring of everything before the colon (the index) and everything
            // after (the string)
            string tableIndexString = stringTableEntry.Substring(0, firstColonIndex);
            string tableString = stringTableEntry.Substring(firstColonIndex + 1).Trim();

            // Next, we can check that the table index is actually a number by trying to parse it.
            if (!int.TryParse(tableIndexString, out tableIndex))
            {
                throw new ParsingException(exceptionMessage, lineNumber);
            }

            // We can then deal with the string in quotes. Let's check that it ends in a double-quote
            // first...
            if (!tableString.EndsWith("\"", StringComparison.Ordinal))
            {
                throw new ParsingException(exceptionMessage, lineNumber);
            }

            // Then we can remove the quotes and tackle the string within.
            tableString = tableString.Substring(1);
            tableString = tableString.Substring(0, tableString.Length - 1);

            return ParseTableString(tableString, lineNumber);
        }

        private static bool IsLabelLine(string line)
        {
            // Labels start with any character in [_a-zA-Z]
            // and continue with zero or more character in [_a-zA-Z0-9]
            // and end with a :
            // The entire line must match
            const string labelRegex = @"^[_a-zA-Z][_a-zA-Z0-9]*:$";
            var match = Regex.Match(line, labelRegex);

            return match.Success || line.ToLowerInvariant().StartsWith("globals", StringComparison.Ordinal);
        }

        private static bool IsValidSize(string operand) => 
            operand == "byte" || operand == "word" || operand == "dword" || operand == "qword";

        private static string ParseTableString(string tableString, int lineNumber)
        {
            var resultBuilder = new StringBuilder(tableString.Length);

            for (int i = 0; i < tableString.Length; i++)
            {
                char current = tableString[i];

                switch (current)
                {
                    // Scenario 1: this character shouldn't appear without being escaped (' or ")
                    case '\'':
                    case '\"':
                        throw new ParsingException($"An entry in the string table has an illegal {current} character at index {i}.",
                            lineNumber);
                    case '\\' when i == tableString.Length - 1:
                        throw new ParsingException($"An entry in the string table ends in a slash, but no escape sequence was present.",
                            lineNumber);
                    case '\\':
                    {
                        char next = tableString[i + 1];
                        switch (next)
                        {
                            case '\'':
                                resultBuilder.Append('\'');
                                i++;
                                break;
                            case '\"':
                                resultBuilder.Append('\"');
                                i++;
                                break;
                            case '0':
                                resultBuilder.Append('\0');
                                i++;
                                break;
                            case 'a':
                                resultBuilder.Append('\a');
                                i++;
                                break;
                            case 'b':
                                resultBuilder.Append('\b');
                                i++;
                                break;
                            case 'f':
                                resultBuilder.Append('\f');
                                i++;
                                break;
                            case 'n':
                                resultBuilder.Append('\n');
                                i++;
                                break;
                            case 'r':
                                resultBuilder.Append('\r');
                                i++;
                                break;
                            case 't':
                                resultBuilder.Append('\t');
                                i++;
                                break;
                            case 'v':
                                resultBuilder.Append('\v');
                                i++;
                                break;
                            case 'u':
                            case 'U':
                                int codePointLength = (next == 'u') ? 4 : 8;
                                if (i + 1 + codePointLength > tableString.Length - 1)
                                {
                                    throw new ParsingException(
                                        "An entry in the string table ends in a Unicode escape sequence, but there aren't enough hexadecimal digits to determine the codepoint.",
                                        lineNumber);
                                }
                                string codePointString = tableString.Substring(i + 2, codePointLength);
                                if (!int.TryParse(codePointString, NumberStyles.HexNumber,
                                    CultureInfo.CurrentCulture, out int codePoint))
                                {
                                    throw new ParsingException($"The sequence {codePointString} is not a valid Unicode code point.", lineNumber);
                                }

                                resultBuilder.Append(char.ConvertFromUtf32(codePoint));
                                i += 1 + codePointLength;
                                break;
                            default:
                                throw new ParsingException($"Unrecognized escape sequence \\{next} at index {i}", lineNumber);
                        }

                        break;
                    }
                    default:
                        resultBuilder.Append(current);

                        break;
                }
            }

            return resultBuilder.ToString();
        }
    }
}
