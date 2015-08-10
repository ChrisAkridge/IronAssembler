using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler
{
	public static class GlobalLogger
	{
		public static ILogger Logger { get; private set; }

		public static void Write(string message, VerbosityLevel verbosity)
		{
			Logger.Write(message, verbosity);
		}

		public static void WriteLine(string message, VerbosityLevel verbosity)
		{
			Logger.WriteLine(message, verbosity);
		}
	}
}
