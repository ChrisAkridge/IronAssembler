using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler.Data
{
    internal sealed class ParsedInstruction
    {
        public string Mnemonic { get; }
        public OperandSize Size { get; }
        public string Operand1Text { get; }
        public string Operand2Text { get; }
        public string Operand3Text { get; }

        public ParsedInstruction(string mnemonic, OperandSize size = OperandSize.Default,
            string operand1Text = null, string operand2Text = null, string operand3Text = null)
        {
            Mnemonic = mnemonic.ToLowerInvariant();
            Size = size;
            Operand1Text = operand1Text;
            Operand2Text = operand2Text;
            Operand3Text = operand3Text;
        }
    }
}
