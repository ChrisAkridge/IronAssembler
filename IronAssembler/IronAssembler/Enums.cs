using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler
{
	public enum VerbosityLevel
	{
		Minimal,
		Stages,
		High,
		Verbose
	}

	public enum OperandSize
	{
		Default,
		Byte,
		Word,
		DWord,
		QWord
	}
}
