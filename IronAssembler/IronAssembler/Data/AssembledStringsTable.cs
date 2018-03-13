using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler.Data
{
	/// <summary>
	/// Represents an assembled strings table.
	/// </summary>
	internal sealed class AssembledStringsTable
	{
		private List<byte> bytes;

		/// <summary>
		/// Gets a read-only list of the bytes of this string table.
		/// </summary>
		public IReadOnlyList<byte> Bytes => bytes.AsReadOnly();

		/// <summary>
		/// Initializes a new instance of the <see cref="AssembledStringsTable"/> class.
		/// </summary>
		/// <param name="bytes">A sequence of bytes that makes up this strings table.</param>
		public AssembledStringsTable(IEnumerable<byte> bytes)
		{
			this.bytes = bytes.ToList();
		}
	}
}
