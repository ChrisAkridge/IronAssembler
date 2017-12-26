using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler.Data
{
	internal sealed class AssembledInstruction
	{
		private List<byte> bytes;

		public IReadOnlyList<byte> Bytes => bytes.AsReadOnly();
		public string Operand1Label { get; }
		public string Operand2Label { get; }
		public string Operand3Label { get; }

		public AssembledInstruction(IEnumerable<byte> bytes, string operand1Label = null,
			string operand2Label = null, string operand3Label = null)
		{
			this.bytes = bytes.ToList();
			Operand1Label = operand1Label;
			Operand2Label = operand2Label;
			Operand3Label = operand3Label;
		}
	}
}
