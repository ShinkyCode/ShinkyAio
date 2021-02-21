using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ShinkyAio
{
    class Program
    {
        static void Main(string[] args)
        {
            GameEvent.OnGameLoad += OnLoadGame;
        }

        private static void OnLoadGame()
        {

            new AutoLevelUp().Load();

            if (ObjectManager.Player.CharacterName == "Blitzcrank")
            {
                new ShinkyBlitz().OnLoad();
            }
        }
    }
}
