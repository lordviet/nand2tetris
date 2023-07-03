// Program: Signum.asm
// Computes: if R0 > 0
//              R1 = 1
//           else
//              R1 = 0

    @R0
    D=M // D=R[0]

    @POSITIVE // using the label
    D;JGT // If R0 > 0 GOTO 8

    @R1
    M=0 // R1 = 0

    @END
    0;JMP

(POSITIVE) // declaring a label for positive scenario
    @R1
    M=1 // R1 = 1

(END) // declaring a label for terminating the program
    @END
    0;JMP