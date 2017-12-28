using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler.Data
{
	internal sealed class InstructionInfo
	{
		public string ProperName { get; }
		public string Mnemonic { get; }
		public ushort Opcode { get; }
		public bool NeedsFlags { get; }
		public bool NeedsSize { get; }
		public int OperandCount { get; }
		public bool CanUseLabels { get; }

		public InstructionInfo(string properName, string mnemonic, ushort opcode, bool needsFlags,
			bool needsSize, int operandCount, bool canUseLabels)
		{
			ProperName = properName;
			Mnemonic = mnemonic;
			Opcode = opcode;
			NeedsFlags = needsFlags;
			NeedsSize = needsSize;
			OperandCount = operandCount;
			CanUseLabels = canUseLabels;
		}
	}
}
