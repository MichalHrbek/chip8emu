using System;
using System.Drawing;
using SDL2;

namespace chip8emu
{
    class Program
    {
        static void Main(string[] args)
        {
            chip8 vm = new chip8();
            vm.init();
            vm.loadGame(@"C:\Users\hppcp\Downloads\pong.rom");

            Color displayColor = Color.Green;

            int[] keys = new int[] { 49, 50, 51, 52, 113, 119, 101, 114, 97, 115, 100, 102, 122, 120, 99, 118 };

            int scale = 15;

            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine("Error initializing SDL. Error: {0}", SDL.SDL_GetError());
                return;
            }

            IntPtr window = SDL.SDL_CreateWindow("Chip8", SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, 64 * scale, 32 * scale, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            IntPtr renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

            SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
            SDL.SDL_RenderClear(renderer);

            engine.Input.SetupInput();

            bool shouldQuit = false;

            engine.PollEvents.addCallback(SDL.SDL_EventType.SDL_QUIT, (SDL.SDL_Event e) => { shouldQuit = true; }); // Quit window event

            while (!shouldQuit)
            {
                engine.PollEvents.poll();
                for (int i = 0; i < vm.key.Length; i++)
                {
                    vm.key[i] = engine.Input.isKeyDown((byte)keys[i]);
                }
                vm.emulateCycle();
                if (vm.screenSchanged())
                {
                    for (int i = 0; i < vm.gfx.Length; i++)
                    {
                        SDL.SDL_Rect rect = new SDL.SDL_Rect() { x = (i % 64) * scale, y = (i / 64) * scale, h = scale, w = scale};
                        SDL.SDL_SetRenderDrawColor(renderer, (byte)(vm.gfx[i] * displayColor.R), (byte)(vm.gfx[i] * displayColor.G), (byte)(vm.gfx[i] * displayColor.B), 255);
                        SDL.SDL_RenderFillRect(renderer, ref rect);
                    }
                    SDL.SDL_RenderPresent(renderer);
                }
            }

            // Clean up.
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
        }
    }
}
