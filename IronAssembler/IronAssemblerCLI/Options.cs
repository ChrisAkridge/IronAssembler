using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace IronAssemblerCLI
{
	internal class Options
	{
		[Option("input", HelpText = "\"path\\to\\input\": The path to an input file containing IronArc assembly.", Required = true)]
		public string InputFilePath { get; set; }

		[Option("output", HelpText = "\"path\\to\\output\": The path to an output file to write IronArc executable code to.", Required = true)]
		public string OutputFilePath { get; set; }

		[Option('m', "minimal", SetName = "verbosity", Required = false, HelpText = "Minimal verbosity. Prints assembly started/completed and errors.")]
		public bool MinimalVerbosity { get; set; }

		[Option('s', "stages", SetName = "verbosity", Required = false, HelpText = "Stages verbosity. Prints Minimal messages and the start and end of each stage.")]
		public bool StagesVerbosity { get; set; }

		[Option('h', "high", SetName = "verbosity", Required = false, HelpText = "High verbosity. Prints Stages messages and benchmarks and information within stages.")]
		public bool HighVerbosity { get; set; }

		[Option('v', "verbose", SetName = "verbosity", Required = false, HelpText = "Verbose. Prints all messages.")]
		public bool VerboseVerbosity { get; set; }

		// Add option 't' - given an IronArc Direct Assembly file, skips the translation phase
		// Add option 'd' - produces an IronArc Direct Assembly file instead of an IronArc executable (extension is always iexe)
	}
}
