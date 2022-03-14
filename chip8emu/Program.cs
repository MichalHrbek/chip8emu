using Raylib_cs;
using System;

namespace chip8emu
{
    class Program
    {
        static void Main(string[] args)
        {
            chip8 vm = new chip8();
            vm.init();
            vm.loadGame(@"C:\Users\hppcp\Downloads\pong.rom");

            Color displayColor = Color.GREEN;

            int[] keys = new int[] { 49, 50, 51, 52, 81, 87, 69, 82, 65, 83, 68, 70, 90, 88, 67, 86 };

            int scale = 15;
            Raylib.InitWindow(64 * scale, 32 * scale, "Hello World");
            Raylib.SetTargetFPS(60);

            while (!Raylib.WindowShouldClose())
            {
                for (int i = 0; i < vm.key.Length; i++)
                {
                    vm.key[i] = Raylib.IsKeyDown((KeyboardKey)keys[i]);
                }
                vm.emulateCycle();

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.WHITE);

                for (int i = 0; i < vm.gfx.Length; i++)
                {
                    Raylib.DrawRectangle((i % 64) * scale, (i / 64) * scale, scale, scale, new Color(vm.gfx[i] * displayColor.r, vm.gfx[i] * displayColor.g, vm.gfx[i] * displayColor.b, (byte)255));
                }

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}
