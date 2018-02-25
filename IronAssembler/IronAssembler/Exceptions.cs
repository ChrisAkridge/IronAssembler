using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronAssembler
{
	[Serializable]
	public class TranslationException : Exception
	{
		public TranslationException() { }
		public TranslationException(string message) : base(message) { }
		public TranslationException(string message, Exception inner) : base(message, inner) { }
		protected TranslationException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class ParsingException : Exception
	{
		public ParsingException() { }
		public ParsingException(string message) : base(message) { }
		public ParsingException(string message, Exception inner) : base(message, inner) { }
		protected ParsingException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}


	[Serializable]
	public class AssemblerException : Exception
	{
		public AssemblerException() { }
		public AssemblerException(string message) : base(message) { }
		public AssemblerException(string message, Exception inner) : base(message, inner) { }
		protected AssemblerException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
