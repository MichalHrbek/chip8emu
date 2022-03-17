using SDL2;

namespace chip8emu.engine
{
    class Input
    {
        public static bool[] keysDown = new bool[256];

        public static void SetupInput()
        {
            PollEvents.addCallback(SDL.SDL_EventType.SDL_KEYDOWN, KeyDownCallback);
            PollEvents.addCallback(SDL.SDL_EventType.SDL_KEYUP, KeyUpCallback);
        }

        public static void KeyDownCallback(SDL.SDL_Event e)
        {
            keysDown[(byte)e.key.keysym.sym] = true;
        }

        public static void KeyUpCallback(SDL.SDL_Event e)
        {
            keysDown[(byte)e.key.keysym.sym] = false;
        }

        public static bool isKeyDown(byte key)
        {
            return keysDown[key];
        }
    }
}
