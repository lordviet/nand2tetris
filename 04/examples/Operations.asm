// D = 10
@10
D=A

// D++
D=D+1

// D=RAM[17]
@17
D=M

// RAM[17]=D
@17
M=D

// RAM[17]=10
@10
D=A
@17
M=D

// RAM[5] = RAM[3]
@5
D=M

// @3
// M=D

// Hack features built-in symbols
// R0 -> 0
// R1 -> 1
// R2 -> 2
// ...
// R15 -> 15
// SCREEN -> 16384
// KBD -> 24576

// These symbols can be used as virtual registers

// For example 
// Instead of representing RAM[15] = 5 as

// @5
// D=A
// @15
// M=D

// We can use virtual registers

// @5
// D=A
// @R15
// M=D
