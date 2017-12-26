using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler.Data
{
	internal static class InstructionTable
	{
		private static Dictionary<string, InstructionInfo> instructions;

		public static InstructionInfo Lookup(string mnemonic)
		{
			return instructions[mnemonic];
		}

		static InstructionTable()
		{
			instructions = new Dictionary<string, InstructionInfo>
			{
				// Control Flow Instructions (0x00)
				{ "nop", new InstructionInfo("No Operation", "nop", 0x0000, false, 0, false) },
				{ "end", new InstructionInfo("End", "end", 0x0001, false, 0, false) },
				{ "jmp", new InstructionInfo("Jump", "jmp", 0x0002, false, 1, true) },
				{ "call", new InstructionInfo("Call", "call", 0x0003, false, 1, true) },
				{ "ret", new InstructionInfo("Return", "ret", 0x0004, false, 0, false) },
				{ "je", new InstructionInfo("Jump if Equal", "je", 0x0005, false, 1, true) },
				{ "jne", new InstructionInfo("Jump if Not Equal", "je", 0x0006, false, 1, true) },
				{ "jlt", new InstructionInfo("Jump if Less Than", "je", 0x0007, false, 1, true) },
				{ "jgt", new InstructionInfo("Jump if Greater Than", "je", 0x0008, false, 1, true) },
				{ "jlte", new InstructionInfo("Jump if Less Than or Equal To", "je", 0x0009, false, 1, true) },
				{ "jgte", new InstructionInfo("Jump if Greater Than or Equal To", "je", 0x000A, false, 1, true) },
				{ "jmpa", new InstructionInfo("Absolute Jump", "jmpa", 0x000B, false, 1, true) },
				{ "hwcall", new InstructionInfo("Hardware Call", "hwcall", 0x000C, false, 1, false) },
				{ "stackargs", new InstructionInfo("Stack Argument Prologue", "stackargs", 0x000D, false, 0, false) },

				// Data Operations (0x01)
				{ "mov", new InstructionInfo("Move Data", "mov", 0x0100, true, 2, false) },
				{ "movln", new InstructionInfo("Move Data with Length", "movln", 0x0101, false, 3, false) },
				{ "push", new InstructionInfo("Push to Stack", "push", 0x0102, true, 1, false) },
				{ "pop", new InstructionInfo("Pop from Stack", "pop", 0x0103, true, 1, false) },
				{ "arrayread", new InstructionInfo("Read Array Value", "arrayread", 0x0104, true, 1, false) },
				{ "arraywrite", new InstructionInfo("Write Array Value", "arraywrite", 0x0105, true, 2, false) },

				// Integral/Bitwise Operations (0x020*)
				{ "add", new InstructionInfo("Stack Addition", "add", 0x0200, true, 0, false) },
				{ "sub", new InstructionInfo("Stack Subtraction", "sub", 0x0201, true, 0, false) },
				{ "mult", new InstructionInfo("Stack Multiplication", "mult", 0x0202, true, 0, false) },
				{ "div", new InstructionInfo("Stack Division", "div", 0x0203, true, 0, false) },
				{ "mod", new InstructionInfo("Stack Modulus Division", "mod", 0x0204, true, 0, false) },
				{ "inc", new InstructionInfo("Stack Increment", "inc", 0x0205, true, 0, false) },
				{ "dec", new InstructionInfo("Stack Decrement", "dec", 0x0206, true, 0, false) },
				{ "bwand", new InstructionInfo("Stack Bitwise AND", "bwand", 0x0207, true, 0, false) },
				{ "bwor", new InstructionInfo("Stack Bitwise OR", "bwor", 0x0208, true, 0, false) },
				{ "bwxor", new InstructionInfo("Stack Bitwise XOR", "bwxor", 0x0209, true, 0, false) },
				{ "bwnot", new InstructionInfo("Stack Bitwise NOT", "bwnot", 0x020A, true, 0, false) },
				{ "lshift", new InstructionInfo("Stack Bitwise Shift Left", "lshift", 0x020B, true, 0, false) },
				{ "rshift", new InstructionInfo("Stack Bitwise Shift Right", "rshift", 0x020C, true, 0, false) },
				{ "land", new InstructionInfo("Stack Logical AND", "land", 0x020D, true, 0, false) },
				{ "lor", new InstructionInfo("Stack Logical OR", "lor", 0x020E, true, 0, false) },
				{ "lxor", new InstructionInfo("Stack Logical XOR", "lxor", 0x020F, true, 0, false) },
				{ "lnot", new InstructionInfo("Stack Logical NOT", "lnot", 0x0210, true, 0, false) },
				{ "cmp", new InstructionInfo("Stack Comparison", "cmp", 0x0211, true, 0, false) },
				{ "addl", new InstructionInfo("Long Addition", "addl", 0x0212, true, 3, false) },
				{ "subl", new InstructionInfo("Long Subtraction", "subl", 0x0213, true, 3, false) },
				{ "multl", new InstructionInfo("Long Multiplication", "multl", 0x0214, true, 3, false) },
				{ "divl", new InstructionInfo("Long Division", "divl", 0x0215, true, 3, false) },
				{ "modl", new InstructionInfo("Long Modulus Division", "modl", 0x0216, true, 3, false) },
				{ "incl", new InstructionInfo("Long Increment", "incl", 0x0217, true, 3, false) },
				{ "decl", new InstructionInfo("Long Decrement", "decl", 0x0218, true, 3, false) },
				{ "bwandl", new InstructionInfo("Long Bitwise AND", "bwandl", 0x0219, true, 3, false) },
				{ "bworl", new InstructionInfo("Long Bitwise OR", "bworl", 0x021A, true, 3, false) },
				{ "bwxorl", new InstructionInfo("Long Bitwise XOR", "bwxorl", 0x021B, true, 3, false) },
				{ "bwnotl", new InstructionInfo("Long Bitwise NOT", "bwnotl", 0x021C, true, 3, false) },
				{ "lshiftl", new InstructionInfo("Long Bitwise Shift Left", "lshiftl", 0x021D, true, 3, false) },
				{ "rshiftl", new InstructionInfo("Long Bitwise Shift Right", "rshiftl", 0x021E, true, 3, false) },
				{ "landl", new InstructionInfo("Long Logical AND", "landl", 0x021F, true, 3, false) },
				{ "lorl", new InstructionInfo("Long Logical OR", "lorl", 0x0220, true, 3, false) },
				{ "lxorl", new InstructionInfo("Long Logical XOR", "lxorl", 0x0221, true, 3, false) },
				{ "lnotl", new InstructionInfo("Long Logical NOT", "lnotl", 0x0222, true, 3, false) },
				{ "cmpl", new InstructionInfo("Long Comparison", "cmpl", 0x0223, true, 3, false) },

				// Floating Point Stack Operations (0x028*)
				{ "fadd", new InstructionInfo("Floating Stack Addition", "fadd", 0x0280, true, 0, false) },
				{ "fsub", new InstructionInfo("Floating Stack Subtraction", "fsub", 0x0281, true, 0, false) },
				{ "fmult", new InstructionInfo("Floating Stack Multiplication", "fmult", 0x0282, true, 0, false) },
				{ "fdiv", new InstructionInfo("Floating Stack Division", "fdiv", 0x0283, true, 0, false) },
				{ "fmod", new InstructionInfo("Floating Stack Modulus Division", "fmod", 0x0284, true, 0, false) },
				{ "fcmp", new InstructionInfo("Floating Stack Comparison", "fcmp", 0x0285, true, 0, false) },
				{ "fsqrt", new InstructionInfo("Floating Stack Square Root", "fsqrt", 0x0286, true, 0, false) }
			};
		}
	}
}
