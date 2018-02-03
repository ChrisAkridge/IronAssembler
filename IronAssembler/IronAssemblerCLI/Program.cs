using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using IronAssembler;

namespace IronAssemblerCLI
{
	class Program
	{
		static void Main(string[] args)
		{
			var result = Parser.Default.ParseArguments<Options>(args);

			if (result is Parsed<Options> options)
			{
				if ((!options.Value.MinimalVerbosity) &&
					(!options.Value.StagesVerbosity) &&
					(!options.Value.HighVerbosity) &&
					(!options.Value.VerboseVerbosity))
				{
					options.Value.MinimalVerbosity = true;
				}

				var inputFile = ReadInputFile(options.Value.InputFilePath);

				// If producing an IEXE, do the below line
				var outputFile = Assembly.AssembleProgram(inputFile /*, isDirectAssemblyFile: {true|false}*/);

				// If options says make an IronArc Direct Assembly,
				//	Switch extension on output file to IASM regardless of what it is
				//	Do the translation stage only
				//	Write to file below

				File.WriteAllBytes(options.Value.OutputFilePath, outputFile);
				// Create code that is included only in debug builds that prints any exceptions
				// and gracefully exits
			}
			else
			{
				Console.WriteLine("The arguments you provided could not be parsed.");
			}
		}

		static string ReadInputFile(string inputFilePath)
		{
			if (!File.Exists(inputFilePath))
			{
				throw new FileNotFoundException($"The file at {inputFilePath} does not exist.", inputFilePath);
			}

			return File.ReadAllText(inputFilePath);
		}
	}
}
