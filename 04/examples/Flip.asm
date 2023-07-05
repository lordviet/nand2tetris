// Program: Flip.asm
// flips the values of
// RAM[0] and RAM[1]

    @R0
    D=M

    @temp
    M=D // TEMP = R0

    @R1
    D=M

    @R0
    M=D // R0 = R1

    @temp
    D=M

    @R1
    M=D // R1 = TEMP

(END)
    @END
    0;JMP