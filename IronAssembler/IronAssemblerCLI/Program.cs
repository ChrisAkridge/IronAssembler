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
				if (!options.Value.DisassembleFile)
				{
					var inputFile = ReadInputFile(options.Value.InputFilePath);

					if (!options.Value.ProduceDirectAssembly)
					{
						byte[] outputFile = Assembly.AssembleProgram(inputFile, isDirectAssemblyFile: options.Value.SkipTranslationPhase);
						File.WriteAllBytes(options.Value.OutputFilePath, outputFile);
					}
					else
					{
						string outputPath = options.Value.OutputFilePath;
						int extensionDotIndex = outputPath.LastIndexOf('.');
						var outputPathWithoutExtension = outputPath.Substring(0, extensionDotIndex);
						options.Value.OutputFilePath = outputPathWithoutExtension + ".iasm";

						IList<string> directAssembly = Translator.TranslateFile(inputFile);
						File.WriteAllLines(options.Value.OutputFilePath, directAssembly);
					}

					// Create code that is included only in debug builds that prints any exceptions
					// and gracefully exits
				}
				else
				{
					var inputFile = File.ReadAllBytes(options.Value.InputFilePath);
					Console.WriteLine($"Loaded file, length {inputFile.Length} byte(s).");

					string disassembled = Disassembler.DisassembleProgram(inputFile, true, true);
					Console.WriteLine($"Disassembly complete.");

					File.WriteAllText(options.Value.OutputFilePath, disassembled);
				}
			}
			else
			{
				Console.WriteLine("The arguments you provided could not be parsed.");
			}

			Console.ReadKey();
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
