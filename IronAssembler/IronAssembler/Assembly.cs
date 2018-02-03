﻿using System;
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

		public static byte[] AssembleProgram(string program /*, bool isDirectAssemblyFile */)
		{
			// If the file is a direct assembly file, skip the translation stage

			logger.Info("Start I/O Stage");
			var programLines = IO.SplitInputByLine(program);
			logger.Info("End I/O Stage");

			// Log and perform the translation phase

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