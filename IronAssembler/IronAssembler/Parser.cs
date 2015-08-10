using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronAssembler.Components;

namespace IronAssembler
{
	internal static class Parser
	{
		public static List<Label> ParseInputFile(IEnumerable<string> fileLines)
		{
			List<Label> result = new List<Label>();
			Label current = null;
			int overallInstructionNumber = 0;
			int localInstructionNumber = 0;

			foreach (string line in fileLines)
			{
				if (Instruction.IsInstructionLine(line))
				{
					if (current == null)
					{
						GlobalLogger.WriteLine("While parsing the file into a list of labels, the current label did not exist. This is usually a sign that the file does not begin with a label.", VerbosityLevel.Minimal);
						throw new ArgumentException();
					}

					current.Instructions.Add(new Instruction(overallInstructionNumber, localInstructionNumber, line));
					overallInstructionNumber++;
					localInstructionNumber++;
				}
				else if (Label.IsLabelDefinitionLine(line))
				{
					if (current != null)
					{
						result.Add(current);
						
						string labelName = line.Substring(0, line.Length - 1);
						current = new Label(labelName);
						localInstructionNumber = 0;
					}
				}
			}

			return result;
		}
	}
}
