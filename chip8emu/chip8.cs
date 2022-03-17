using System;
using System.IO;
using System.Timers;

namespace chip8emu
{
    class chip8
    {
        ushort opcode;
        byte[] memory;
        byte[] V;
        ushort I;
        ushort pc;
        public byte[] gfx;
        byte delay_timer;
        byte sound_timer;
        ushort[] stack;
        ushort sp;
        public bool[] key;
        bool screenFlag;
        Random r;

        byte[] fontset = new byte[] {
            0xF0, 0x90, 0x90, 0x90, 0xF0,		// 0
	        0x20, 0x60, 0x20, 0x20, 0x70,		// 1
	        0xF0, 0x10, 0xF0, 0x80, 0xF0,		// 2
	        0xF0, 0x10, 0xF0, 0x10, 0xF0,		// 3
	        0x90, 0x90, 0xF0, 0x10, 0x10,		// 4
	        0xF0, 0x80, 0xF0, 0x10, 0xF0,		// 5
	        0xF0, 0x80, 0xF0, 0x90, 0xF0,		// 6
	        0xF0, 0x10, 0x20, 0x40, 0x40,		// 7
	        0xF0, 0x90, 0xF0, 0x90, 0xF0,		// 8
	        0xF0, 0x90, 0xF0, 0x10, 0xF0,		// 9
	        0xF0, 0x90, 0xF0, 0x90, 0x90,		// A
	        0xE0, 0x90, 0xE0, 0x90, 0xE0,		// B
	        0xF0, 0x80, 0x80, 0x80, 0xF0,		// C
	        0xE0, 0x90, 0x90, 0x90, 0xE0,		// D
	        0xF0, 0x80, 0xF0, 0x80, 0xF0,		// E
	        0xF0, 0x80, 0xF0, 0x80, 0x80		// F
        };

        public void init()
        {
            opcode = 0;
            memory = new byte[4096];
            V = new byte[16];
            I = 0;
            pc = 0x200;
            gfx = new byte[64 * 32];
            delay_timer = 0;
            sound_timer = 0;
            stack = new ushort[16];
            sp = 0;
            key = new bool[16];
            screenFlag = true;
            r = new Random();

            for (int i = 0; i < 80; ++i)
                memory[i] = fontset[i];
        }

        public void loadGame(string filePath)
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            bytes.CopyTo(memory, 512);
        }

        public void emulateCycle(Object source, ElapsedEventArgs e)
        {
            emulateCycle();
        }

        public bool screenSchanged()
        {
            if (screenFlag)
            {
                screenFlag = false;
                return true;
            }
            else return false;
        }

        public void emulateCycle()
        {
            opcode = (ushort)(memory[pc] << 8 | memory[pc + 1]);
            OpCode opc = new OpCode(opcode);

            //Console.WriteLine(opcode.ToString("X"));

            if (delay_timer > 0) --delay_timer;

            if (sound_timer > 0)
            {
                if (sound_timer == 1) Console.Beep(440, 100);
                --sound_timer;
            }

            switch (opcode & 0xF000)
            {
                case 0x0000:
                    switch (opcode & 0x00FF)
                    {
                        case 0x00E0: // Clears the screen      
                            for (int i = 0; i < 64 * 32; ++i)
                                gfx[i] = 0;
                            pc += 2;
                            break;

                        case 0x00EE: // Returns from a subroutine          
                            sp -= 1;
                            pc = stack[sp];
                            pc += 2;
                            break;

                        case 0x0000:
                            //Console.WriteLine("exec" + pc);
                            pc += 2;
                            break;

                        default:
                            Console.WriteLine("Unknown opcode: 0x{0}", opcode.ToString("X"));
                            break;
                    }
                    break;

                case 0x1000: // Jumps to address NNN
                    pc = opc.NNN;
                    break;

                case 0x2000: // Calls subroutine at NNN
                    stack[sp] = pc;
                    sp++;
                    pc = opc.NNN;
                    break;

                case 0x3000: // Skips the next instruction if VX equals NN
                    if (V[opc.X] == opc.NN) pc += 4;
                    else pc += 2;
                    break;

                case 0x4000: // Skips the next instruction if VX does not equal NN
                    if (V[opc.X] != opc.NN) pc += 4;
                    else pc += 2;
                    break;

                case 0x5000: // Skips the next instruction if VX equals VY
                    if (V[opc.X] == V[opc.Y]) pc += 4;
                    else pc += 2;
                    break;

                case 0x6000: // Sets VX to NN
                    V[opc.X] = opc.NN;
                    pc += 2;
                    break;

                case 0x7000: // Adds NN to VX
                    V[opc.X] += opc.NN;
                    pc += 2;
                    break;

                case 0x8000: // Math
                    switch (opcode & 0x000F)
                    {
                        case 0x0000: // Sets VX to the value of VY
                            V[opc.X] = V[opc.Y];
                            pc += 2;
                            break;

                        case 0x0001: // Sets VX to VX or VY
                            V[opc.X] |= V[opc.Y];
                            pc += 2;
                            break;

                        case 0x0002: // Sets VX to VX and VY
                            V[opc.X] &= V[opc.Y];
                            pc += 2;
                            break;

                        case 0x0003: // Sets VX to VX xor VY
                            V[opc.X] ^= V[opc.Y];
                            pc += 2;
                            break;

                        case 0x0004: // Sets VX to VX xor VY
                            V[0xF] = (byte)(V[opc.X] + V[opc.Y] > 0xFF ? 1 : 0);
                            V[opc.X] += V[opc.Y];
                            pc += 2;
                            break;

                        case 0x0005: // VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there is not
                            V[0xF] = (byte)(V[opc.X] > V[opc.Y] ? 1 : 0);
                            V[opc.X] -= V[opc.Y];
                            pc += 2;
                            break;

                        case 0x0006: // Stores the least significant bit of VX in VF and then shifts VX to the right by 1
                            V[0xF] = (byte)((V[opc.X] & 0x1) != 0 ? 1 : 0);
                            V[opc.X] >>= 1;
                            pc += 2;
                            break;

                        case 0x0007: // Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there is not
                            V[0xF] = (byte)(V[opc.Y] > V[opc.X] ? 1 : 0);
                            V[opc.X] = (byte)(V[opc.Y] - V[opc.X]);
                            pc += 2;
                            break;

                        case 0x000E: // Stores the most significant bit of VX in VF and then shifts VX to the left by 1
                            V[0xF] = (byte)((V[opc.X] & 0xF) != 0 ? 1 : 0);
                            V[opc.X] <<= 1;
                            pc += 2;
                            break;

                        default:
                            Console.WriteLine("Unknown opcode: 0x{0}", opcode.ToString("X"));
                            break;
                    }
                    break;

                case 0x9000: // Skips the next instruction if VX does not equal VY
                    if (V[opc.X] != V[opc.Y]) pc += 4;
                    else pc += 2;
                    break;

                case 0xA000: // Sets I to the address NNN
                    I = opc.NNN;
                    pc += 2;
                    break;

                case 0xB000: // Jumps to the address NNN plus V0
                    pc = (ushort)(opc.NNN + V[0]);
                    break;

                case 0xC000: // Sets VX to the result of a bitwise and operation on a random number and NN
                    V[opc.X] = (byte)(r.Next(0, 255) & opc.NN);
                    pc += 2;
                    break;

                case 0xD000: // Draws a sprite
                    {
                        ushort x = V[(opcode & 0x0F00) >> 8];
                        ushort y = V[(opcode & 0x00F0) >> 4];
                        ushort height = (ushort)(opcode & 0x000F);
                        ushort pixel;

                        V[0xF] = 0;
                        for (int yline = 0; yline < height; yline++)
                        {
                            pixel = memory[I + yline];
                            for (int xline = 0; xline < 8; xline++)
                            {
                                if ((pixel & (0x80 >> xline)) != 0)
                                {
                                    byte posX = (byte)((x + xline) % 64);
                                    byte posY = (byte)((y + yline) % 32);

                                    ushort posPixel = (ushort)(posX + ((posY) * 64));

                                    if (gfx[posPixel] == 1)
                                        V[0xF] = 1; // set vf register
                                    gfx[posPixel] ^= 1;
                                    screenFlag = true;
                                }
                            }
                        }

                        pc += 2;
                        break;
                    }

                case 0xE000: // Keypress
                    switch (opcode & 0x00FF)
                    {
                        case 0x009E: // Skips the next instruction if the key stored in VX is pressed
                            if (key[V[opc.X]]) pc += 4;
                            else pc += 2;
                            break;

                        case 0x00A1: // Skips the next instruction if the key stored in VX is not pressed
                            if (!key[V[opc.X]]) pc += 4;
                            else pc += 2;
                            break;

                        default:
                            Console.WriteLine("Unknown opcode: 0x{0}", opcode.ToString("X"));
                            break;
                    }
                    break;

                case 0xF000:
                    switch (opcode & 0x00FF)
                    {
                        case 0x0007: // Sets VX to the value of the delay timer.
                            V[opc.X] = delay_timer;
                            pc += 2;
                            break;

                        case 0x000A: // A key press is awaited, and then stored in VX
                            for (byte i = 0; i < key.Length; i++)
                                if (key[i]) { V[opc.X] = i; pc += 2; }
                            break;

                        case 0x0015: // Sets the delay timer to VX
                            delay_timer = V[opc.X];
                            pc += 2;
                            break;

                        case 0x0018: // Sets the sound timer to VX
                            sound_timer = V[opc.X];
                            pc += 2;
                            break;

                        case 0x001E: // Adds VX to I
                            I += V[opc.X];
                            pc += 2;
                            break;

                        case 0x0029: // Sets I to the location of the sprite for the character in VX
                            I = (ushort)(V[opc.X] * 5);
                            pc += 2;
                            break;

                        case 0x0033: // FX33: Stores the Binary-coded decimal representation of VX at the addresses I, I + 1, and I + 2
                            memory[I] = (byte)(V[(opc.X) >> 8] / 100);
                            memory[I + 1] = (byte)(V[(opc.X) >> 8] / 10 % 10);
                            memory[I + 2] = (byte)(V[opc.X >> 8] % 100 % 10);
                            pc += 2;
                            break;

                        case 0x0055: // Stores from V0 to VX (including VX) in memory, starting at address I
                            for (int i = 0; i <= opc.X; i++)
                                memory[I + i] = V[i];
                            pc += 2;
                            break;

                        case 0x0065: // Fills from V0 to VX (including VX) with values from memory, starting at address I
                            for (int i = 0; i <= opc.X; i++)
                                V[i] = memory[I + i];
                            pc += 2;
                            break;
                        default:
                            Console.WriteLine("Unknown opcode: 0x{0}", opcode.ToString("X"));
                            break;
                    }
                    break;




                default:
                    Console.WriteLine("Unknown opcode: 0x{0}", opcode.ToString("X"));
                    break;
            }
        }
    }

    class OpCode
    {
        public ushort NNN;
        public byte NN, X, Y, N;

        public OpCode(ushort opcode)
        {
            NNN = (ushort)(((opcode << 4) >> 4) & 0x0FFF);
            NN = (byte)(((opcode << 8) >> 8) & 0x00FF);
            N = (byte)(((opcode << 12) >> 12) & 0x000F);
            X = (byte)(((opcode << 4) >> 12) & 0x000F);
            Y = (byte)(((opcode << 8) >> 12) & 0x000F);
        }

    }
}
