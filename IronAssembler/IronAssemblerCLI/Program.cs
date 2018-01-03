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

				//try
				//{
					var inputFile = ReadInputFile(options.Value.InputFilePath);
					var outputFile = Assembly.AssembleProgram(inputFile);
				File.WriteAllBytes(options.Value.OutputFilePath, outputFile);
				//}
				//catch (Exception ex)
				//{
				//	Console.WriteLine();
				//	Console.WriteLine("IronAssembler has encountered an error and cannot complete the assembly.");
				//	Console.WriteLine($"The type of the error is {ex.GetType().Name}.");
				//	Console.WriteLine($"The message of the error is:");
				//	Console.WriteLine(ex.Message);
				//	Console.WriteLine();
				//	Console.WriteLine("Press S to print a stack trace.");
				//	if (Console.ReadKey(intercept: true).Key == ConsoleKey.S)
				//	{
				//		Console.Write(ex.StackTrace);
				//	}
				//	else { return; }
				//}
			}

			Console.ReadKey(intercept: true);
		}

		// WYLO: assemble the string table
		// Also make every method except AssembleParsedFile in Assembler as a private method
		// Maybe do the same with Parser and just add a ParseFile(IEnumerable<string>)

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
