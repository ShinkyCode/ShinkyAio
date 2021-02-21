using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

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
            var pred = new Menu("spred", "Predict Selection", true);
            SPrediction.Prediction.Initialize(pred);
            pred.Attach();

            new AutoLevelUp().Load();

            if (ObjectManager.Player.CharacterName == "Blitzcrank")
            {
                new ShinkyBlitz().OnLoad();
            }
        }
    }
}
