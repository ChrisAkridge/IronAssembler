using System;

namespace IronAssembler
{
    [Serializable]
    public class TranslationException : Exception
    {
        public TranslationException() { }
        public TranslationException(string message) : base(message) { }
        public TranslationException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class ParsingException : Exception
    {
        public int LineNumber { get; }
        
        public ParsingException() { }
        public ParsingException(string message, int lineNumber) : base($"{message} (line {lineNumber})") { }
        public ParsingException(string message, int lineNumber, Exception inner) : base(
            $"{message} (line {lineNumber})", inner) { }
    }

    [Serializable]
    public class AssemblerException : Exception
    {
        public AssemblerException() { }
        public AssemblerException(string message) : base(message) { }
        public AssemblerException(string message, Exception inner) : base(message, inner) { }
    }
}
