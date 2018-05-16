# IronAssembler Specification

The IronAssembler program assembles a text file containing IronArc assembly into a flat binary containing executable IronArc code.

[IronArc](https://github.com/Celarix/IronArc) is a virtual processor and computing platform designed for the study of how computers and processors work, and is not intended for serious usage. IronArc runs on executable code written according to a variable-width [instruction set](https://github.com/Celarix/IronArc/blob/master/Documentation/Simple%20Instruction%20Listing.txt).

## Input Format
IronAssembler assembles sequences of instructions bundled beneath labels. Labels denote the beginnings and ends of sequences of instructions and can be used as references within instructions. For instance, `jmp MyLabel` will be assembled into a sequence of bytes where the label is replaced with the address where `MyLabel` begins.

IronAssembler may assemble from one file containing assembly instructions. This file will adhere to the following grammar:

```
  assembly-file:
    global-variable-block then
    Zero or more blocks then
    string-table

  global-variable-block:
    globals: then
    newline then
    Zero or more global-variable-declarations

  global-variable-declaration:
    operand-size then
    One or more whitespaces then
    identifier then
    One or more whitespaces then
    One, two, four, or eight hexadecimal-digits

  block:
    label then
    colon (U+003A) then
    newline
    Zero or more assembly-instructions

  label:
    one of label-start-characters then
    zero or more of label-characters

  label-start-characters:
    One of {abcdefghijklmnopqrstuvwxyz_}

  label-characters:
    One of {abcdefghijklmnopqrstuvwxyz0123456789_}

  identifier:
    label

  newline:
    cr-lf-newline OR
    lf-newline

  cr-lf-newline:
    Carriage return (U+000D) then
    Linefeed (U+000A)

  lf-newline:
    Linefeed (U+000A)

  whitespace:
    Space (U+0020) OR
    Tab (U+0009)

  memory-address:
    memory-address-with-pointer OR
    memory-address-non-pointer

  memory-address-with-pointer:
    * then
    0x then
    8 hexadecimal-digit

  memory-address-non-pointer:
    0x then
    8 hexadecimal-digits

  hexadecimal-digit:
    One of {0123456789ABCDEF}, case insensitive

  assembly-instruction:
    Zero or more whitespaces
    zero-operand-instruction OR
    one-operand-instruction OR
    two-operand-instruction OR
    three-operand-instruction OR
    newline

  zero-operand-instruction:
    One of { nop end ret stackargs add sub mult div mod inc dec bwand bwor bwxor lshift rshift land lor lxor lnot cmp fadd sub fmult fidv fmod fcmp fsqrt }, case insensitive

  one-operand-instruction:
    One of { mov push pop arrayread arraywrite }, case insensitive then
    operand-size then
    whitespace then
    address-block

  two-operand-instruction:
    One of { }, case insensitive then
    operand-size then
    whitespace then
    2 address-blocks

  three-operand-instruction:
    One of { addl subl multl divl modl incl decl bwandl bworl bwxorl bwnotl lshiftl rshiftl landl lorl lxorl lnotl cmpl }, case insensitive then
    operand-size then
    whitespace then
    3 address-blocks

  movln-instruction:
    movln then
    3 address-blocks

  operand-size:
    One of { BYTE WORD DWORD QWORD }, case-sensitive

  address-block:
    memory-address OR
    processor-register OR
    processor-register-pointer OR
    processor-register-pointer-offset OR
    numeric-literal OR
    string-table-entry OR
    label-ref then
    whitespace

  processor-register:
    One of { eax ebx ecx edx eex efx egx ehx eflags eip esp ebp erp }, case insensitive

  processor-register-pointer:
    * then
    One of { eax ebx ecx edx eex efx egx ehx eflags eip esp ebp erp }, case insensitive

  processor-register-pointer-offset:
    * then
    One of { eax ebx ecx edx eex efx egx ehx eflags eip esp ebp erp }, case insensitive then
    + OR - then
    decimal number 0 to 2147483647 inclusive

  numeric-literal:
    Decimal number -9223372036854775808 to 9223372036854775807 inclusive

  string-table-entry:
    str: then
    Decimal number

  label-ref:
    One label-start-character then
    Zero or more label-characters

  string-table:
    strings: then
    Zero or more string-table-literal

  string-table-literal:
    Decimal number then
    : then
    string-literal

  string-literal:
    Quotation mark (U+0022) then
    Zero or more characters OR string-escape-sequences then
    Quotation mark (U+0022)

  string-escape-sequences:
    One of { \r \n \t \0 \" \\ }
```

All instructions, labels, and string table literals must be on one line. There can be arbitrary amounts of whitespace inside of an between lines.

## Stages of Assembling

### Stage 1: Loading Input
In the first stage, the entire input file is loaded into memory. It is then split on `\r, \n, or \r\n` into an array of lines. Each line is trimmed of whitespace on both sides.

### Stage 2: Parsing
Each line of the input file is here converted into the following structure:

```
  Dictionary<string, Label> Labels
    KeyValuePair<string, Label> LabelKVP
      Key: name of label
      Value:
      class Label
      {
        string Name;
        List<string> Instructions;
      }
```

### Stage 3: Assembling Labels
The instructions in each label are then assembled, converting the string value into an `AssembledInstruction`.

```
class AssembledInstruction
{
	byte[] AssembledBytes;
	List<string> LabelReferences;
}
```

Each `AssembledInstruction` maintains a list of zero to three references to other labels that are resolved in the Linking stage. The bytes that will store the addresses of the labels, if present, have been initialized to `0xCCCCCCCCCCCCCCCC`, `0xDDDDDDDDDDDDDDDD`, and `0xEEEEEEEEEEEEEEEE`, respectively.

AssembledInstructions are stored within AssembledLabels.

```
class AssembledLabel
{
	string Name;
	List<AssembledInstructions> Instructions;
	ulong Address { get; set; }
	ulong CodeSize { get; }
}
```

### Stage 4: Linking
The assembled labels must then be linked. First, a running sum of all the code sizes of each label is tallied in the order the labels were defined in the input file, and at each step of the sum, the `Address` property of the current assembled label has the sum assigned to it. Next, all assembled instructions are enumerated, in the order that the labels were defined in the input file and in the order they were defined in the label. Each assembled instruction will have the placeholder values for any label references replaced with the actual address of the label.

### Stage 5: Concatenation
Finally, all instructions are concatenated into one large byte array and written to disk.

## Code Implementation Map
Data types:
```
IronAsssembler.Components.Parsed:
  Instruction: Represents a parsed-but-not-assembled instruction. Stores the local (inside the label) and global (inside the program) instruction number.
  Label: Represents a named list of instructions.
  ParsedFile: Represents a dictionary of labels with their names and a string table.
IronAssembler.Components.Assembled:
  AssembledInstruction: Represents an assembled instruction as an array of bytes along with the names of any labels referenced in the instruction.
  AssembledLabel: Represents a named list of assembled instructions.
```

Stage processors:
```
  IronAssembler.IO: Reads and writes files.
  IronAssembler.Parser: Converts a list of strings into a ParsedFile.
  IronAssembler.Assembler: Assembles each instruction into a binary form.
  IronAssembler.Linker: Links every assembled label into a single group of bytes, substituting known label addresses for placeholders.
```