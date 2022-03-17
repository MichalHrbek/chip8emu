using SDL2;
using System.Collections.Generic;

namespace chip8emu.engine
{
    class PollEvents
    {
        public delegate void EventCallback(SDL.SDL_Event e);

        public static Dictionary<SDL.SDL_EventType, List<EventCallback>> events = new();

        public static void poll()
        {
            SDL.SDL_Event e;

            while (SDL.SDL_PollEvent(out e) != 0)
            {
                List<EventCallback> callbacks;
                if (events.TryGetValue(e.type, out callbacks))
                {
                    for (int j = 0; j < callbacks.Count; j++)
                    {
                        callbacks[j](e);
                    }
                }
            }
        }

        public static void addCallback(SDL.SDL_EventType e, EventCallback callback)
        {
            if (events.ContainsKey(e)) events[e].Add(callback);
            else events.Add(e, new List<EventCallback>() { callback });
        }

        public static void removeCallback(SDL.SDL_EventType e, EventCallback callback)
        {
            if (events.ContainsKey(e)) events[e].Remove(callback);
        }
    }
}
