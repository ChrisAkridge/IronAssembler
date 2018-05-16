# IronAssembler Specification

### For Alpha v0.1

This document specifies the form of IronArc Assembly files and the process to convert them into IronArc Executable files.

## The Basics

[IronArc](https://www.github.com/ChrisAkridge/IronArc) is a virtual processor architecture that can run arbitrary machine code. This machine code, known as an IronArc Executable file, is a series of bytes that are separated into a header, a sequence of instructions, and a table containing strings used by the program.

The header states key aspects of the program, such as the specification and assembler version and the starting addresses of the instructions and the string table.

The instructions are a sequence of individual instructions. Each instruction consists of a two-byte opcode followed by an optional byte that states the size and types of the following zero to three operands. Each operand, also known as an "address block", specifies a processor register, memory address, numeric literal, or string literal.

The string table, which occurs after all instructions, starts with the number of strings in the table followed by all their addresses in the file. After the addresses come each string, which is stored as a length-prefixed UTF-8 string: a 32-bit integer that states the number of bytes in the string, followed by the content of the string encoded using UTF-8.

The IronArc Executable format is further described in the [official specification](https://github.com/ChrisAkridge/IronArc/blob/master/Documentation/Specification.md#ironarc-binary-iexe-format).

IronAssembler is a program that is able to produce IronArc Executable files. It does so by translating from another format called IronArc Assembly. IronArc Assembly is a text format that is able to express instructions.

Internally, IronAssembler performs a translation from IronArc Assembly to an intermediate format named IronArc Direct Assembly, which removes comments, normalizes certain aspects of the program, and generate a strings table from literals included in the assembly file.

## IronArc Assembly Format
IronArc Assembly files are text files, preferably encoded as ASCII or, even more preferably, a form of Unicode such as UTF-8 or UTF-16. These files must be processed line-by-line, with whitespace being trimmed off the ends of every line and blank lines discarded. As a result, **no two elements (labels, instructions, etc.) may occur on the same line**. Additionally, **one element may not span multiple lines.**

Comments are started with the `#` character. They may appear on a single line or at the ends of lines.

IronArc Assembly files consist of groups of blocks. Blocks are groups of lines of text that start with a label. A label is an identifier of text ending in a colon. Labels can contain letters, numbers, and underscores, but cannot start with a number. Additionally, the label `strings:` is a reserved label and cannot be used.

All lines below are examples of valid labels.

```
main:
loop_1_start:
_returnValueOrRaiseError:
_23_ab_2111:
_:
```

Below are examples of invalid labels.

```
10th_block:
123:
:
globals:
strings:
```

The first block in the file should be called `globals:`. This tells the assembler how many bytes to include to make space for global variables. The `globals:` label should be followed by a decimal number indicating the number of bytes.

Blocks (except for `globals:` and `strings:`) are then followed by one or more instructions, each on their own line. No block may have zero instructions. For readability, instructions should be indented by one level (spaces or a tab).

An instruction is a whitespace-delimited group of tokens all on one line. Whitespace is the only valid delimiter; commas or semicolons are illegal.

The first token is a mnemonic. These mnemonics can be located in the [official IronArc instruction reference](https://github.com/ChrisAkridge/IronArc/blob/master/Documentation/Instructions.md). Mnemonics are case-insensitive; `nop`, `NOP`, and `nOp` are all the same mnemonic. For readability, however, mnemonics should be all lowercase.

Some instructions require the size of the data they're operating on to be specified. There are four valid token values:

* `BYTE`: Specifies the size of the data is 1 byte (8 bits).
* `WORD`: Specifies the size of the data is 2 bytes (16 bits).
* `DWORD`: Specifies the size of the data is 4 bytes (32 bits).
* `QWORD`: Specifies the size of the data is 8 bytes (64 bits).

These four values, known as size tokens, are also case insensitive but should be all uppercase. These always occur immediately after the mnemonic if present.


The next tokens are 0 to 3 operand tokens. Operands can specify a processor register, memory address, numeric literal, or string literal. The specific use of each token, and its presence, is determined by the instruction being used.

A processor register operand takes one of the following values (case-insensitive but should be all lowercase):

```
eax ebx ecx edx eex efx egx ehx ebp esp eip erp eflags
```

An asterisk may come before the value (i.e. `*eax`). This specifies that the instruction should use the value in the register as an address into memory pointing to the desired value. Operands like this are called registers-with-pointer.

Registers-with-pointer may also be suffixed with an offset. An offset starts with a `+` or a `-` and then a decimal or hexadecimal number prefixed with `0x`, such as `*eax+24` or `*ecx-0x3F`. Offsets on non-pointer register operands are illegal. An offset is a number to add to the value of the register before using it as an address into memory.

A memory address operand is a hexadecimal number prefixed with `mem:0x`, composed of one to sixteen hex digits, such as `mem:0xFE` or `mem:0x0007FE24`. Memory address operand, like registers, can have an asterisk prefixed to them (i.e. `*mem:0x03F2D`), to specify that the value at this address should be treated as an address to another part of memory. Operands like this are called memory address pointers. Memory addresses cannot have offsets applied to them.

A numeric literal operand is a decimal, hexadecimal, or floating-point number that is used as an immediate value. Decimal numbers are merely sequences of digits (`123456`). Hexadecimal numbers are sequences of hex digits prefixed with `0x` (`0x1E240`). Floating-point numbers are numbers surrounded by `single()` or `double()` (`single(3.1415962)`, `double(2.71828)`).

A string literal is a string surrounded by double quotes (`"Hello, world!"`). Strings may contain the following escape characters:

* Single quote (`'`), written `\'` in the string
* Double quote (`"`), written `\"` in the string
* Null character (`U+0000`), written `\0`
* Alert character (`U+0007`), written `\a`
* Backspace character (`U+0008`), written `\b`
* Form feed character (`U+000C`), written `\f`
* Line feed character (`U+000A`), written `\n`
* Carriage return character (`U+000D`), written `\r`
* Horizontal tab character (`U+0009`), written `\t`
* Vertical tab character (`U+000B`), written `\v`
* Short Unicode code point (`U+0000 to U+FFFF`), written `\uXXXX` where `XXXX` is four hex digits
* Long Unicode code point (`U+000000 to U+10FFFF`), written `\uXXXXXXXX` where `XXXXXXXX` is eight hex digits.

Any instruction in which memory addresses are legal can accept label names in the place of those memory addresses. During assembly, the label names must be substituted for the address of the first instruction in the block named by that label.

Operands appear after the size token (if present) and the mnemonic. All hexadecimal sequences are case-insensitive but should be all uppercase.

The size token must be omitted in the following scenarios, even if the instruction otherwise requires it:

* The instruction has one operand, and the operand is a string literal. In this case, the size is implied to be `QWORD`, as the string becomes an entry in the string table whose address is a `QWORD`.
* The instruction has one operand, and the operand is a floating point numeric literal. The type of the literal (`single()` or `double()`) is used to determine the size as `DWORD` or `QWORD`, respectively.

Examples of valid instructions:

```
nop
jmp mem:0x3FFE
addl QWORD *eax *ebx-0xCF *ebp+24
push "Hello, world!"
hwcall "Terminal::WriteLine"
lshift DWORD
push DWORD 5
push DWORD 0x03
add
push single(3.5)
push single(8.2)
fsub
push double(3.1415962)
fsqrt
```

## IronArc Direct Assembly Format

IronArc Direct Assembly Format is a simpler but more explicit format that is used to produce IronArc Executable files. This format is the same as the Assembly format except for the following changes made in the translation phase:

* All comments are removed.
* All hexadecimal register offsets are converted into decimal (`*eax+0x3F` becomes `*eax+63`).
* All memory addresses are converted into eight-byte hex numbers with the `0x` prefix (`mem:0x0010FF` becomes `0x00000000000010FF`).
* All hexadecimal numeric literals are converted to decimal (`0x2AE` becomes `686`).
* All single- or double-precision floating point literals are converted first into their binary representation and then into a decimal value made from the binary representation (`single(3.1415962)` becomes `1078530026`).
* All string literals are scanned, any duplicates are removed, and the strings are placed into a string table in the order they were used in the file. The literal is then replaced with `str:` followed by the index of the string in the table (`"Hello, world!"` becomes `str:0`) (See "Strings Table" below.)
* Any omitted size tokens for string or floating-point literals are reinserted after the mnemonic token (`push "Hello, world!"` becomes `push QWORD str:0`).

### Strings Table
The strings table in IronArc Direct Assembly is a labelled block with the `strings:` label. What follows, one per line, is one or more string table entries, which are composed of an index in the table, a colon, then a quote-enclosed string. The same escape characters that are legal in Assembly format are legal in Direct Assembly format.

Indices for the entries must start from zero and increase by one for each entry. If no string literals are used in the file, a blank string ("") must be inserted at index 0.

An example string table:
```
strings:
	0: "Hello, world!"
	1: "Terminal::WriteLine"
	2: "This\r\nis\r\n\a\r\nmultiline\r\nstring with null termination for some reason\0"
	3: "\u2764"
```

## IronArc Executable Format
After translation, any assembler must produce a file in the IronArc Executable Format. An IronArc Executable file is a binary file containing a header, a sequence of instructions, and a strings table. Assemblers must offer the ability to produce the executable file in little-endian or big-endian format.

The header is composed of the following data:

| **Field**                 | **Size** | **Value**                                                                                                                                                                                 | **Endianness**                                         |
|---------------------------|----------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------|
| Magic Number              | 4 bytes  | The ASCII value "`IEXE`" (`0x49455845`)                                                                                                                                                   | Always stored in that order, regardless of endianness. |
| Specification Version     | 4 bytes  | Two two-byte numbers that compose a major and minor version. *The current specification is version `0x00010002`.*                                                                         | Reverse bytes if producing little endian.              |
| Assembler Version         | 4 bytes  | Two two-byte numbers that compose a major and minor version. Hand-written IronArc Executable files should have version `0x00000000`. An assembler can use any two numbers for this value. | Reverse bytes if producing little endian.              |
| First Instruction Address | 8 bytes  | The address of the start of the first instruction (the first byte of the opcode).                                                                                                         | Reverse bytes if producing little endian.              |
| String Table Address      | 8 bytes  | The address of the start of the string table (the first byte of the number of strings in the table).                                                                                      | Reverse bytes if producing little endian.              |

Immediately following the header is a group of `00` bytes, the count of which is defined in the `globals:` block label at the start of the assembly file. If the byte count is `0`, there are no bytes emitted.

Following the global variable bytes is a sequence of instructions. These instructions must be generated from the blocks in the assembly file, in the order the blocks where declared in the file (ignoring the `strings:` block). Instructions have no padding or alignment requirements.

Each instruction starts with a two-byte opcode as defined by the instruction reference. The bytes should be written in reverse order if producing a little-endian file (the opcode for `jmp` would be written as `02 00`).

If the instruction has any operands or a defined size, it is followed with a flags byte which uses the value of every pair of bits to determine the size or type of the operands. The highest two bits (`1100 0000`) always declare the size (`00` maps to BYTE, `01` maps to WORD, `10` maps to DWORD, and `11` maps to QWORD). The remaining six bits define the type of operand.

All one-operand instructions use the second highest bit pair (`0011 0000`) to define the type of the operand (`00` are memory addresses, `01` are processor registers, `10` are numeric literals, and `11` are string literals).

Long-form mathematical or bitwise operations that take only one input and one output operand (inverse, bitwise NOT, increment, decrement) use the second-highest and lowest bit pairs (`0011 0011`) to define the types of the input and output operands.

All other two operand instructions use the second- and third-highest bit pairs (`0011 1100`) to define the types of the operands.

All three operand instructions use all six bits (`0011 1111`) to define the types of the operands.

Following the flags byte are the bytes of the operands, if any. These bytes are encoded as follows:

| **Operand Type**   | **Encoding**                                                                                                                                                                                                                                                                                                           |
|--------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Memory address     | Eight bytes containing the address as a QWORD. If the high bit is set (`0x8000000000000000`), the value at the memory address is treated as a pointer. Memory addresses must be produced as the result of jumps to named labels. The address produced must be the first byte of the first instruction of the label.    |
| Processor register | One byte. The highest bit (`0x80`) is set if the register contains a pointer. The next highest bit (`0x40`) is set if the register has an offset, in which case four more bytes containing a signed DWORD representing the register follow. The next six bits (`0x3F`) are used as the index of the specific register. |
| Numeric literal    | A one, two, four, or eight byte value containing the value of the literal.                                                                                                                                                                                                                                             |
| String literal     | A four byte value containing the index of the string in the table as a DWORD.                                                                                                                                                                                                                                          |

If a little-endian file is being produced, all the operand bytes above must be in reverse order.

Following all instructions is the strings table. The strings table starts with an unsigned DWORD that specifies how many strings are in the table. Next is an array of QWORD values that name the address of each string in the table.

Following the count and addresses are each string in the table, in the order they were declared in the file. Each string starts with an unsigned DWORD stating the number of bytes in the rest of the string. The string itself, encoded in UTF-8, follows.

All numeric values in the string table (string count, addresses, byte counts) must be in reverse order if a little-endian file is being produced.

## Disassembler
IronAssembler will support the ability to disassemble IronArc executable files back into IronArc Direct Assembly. The disassembler will support disassembling entire programs or sequeunces of bytes at a pointer or from a memory stream.

The core of the disassembler is the ability to disassemble individual instructions. The disassembler starts by reading the two-byte opcode and looking up information about the operands that follow, if any. Next, the flags byte is read (if present) and used to determine the size of the data and the types of the operands that follow.

Each individual operand type can be disassembled with relative ease by merely inspecting the bytes that compose them. Registers can be disassembled by inspecting their highest bits to see if the operand is used as a pointer and if it has an offset. Memory addresses can be read verbatim as well as string table entries and numeric literals.

Disassembly of an entire *.iexe file is also rather straightforward. The header, being at the start of the file, is both easy and useful, as it stores the address of the strings table. This allows the strings table to be properly disassembled.

Entire *.iexe file disassembly is intended to produce a text file that can then be fed back into IronAssembler to produce the same iexe file.

Another key usage of disassemblers is in debuggers. Typically, a debugger gives a view into only a portion of the program's code at a time, typically in a window the user can scroll in or set to a different location of the program.

Thus, IronAssembler's disassembler, in addition to the ability to disassemble entire *.iexe files, will support the concept of disassembly windows. A disassembly window is composed of an address into a block of bytes (typically memory) and a number of instructions to disassemble from that address. Each disassembled instruction is additionally marked with its address in memory.

Every block of bytes is expected to have bytes that are not IronArc instructions. Each pair of such invalid bytes in a disassembly window will be displayed as `?? ??`.