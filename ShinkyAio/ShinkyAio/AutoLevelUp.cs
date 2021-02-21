using System;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;

namespace ShinkyAio
{
    class AutoLevelUp
    {
        int lvl1, lvl2, lvl3, lvl4;

        private static Menu menu;
        public void Load()
        {
            menu = new Menu("AutoLevel", "Auto Level Up", true);
            menu.Add(new MenuBool("lvl", "Enable"));
            menu.Add(new MenuList("1", "1", new[] { "Q", "W", "E", "R" }, 3));
            menu.Add(new MenuList("2", "2", new[] { "Q", "W", "E", "R" }, 1));
            menu.Add(new MenuList("3", "3", new[] { "Q", "W", "E", "R" }, 1));
            menu.Add(new MenuList("4", "4", new[] { "Q", "W", "E", "R" }, 1));
            menu.Add(new MenuSlider("Start", "Auto Level Start", 2, 1, 6));
            menu.Attach();
           
            Game.OnUpdate += Game_OnGameUpdate;
            AIHeroClient.OnLevelUp += Obj_AI_Base_OnLevelUp;
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (!menu["lvl"].GetValue<MenuBool>().Enabled)
                return;
            lvl1 = menu["1"].GetValue<MenuList>().Index;
            lvl2 = menu["2"].GetValue<MenuList>().Index;
            lvl3 = menu["3"].GetValue<MenuList>().Index;
            lvl4 = menu["4"].GetValue<MenuList>().Index;
        }

        private void Obj_AI_Base_OnLevelUp(AIHeroClient sender, AIHeroClientLevelUpEventArgs aiHeroClientLevelUpEventArgs)
        {
            if (!sender.IsMe || !menu["lvl"].GetValue<MenuBool>().Enabled || ObjectManager.Player.Level < menu["Start"].GetValue<MenuSlider>().Value)
                return;
            if (lvl2 == lvl3 || lvl2 == lvl4 || lvl3 == lvl4)
                return;
            Up(lvl1);
            Up(lvl2);
            Up(lvl3);
            Up(lvl4);
        }

        private void Up(int lvl)
        {
            if (ObjectManager.Player.Level < 3)
            {
                if (lvl == 0 && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level == 0)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                if (lvl == 1 && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level == 0)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                if (lvl == 2 && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level == 0)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
            }
            else
            {
                if (lvl == 0)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                if (lvl == 1)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                if (lvl == 2)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                if (lvl == 3)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
            }
        }
    }
}
