﻿using System.Collections.Generic;
using System.Linq;

namespace IronAssembler.Data
{
    internal sealed class ParsedBlock
    {
        private readonly List<ParsedInstruction> instructions;	

        public string Name { get; }
        public IReadOnlyList<ParsedInstruction> Instructions => instructions.AsReadOnly();

        public ParsedBlock(string name, IEnumerable<ParsedInstruction> instructions)
        {
            Name = name;
            this.instructions = instructions.ToList();
        }
    }
}
