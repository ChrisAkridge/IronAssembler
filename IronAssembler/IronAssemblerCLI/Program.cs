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
