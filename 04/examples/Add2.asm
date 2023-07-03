// Program: Add2.asm
// Computes: RAM[2] = RAM[0] + RAM[1]
// Usage: put values in RAM[0] and RAM[1]

@R0
D=M // D = RAM[0]

@R1
D=D+M // D = D + RAM[1]

@R2
M=D // RAM[2]=D

// Terminate with infinite loop to prevent NOP slide attack
@6 // Line to jump to
0;JMP