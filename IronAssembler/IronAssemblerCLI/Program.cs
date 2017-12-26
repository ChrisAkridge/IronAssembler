using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

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

				Console.WriteLine("You want to parse {0}", options.Value.InputFilePath);
				Console.WriteLine("And output a file at {0}", options.Value.OutputFilePath);
			}

			Console.ReadKey();
		}
	}
}
