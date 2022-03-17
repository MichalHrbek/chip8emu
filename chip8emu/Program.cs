using System;
using System.Drawing;
using SDL2;

namespace chip8emu
{
    class Program
    {
        static void Main(string[] args)
        {
            chip8 vm;
            Color displayColor = Color.Green;
            int[] keys = new int[] { 120, 49, 50, 51, 113, 119, 101, 97, 115, 100, 122, 99, 52, 114, 102, 118};

            int scale = 15;

            void load()
            {
                vm = new chip8();
                vm.init();
                if (args.Length > 0) vm.loadGame(args[0]);
                else Console.WriteLine("Specify the rom");
            }

            load();

            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine("Error initializing SDL. Error: {0}", SDL.SDL_GetError());
                return;
            }

            IntPtr window = SDL.SDL_CreateWindow("Chip8", SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, 64 * scale, 32 * scale, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            IntPtr renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
            SDL.SDL_RenderClear(renderer);

            engine.Input.SetupInput();

            bool shouldQuit = false;

            engine.PollEvents.addCallback(SDL.SDL_EventType.SDL_QUIT, (SDL.SDL_Event e) => { shouldQuit = true; }); // Quit window event
            while (!shouldQuit)
            {

                DateTime start = DateTime.UtcNow;
                engine.PollEvents.poll();
                
                if (engine.Input.isKeyDown((byte)SDL.SDL_Keycode.SDLK_ESCAPE)) load();
                
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

                DateTime end = DateTime.UtcNow;
                float elapsedMS = (float)(end - start).TotalMilliseconds;
                if(1.6666f - elapsedMS > 0) SDL.SDL_Delay((uint)(1.6666f - elapsedMS));
            }

            // Clean up.
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
        }
    }
}
