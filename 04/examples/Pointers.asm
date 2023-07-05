// for (i=0; i<n; i++){
//     arr[i] = -1
// }

// Suppose that arr = 100 and n = 10

// Variables that store memory addresses,
// like arr are called pointers

// Hack pointer logic: we use A instruction when we have to access memory using a pointer
// Semantics: set the address register to the contents of some memory register

// arr = 100
@100
D=A

@arr
M=D

// n = 10
@10
D=A

@n
M=D

// i = 0
@i
M=0

(LOOP)
    // if (i == n) goto END
    @n
    D=M

    @i
    D=D-M

    @END
    D;JEQ

    // RAM[arr+i] = -1
    @arr
    D=M // store the value of arr to the D register

    @i // M register becomes i
    A=D+M // Get address arr + i 
    M=-1 // Assign value -1 to the register address loaded in A

    // i++
    @i
    M=M+1

    @LOOP
    0;JMP

(END)
    @END
    0;JMP