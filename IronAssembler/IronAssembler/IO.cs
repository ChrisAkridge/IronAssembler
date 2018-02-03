using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace IronAssembler
{
	internal static class IO
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		internal static List<string> SplitInputByLine(string input)
		{
			logger.Trace($"Received input of {input.Length} characters");

			if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
			{
				throw new ArgumentException($"The provided assembly file had no content.");
			}

			// https://stackoverflow.com/a/25196003/2709212
			return input.GetLines(removeEmptyLines: true).ToList();
		}
	}
}
