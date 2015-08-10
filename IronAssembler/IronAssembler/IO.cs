using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler
{
	internal static class IO
	{
		public static List<string> LoadInputFile(string inputFilePath)
		{
			try
			{
				string[] file = File.ReadAllLines(inputFilePath);
				return file.Select(f => f.Trim()).ToList();
			}
			catch (Exception ex)	// Rationale: we're just logging the error here so catching Exception isn't bad practice I swear
			{
				GlobalLogger.WriteLine(string.Format("Error: {0}", ex.Message), VerbosityLevel.Minimal);
				throw;
			}
		}

		public static string GetOutputFilePath(string inputFileName)
		{
			try
			{
				return Path.ChangeExtension(inputFileName, "iexe");
			}
			catch (ArgumentException ex) // ofc it's easier if a method only throws one exception type
			{
				GlobalLogger.WriteLine(string.Format("Error: {0}", ex.Message), VerbosityLevel.Minimal);
				throw;
			}
		}

		public static void WriteOutputFile(string outputFileName, byte[] outputBytes)
		{
			try
			{
				File.WriteAllBytes(outputFileName, outputBytes);
			}
			catch (Exception ex)
			{
				GlobalLogger.WriteLine(string.Format("Error: {0}", ex.Message), VerbosityLevel.Minimal);
				throw;
			}
		}
	}
}
