// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/04/Fill.asm

// Runs an infinite loop that listens to the keyboard input.
// When a key is pressed (any key), the program blackens the screen,
// i.e. writes "black" in every pixel;
// the screen should remain fully black as long as the key is pressed. 
// When no key is pressed, the program clears the screen, i.e. writes
// "white" in every pixel;
// the screen should remain fully clear as long as no key is pressed.

// Put your code here.
(LIFECYCLE)
    @i
    M=0 // i = 0

    @y
    M=0 // y = 0

    @32
    D=A

    @width
    M=D // width = 32

    @255
    D=A

    @height
    M=D // height = 255

    @SCREEN
    D=A

    @address
    M=D // address = 16384 (base address of the Hack screen)

    @KBD // Access keyboard memory map
    D=M // Get the input and store it to the data register

    @DEACTIVATE
    D;JEQ // if (input == 0) goto DEACTIVATE

    @ACTIVATE
    0;JMP // else goto ACTIVATE

// Outer Loop that takes care of the rows
(OUTER)
    @i
    D=M

    @height
    D=D-M

    @LIFECYCLE
    D;JEQ

    @y
    M=0

    @INNER
    0;JMP

// Inner Loop that takes care of the columns
(INNER)
    @y
    D=M

    @width
    D=D-M

    @LOOPBRIDGE
    D;JEQ

    @pixel
    D=M

    @address
    A=M
    M=D

    @y
    M=M+1

    @address
    M=M+1 // address = address + 1

    @INNER
    0;JMP

// Cleanup after outer and preparation for the next iteration of Outer
(LOOPBRIDGE)
    @i
    M=M+1

    @OUTER
    0;JMP

(DEACTIVATE)
    @pixel
    M=0 // pixel becomes white

    @OUTER
    0;JMP

(ACTIVATE)
    @pixel
    M=-1 // pixel becomes black

    @OUTER
    0;JMP
    