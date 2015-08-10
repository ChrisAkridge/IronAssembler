using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler.Components
{
	internal sealed class Instruction
	{
		public static readonly string[] InstructionMnemonics = new string[]
		{
			"nop", "jmp", "jmpa", "je", "jne", "jlt", "jgt", "jlte", "jgte",
			"jz", "jnz", "call", "calla", "ret", "end", "mov", "add", "sub",
			"mult", "div", "mod", "inv", "eq", "ineq", "lt", "gt", "lteq",
			"gteq", "and", "or", "not", "bwnot", "bwand", "bwor", "bwxor",
			"bwlshift", "bwrshift", "addl", "subl", "multl", "divl", "modl",
			"invl", "eql", "ineql", "ltl", "gtl", "lteql", "gteql", "andl", 
			"orl", "notl", "bwnotl", "bwandl", "bworl", "bwxorl", "bwlshiftl",
			"bwrshiftl", "push", "pop", "peek", "stackalloc", "arrayalloc", 
			"deref", "arrayaccess", "cbyte", "csbyte", "cshort", "cushort", 
			"cint", "cuint", "clong", "culong", "csing", "cdouble", "hwcall",
			"setflag", "clearflag", "testflag", "toggleflag"
		};

		public int OverallInstructionNumber { get; private set; }
		public int LocalInstructionNumber { get; private set; }
		public string Text { get; private set; }

		public Instruction(int overallInstructionNumber, int localInstructionNumber, string text)
		{
			OverallInstructionNumber = overallInstructionNumber;
			LocalInstructionNumber = localInstructionNumber;
			Text = text;
		}

		public static bool IsInstructionLine(string line)
		{
			int firstSpaceIndex = line.IndexOf(' ');
			if (firstSpaceIndex == -1)
			{
				// There are no spaces, so we can compare the whole line to an opcode
				return InstructionMnemonics.Contains(line.ToLowerInvariant());
			}
			else
			{
				string firstWord = line.Substring(0, firstSpaceIndex);
				return InstructionMnemonics.Contains(line.ToLowerInvariant());
			}
		}
	}
}
