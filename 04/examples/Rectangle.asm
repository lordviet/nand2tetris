// Program: Rectangle.asm
// Draw a filled rectangle 16 pixels wide and RAM[0] pixels long

    @i
    M=0 // i = 0

    @R0
    D=M

    @n
    M=D // n = RAM[0]

    @SCREEN
    D=A

    @address
    M=D // address = 16384 (base address of the Hack screen)

(LOOP)
    @i
    D=M

    @n
    D=D-M

    @END
    D;JEQ // TODO: Possibly JGT

    @address
    A=M
    M=-1 // RAM[address] = -1

    @i
    M=M+1

    @32
    D=A

    @address
    M=D+M // address = address + 32

    @LOOP
    0;JMP

(END)
    @END
    0;JMP