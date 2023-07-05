// Program: Sum.asm
// adds the values from 1 to n 
// n is stored in R[0]
// the sum gets stored in R[1]

    @R0
    D=M // get the value stored in R[0] and load it in the D register

    @n
    M=D // @n = R[0]

    @R1
    M=0 // nullify R[1]

    @i
    M=1 // set the counter to 1

(LOOP)
    @n
    D=M // load @n in D register

    @i
    D=D-M // load in D register @n - @i

    @END
    D;JLT // terminate if (@n - @i) < 0

    @i
    D=M // load @i in D register

    @R1
    M=M+D // increment the sum value with i

    @i
    M=M+1 // increment @i

    @LOOP
    0;JMP // goto beginning of loop

(END)
    @END
    0;JMP
