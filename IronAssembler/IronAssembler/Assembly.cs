using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronAssembler.Data;
using NLog;

namespace IronAssembler
{
	public static class Assembly
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		static Assembly()
		{
			LogConfigurer.ConfigureLog();
		}

		public static byte[] AssembleProgram(string program, bool isDirectAssemblyFile)
		{
			logger.Info("Start I/O Stage");

			IList<string> programLines =
				!isDirectAssemblyFile ? Translator.TranslateFile(program) : IO.SplitInputByLine(program);

			logger.Info("End I/O Stage");

			logger.Info("Start Parsing Stage");
			var parsedFile = Parser.ParseFile(programLines);
			logger.Info("End Parsing Stage");

			logger.Info("Start Assembling Stage");
			var assembledFile = Assembler.AssembleFile(parsedFile);
			logger.Info("End Assembling Stage");

			logger.Info("Start Linking Stage");
			var linkedFile = Linker.LinkFile(assembledFile, parsedFile.StringTable);
			logger.Info("End Linking Stage");

			return linkedFile;
		}
	}
}
