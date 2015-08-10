using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler
{
	public interface ILogger
	{
		void Write(string message, VerbosityLevel verbosity);
		void WriteLine(string message, VerbosityLevel verbosity);
	}

	public enum VerbosityLevel
	{
		// Outputs only assembly started, assembly completed, and error messages.
		Minimal,

		// Outputs minimal messages and when each stage has started and finished
		Stages,

		// Outputs stages messages as well as information within stages and benchmarks
		High,

		// Outputs everything. Everything.
		Verbose
	}
}
