using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler.Components
{
	internal sealed class Label
	{
		private List<Instruction> instructions;

		public string Name { get; private set; }
		
		public IReadOnlyList<Instruction> Instructions
		{
			get
			{
				return instructions.AsReadOnly();
			}
		}

		public Label(string name, IEnumerable<Instruction> cInstructions)
		{
			Name = name;
			cInstructions = cInstructions.ToList();
		}

		public static bool IsLabelDefinitionLine(string line)
		{
			return line.EndsWith(":");
		}
	}
}
