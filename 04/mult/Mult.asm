// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/04/Mult.asm

// Multiplies R0 and R1 and stores the result in R2.
// (R0, R1, R2 refer to RAM[0], RAM[1], and RAM[2], respectively.)
//
// This program only needs to handle arguments that satisfy
// R0 >= 0, R1 >= 0, and R0*R1 < 32768.

// Put your code here.
    @i // access memory
    M=1 // assign it value 1

    @R2 // access memory of where product will be stored
    M=0 // assign it value 0    

(LOOP)
    @i
    D=M // D=i
    
    @R1 // Whatever is stored in R1
    D=D-M
    
    @END
    D;JGT // If (i-R1)>0 goto END

    @R0
    D=M // D=R0
    
    @R2 // product
    M=D+M // R2 = R2 + R0

    @i
    M=M+1 // i = i + 1

    @LOOP
    0;JMP
(END)
    @END
    0;JMP