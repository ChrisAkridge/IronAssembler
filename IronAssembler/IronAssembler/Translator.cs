using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler
{
	public static class Translator
	{
		public static IList<string> TranslateFile(IList<string> inputFileLines)
		{
			//  Remove comments
			//	For each line,
			//	|	Split the line by whitespace into words
			//	|	If there are more than two words,
			//	|	|	For each word (operand) of index 2 or higher,
			//	|	|	|	Check if the operand needs to be translated
			//	|	|	|	Translate the operand if it does
			//	|	|	|	Add the word to a StringBuilder for the line
			//	|	Add the line to a list of lines
			//	Perform the floating point translation
			//	Perform the string table translation

			return inputFileLines;
		}

		private static IList<string> RemoveComments(IList<string> lines)
		{
			// For each line,
			//	If line starts with a "#", ignore it
			//	If line contains a "#", cut it and everything after, trim, and add to result list
			return null;
		}

		private static string TranslateOperand(string operand)
		{
			// For all below operands,
			// |	Check if the operand is of that type
			// |	|	True: Translate the operand and return it
			// |	|	False: Return the operand as-is
			return null;
		}
		
		private static bool IsRegisterWithHexadecimalOffset(string operand)
		{
			// Operand must start with * and be followed by a register name (case insensitive)
			// Then a + or -
			// Then "0x" (case insensitive)
			// Then 1 to 8 hex digits (case insensitive)
			return false;
		}

		private static string TranslateRegisterWithHexadecimalOffset(string operand)
		{
			// Find a + or - and split on it
			// Convert the second value (the offset) to decimal
			// Concatenate the register name and the decimal offset
			return null;
		}

		private static bool IsMemoryAddress(string operand)
		{
			// Operand must start with "mem:" (case insensitive)
			// Then "0x" (case insensitive)
			// Then 1 to 16 hex digits (case insensitive)
			return false;
		}

		private static string TranslateMemoryAddress(string operand)
		{
			// Lowercase the operand and split on 'x'
			// Take everything in the second value (the address) and left-pad it up to 16 characters with '0'
			// Concatenate "0x" and the left-pad value
			return null;
		}
		
		private static bool IsHexadecimalNumericLiteral(string operand)
		{
			// Operand must start with "0x" (case insensitive)
			// Then 1 to 16 hex digits (case insensitive)
			return false;
		}

		private static string TranslateHexadecimalNumericLiteral(string operand)
		{
			// Convert operand to decimal
			return null;
		}

		private static bool IsFloatingPointLiteral(string operand)
		{
			// Operand must start with "single(" or "double("  (case insensitive)
			// Then a floating point number
			// Then a ')'
			return false;
		}

		private static IList<string> TranslateFloatingPointLiterals(IList<string> lines)
		{
			// Since we need to add size operands here, we can't just do this as we can all the
			// above translations.

			// For each line,
			// |	Split the line into words by whitespace
			// |	Check if the second word is a size token and store it in a local if it is
			// |	For each word from index ((size token local not null) ? 2 : 1),
			// |	|	If the word is a floating point literal,
			// |	|	|	Split it on '(' and take the second value without the last character
			// |	|	|	Use float.Parse or double.Parse to get the literal's value
			// |	|	|	Use BitConverter or an extension method to get the uint/ulong of the value's bits
			// |	|	|	Set that word in the words array to the uint/ulong's ToString
			// |	|	|	If the size token local is not null,
			// |	|	|	|	Check that the size token is DWORD if the literal is float, or QWORD if it's double. Throw if not.
			// |	|	|	Else, if the size token present flag is not set,
			// |	|	|	|	Set the size token local to DWORD/QWORD for float/double, respectively
			// |	Create a new line from these words: [0], size token local, remainder of words
			// |	Add line to result
			return null;
		}

		private static IList<string> BuildStringTableFromLiterals(IList<string> lines)
		{
			// For each line,
			// |	Find any \" sequence and add the index of that " to an ignored indices list.
			// |	Find all other " characters at non-ignored indices and add them to a list.
			// |	Take each pair of quote indices to make a string and add it to a list if not duplicate.
			// |	Replace the literal in the line with a table index operand using an
			//		 auto-incremented string index (be sure to replace the string in the lines
			//		  argument). Add the line's index to a list of lines to check for size tokens.
			// |	Check if any of the lines with string operands need to add a size token, and
			//		 add one if necessary (be sure to replace the string in the lines argument).
			// |	Make a strings table from the list of strings.

			// Locals: ignored indices list, non-ignored double quote indices list, list of
			// non-duplicate string literals, index of last recorded string literal, list of lines
			// to check for missing size tokens, strings table.
			return null;
		}
	}
}
