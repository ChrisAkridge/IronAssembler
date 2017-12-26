using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler.Data
{
	internal sealed class ParsedBlock
	{
		private List<ParsedInstruction> instructions;	

		public string Name { get; }
		public IReadOnlyList<ParsedInstruction> Instructions => instructions.AsReadOnly();

		public ParsedBlock(string name, IEnumerable<ParsedInstruction> instructions)
		{
			Name = name;
			this.instructions = instructions.ToList();
		}
	}
}
