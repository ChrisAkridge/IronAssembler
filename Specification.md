# IronAssembler Specification

The IronAssembler program assembles a text file containing IronArc assembly into a flat binary containing executable IronArc code.

[IronArc](https://github.com/Celarix/IronArc) is a virtual processor and computing platform designed for the study of how computers and processors work, and is not intended for serious usage. IronArc runs on executable code written according to a variable-width [instruction set](https://github.com/Celarix/IronArc/blob/master/Documentation/Simple%20Instruction%20Listing.txt).

## Input Format
IronAssembler assembles sequences of instructions bundled beneath labels. Labels denote the beginnings and ends of sequences of instructions and can be used as references within instructions. For instance, `jmp MyLabel` will be assembled into a sequence of bytes where the label is replaced with the address where `MyLabel` begins.

IronAssembler may assemble from one file containing assembly instructions. This file will adhere to the following grammar:

```
  assembly-file:
    Zero or more label-sections

  label-section
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
    One of { nop ret end add sub mult div mod inv eq ineq lt gt lteq gteq and or not bwnot bwand bwor bwxor bwlshift bwrshift }, case insensitive

  one-operand-instruction:
    One of { jmp jmpa call calla push pop peek stackalloc hwcall setflag clearflag testflag toggleflag }, case insensitive then
	whitespace then
	address-block

  two-operand-instruction:
	One of { jz jnz mov notl bwnotl arrayalloc deref cbyte csbyte cshort cushort cint cuint clong culong csing cdouble }, case insensitive then
	whitespace then
	2 address-blocks

  three-operand-instruction:
	One of { je jne jlt jgt jlte jgte addl subl multl divl modl ineql ltl gtl lteql gteql andl orl bwandl bworl bwxorl bwlshitl bwrshiftl arrayaccess }, case insensitive then
	whitespace then
	3 address-blocks

  address-block:
    memory-address OR
	processor-register OR
	stack-index OR
	numeric-literal OR
	string-literal
	
  processor-register:
    One of { eax ebx ecx edx eex efx egx ehx eflags eip esp ebp ssp sbp erb nul }, case insensitive
```