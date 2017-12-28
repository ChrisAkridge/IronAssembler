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

		public static byte[] AssembleProgram(string program)
		{
			logger.Info("Start I/O Stage");
			var programLines = IO.SplitInputByLine(program);

			logger.Info("Start Parsing Stage");
			int stringsLabelIndex;
			var labelLocations = Parser.ScanProgramForLabels(programLines, out stringsLabelIndex);
			var parsedBlocks = Parser.ParseBlocks(programLines, labelLocations);
			var parsedStringTable = Parser.ParseStringsTable(programLines, stringsLabelIndex);
			var parsedFile = new ParsedFile(parsedBlocks, parsedStringTable);
			logger.Info("End Parsing Stage");

			logger.Info("Start Assembling Stage");
			var blocks = new List<AssembledBlock>();
			foreach (var block in parsedBlocks)
			{
				blocks.Add(Assembler.AssembleBlock(block));
			}

			return null;
		}
	}
}
