namespace IronAssembler
{
    public enum VerbosityLevel
    {
        Minimal,
        Stages,
        High,
        Verbose
    }

    public enum OperandSize
    {
        Byte,
        Word,
        DWord,
        QWord,
        Default
    }

    public enum OperandType
    {
        MemoryAddress,
        MemoryPointerAddress,
        Register,
        RegisterWithPointer,
        RegisterWithPointerAndOffset,
        NumericLiteral,
        StringTableEntry,
        Label
    }

    public enum Register
    {
        EAX,
        EBX,
        ECX,
        EDX,
        EEX,
        EFX,
        EGX,
        EHX,
        EBP,
        ESP,
        EIP,
        EFLAGS,
        ERP
    }
}
