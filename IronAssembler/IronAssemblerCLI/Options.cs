﻿using CommandLine;

namespace IronAssemblerCLI
{
    internal class Options
    {
        [Option('i', "input", HelpText = "\"path\\to\\input\": The path to an input file containing IronArc assembly.", Required = true)]
        public string InputFilePath { get; set; }

        [Option('o', "output", HelpText = "\"path\\to\\output\": The path to an output file to write IronArc executable code to.", Required = true)]
        public string OutputFilePath { get; set; }

        [Option('t', "translateskip", HelpText = "--translateskip or -t: Skips the translation phase. The input file MUST be an IronArc direct assembly file.", Required = false)]
        public bool SkipTranslationPhase { get; set; }

        [Option('d', "producedirect", HelpText = "-d or --producedirect: Given an IronArc assembly file, produces an IronArc Direct Assembly file. The output file always has extension iasm regardless of the given output path.", Required = false)]
        public bool ProduceDirectAssembly { get; set; }

        [Option('s', "disassemble", HelpText = "-s or --disassemble: Given an IronArc executable file, disassembles into an IronArc Direct Assembly file.")]
        public bool DisassembleFile { get; set; }
    }
}
