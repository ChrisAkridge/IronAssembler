﻿using System.Collections.Generic;
using System.Linq;

namespace IronAssembler.Data
{
    internal sealed class ParsedFile
    {
        private readonly List<ParsedBlock> blocks;

        public IReadOnlyList<ParsedBlock> Blocks => blocks.AsReadOnly();
        public ParsedStringTable StringTable { get; }
        public int SizeOfGlobalVariableBlock { get; }

        public ParsedFile(IEnumerable<ParsedBlock> blocks, ParsedStringTable stringTable,
            int sizeOfGlobalVariableBlock)
        {
            this.blocks = blocks.ToList();
            StringTable = stringTable;
            SizeOfGlobalVariableBlock = sizeOfGlobalVariableBlock;
        }
    }
}
