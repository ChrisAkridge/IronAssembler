using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler.Data
{
	internal sealed class AssembledStringsTable
	{
		private List<byte> bytes;

		public IReadOnlyList<byte> Bytes => bytes.AsReadOnly();

		public AssembledStringsTable(IEnumerable<byte> bytes)
		{
			this.bytes = bytes.ToList();
		}
	}
}
