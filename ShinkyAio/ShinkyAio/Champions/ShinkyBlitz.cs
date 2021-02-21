using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using Color = System.Drawing.Color;

namespace ShinkyAio
{
    class ShinkyBlitz
    {
        internal static Menu mainMenu;
        internal static Spell Q, W, E, R;
        internal static AIHeroClient Player => ObjectManager.Player;

        internal static int Limit;

        public void OnLoad()
        {

            Q = new Spell(SpellSlot.Q, 1150f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 600f);

            Q.SetSkillshot(0.25f, 140f, 1800f, true, SpellType.Line);

            mainMenu = new Menu("Blitz", "ShinkyAio -> Blitz", true);

            var combo = new Menu("Combo", "Combo");
            combo.Add(new MenuBool("useqcombo", "Use Q"));
            combo.Add(new MenuBool("useecombo", "Use E"));
            combo.Add(new MenuBool("usercombo", "Use R"));
            mainMenu.Add(combo);

            var auGrab = new Menu("AutoGrab", "Auto Grab On");
            foreach (var hero in GameObjects.EnemyHeroes)
                auGrab.Add(new MenuBool("auq" + hero.CharacterName, hero.CharacterName, false));
            mainMenu.Add(auGrab);
            auGrab.Add(new MenuBool("autogimmobile", "Immobile/Rooted"));
            auGrab.Add(new MenuBool("autogdashing", "Dashing"));
            auGrab.Add(new MenuBool("autogcasting", "Casting/AA"));
            

            var blqmenu = new Menu("BLK", "Blacklist");
            foreach (var hero in GameObjects.EnemyHeroes)
                blqmenu.Add(new MenuBool("blq" + hero.CharacterName, hero.CharacterName, false));
            mainMenu.Add(blqmenu);

            var flee = new Menu("Flee", "Flee");
            flee.Add(new MenuBool("usewflee", "Use W"));
            flee.Add(new MenuBool("useeflee", "Use E"));
            mainMenu.Add(flee);

            var Misc = new Menu("Misc", "Misc");
            Misc.Add(new MenuList("qhit", "Q - HitChance :", new[] { "High", "Medium", "Low"}));
            Misc.Add(new MenuSlider("maxq", "Maximum Q Range", (int)Q.Range, 100, (int)Q.Range));
            Misc.Add(new MenuSlider("minq", "Minimum Q Range", 420, 100, (int)Q.Range));
            Misc.Add(new MenuSlider("grabhp", "Dont grab if below HP%", 0, 0, 100));
            Misc.Add(new MenuBool("int", "Interrupt"));
            mainMenu.Add(Misc);

            var keymenu = new Menu("Keys", "Keys");
            keymenu.Add(new MenuKeyBind("fleekey", "Flee [active]", Keys.A, KeyBindType.Press)).Permashow();
            keymenu.Add(new MenuKeyBind("grabkey", "Grab Assistant", Keys.G, KeyBindType.Press)).Permashow();
            mainMenu.Add(keymenu);

            var Draw = new Menu("Draw", "Drawing");
            Draw.Add(new MenuBool("qRange", "Q range"));
            Draw.Add(new MenuBool("rRange", "R range"));
            mainMenu.Add(Draw);

            mainMenu.Attach();

            GameEvent.OnGameTick += OnUpdate;
            Interrupter.OnInterrupterSpell += OnInterruptable;
            Drawing.OnDraw += OnDraw;
            AIBaseClient.OnProcessSpellCast += OnProcessSpellCast;

            Game.Print("Welcomme to ShinkyAio", Color.Aqua);
            Game.Print("Shinky Blitz loaded, GL & HF :)", Color.Crimson);
        }

        private void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            var hero = sender as AIHeroClient;
            if (hero != null && hero.IsEnemy && Q.IsReady() && mainMenu["Combo"].GetValue<MenuBool>("useqcombo").Enabled)
            {
                if (Player.HealthPercent < mainMenu["Misc"].GetValue<MenuSlider>("grabhp").Value)
                {
                    return;
                }

                if (hero.IsValidTarget(mainMenu["Misc"].GetValue<MenuSlider>("maxq").Value))
                {
                    if (!mainMenu["BLK"].GetValue<MenuBool>("blq" + hero.CharacterName).Enabled &&
                        mainMenu["AutoGrab"].GetValue<MenuBool>("auq" + hero.CharacterName).Enabled)
                    {
                        if (hero.Distance(Player.Position) > mainMenu["Misc"].GetValue<MenuSlider>("minq").Value)
                        {
                            if (mainMenu["AutoGrab"].GetValue<MenuBool>("autogcasting").Enabled)
                                Q.CastIfHitchanceEquals(hero, hitchanceQ());
                        }
                    }
                }
            }
        }

        private static HitChance hitchanceQ()
        {
            var hit = HitChance.High;
            switch (mainMenu["Misc"].GetValue<MenuList>("qhit").Index)
            {
                case 0:
                    hit = HitChance.High;
                    break;
                case 1:
                    hit = HitChance.Medium;
                    break;
                case 2:
                    hit = HitChance.Low;
                    break;
            }
            return hit;
        }

        private void OnDraw(EventArgs args)
        {
            if (mainMenu["Draw"].GetValue<MenuBool>("qRange").Enabled)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Color.LawnGreen : Color.Red, 2);
            }

            if (mainMenu["Draw"].GetValue<MenuBool>("rRange").Enabled)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? Color.LawnGreen : Color.Red, 2);
            }
        }

        private void OnInterruptable(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (sender.IsEnemy && sender.IsValidTarget() && mainMenu["Misc"].GetValue<MenuBool>("int").Enabled)
            {
                if (R.IsReady() && Player.Distance(sender.Position) <= R.Range)
                {
                    if (args.DangerLevel >= Interrupter.DangerLevel.High)
                    {
                        R.Cast();
                    }
                }

                if (Q.IsReady() && Player.Distance(sender.Position) <= mainMenu["Misc"].GetValue<MenuSlider>("maxq").Value)
                {
                    if (Player.HealthPercent < mainMenu["Misc"].GetValue<MenuSlider>("grabhp").Value)
                    {
                        return;
                    }

                    if (!mainMenu["BLK"].GetValue<MenuBool>("blq" + sender.CharacterName).Enabled &&
                        Player.Distance(sender.Position) > mainMenu["Misc"].GetValue<MenuSlider>("minq").Value)
                    {
                        Q.CastIfHitchanceEquals(sender, HitChance.Medium);
                    }
                }
            }
        }

        private void OnUpdate(EventArgs args)
        {
            if (Player.IsDead || !Orbwalker.CanMove())
            {
                return;
            }

            Grab(mainMenu["Keys"].GetValue<MenuKeyBind>("grabkey").Active);
            Flee(mainMenu["Keys"].GetValue<MenuKeyBind>("fleekey").Active);

            Combo(mainMenu["Combo"].GetValue<MenuBool>("useqcombo").Enabled, mainMenu["Combo"].GetValue<MenuBool>("useecombo").Enabled, 
                mainMenu["Combo"].GetValue<MenuBool>("usercombo").Enabled);

            foreach (var ene in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(mainMenu["Misc"].GetValue<MenuSlider>("maxq").Value)))
            {
                if (Player.HealthPercent < mainMenu["Misc"].GetValue<MenuSlider>("grabhp").Value)
                {
                    return;
                }

                if (!mainMenu["BLK"].GetValue<MenuBool>("blq" + ene.CharacterName).Enabled && mainMenu["AutoGrab"].GetValue<MenuBool>("auq" + ene.CharacterName).Enabled)
                {
                    if (ene.Distance(Player.Position) > mainMenu["Misc"].GetValue<MenuSlider>("minq").Value && Q.IsReady())
                    {

                        if (mainMenu["AutoGrab"].GetValue<MenuBool>("autogdashing").Enabled)
                            Q.CastIfHitchanceEquals(ene, HitChance.Dash);

                        if (mainMenu["AutoGrab"].GetValue<MenuBool>("autogimmobile").Enabled)
                            Q.CastIfHitchanceEquals(ene, HitChance.Immobile);
                    }
                }
            }
        }

        private void Flee(bool enable)
        {
            if (!enable)
            {
                return;
            }

            if (W.IsReady())
            {
                W.Cast();
            }

            var ene = GameObjects.EnemyHeroes.FirstOrDefault(x => x.Distance(Player.Position) <= Player.GetCurrentAutoAttackRange() + 100);
            if (E.IsReady() && ene.IsValidTarget())
            {
                E.Cast();
            }

            if (Player.HasBuff("powerfist") && Player.InAutoAttackRange(ene))
            {
                if (Variables.GameTimeTickCount - Limit >= 150 + Game.Ping)
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, ene);
                    Limit = Variables.GameTimeTickCount;
                }

                return;
            }

            Orbwalker.Orbwalk(null, Game.CursorPos);
        }

        private void Grab(bool enable)
        {
            if (Q.IsReady() && enable)
            {
                var QT = TargetSelector.GetTarget(mainMenu["Misc"].GetValue<MenuSlider>("maxq").Value, DamageType.Magical);
                if (QT != null && mainMenu["BLK"].GetValue<MenuBool>("blq" + QT.CharacterName).Enabled)
                {
                    return;
                }

                if (!(Player.HealthPercent < mainMenu["Misc"].GetValue<MenuSlider>("grabhp").Value))
                {
                    if (QT.IsValidTarget() && QT.Distance(Player.Position) > mainMenu["Misc"].GetValue<MenuSlider>("minq").Value)
                    {
                        if (!QT.IsZombie() && !QT.IsInvulnerable)
                        {
                            Q.CastIfHitchanceEquals(QT, hitchanceQ());
                        }
                    }
                }
            }
        }

        private void Combo(bool useq, bool usee, bool user)
        {
            if (Orbwalker.ActiveMode != OrbwalkerMode.Combo)
            {
                return;
            }

            if (useq && Q.IsReady())
            {
                var QT = TargetSelector.GetTarget(mainMenu["Misc"].GetValue<MenuSlider>("maxq").Value, DamageType.Magical);
                if (QT != null && mainMenu["BLK"].GetValue<MenuBool>("blq" + QT.CharacterName).Enabled)
                {
                    return;
                }

                if (!(Player.HealthPercent < mainMenu["Misc"].GetValue<MenuSlider>("grabhp").Value))
                {
                    if (QT.IsValidTarget() && QT.Distance(Player.Position) > mainMenu["Misc"].GetValue<MenuSlider>("minq").Value)
                    {
                        if (!QT.IsZombie() && !QT.IsInvulnerable)
                        {
                            Q.CastIfHitchanceEquals(QT, hitchanceQ());
                        }
                    }
                }
            }

            if (usee && E.IsReady())
            {
                var ET =
                    GameObjects.EnemyHeroes.FirstOrDefault(
                        x => x.HasBuff("rocketgrab2") || x.Distance(Player.Position) <= Player.GetRealAutoAttackRange() + 100);

                if (ET != null && ET.IsValidTarget())
                {
                    if (!ET.IsZombie() && !ET.IsInvulnerable)
                    {
                        E.Cast();
                    }
                }
            }

            if (user && R.IsReady())
            {
                var RT = TargetSelector.GetTarget(R.Range, DamageType.Magical);
                if (RT.IsValidTarget() && !RT.IsZombie())
                {
                    if (!RT.IsInvulnerable)
                    {
                        if (!E.IsReady() && RT.HasBuffOfType(BuffType.Knockup))
                        {
                            R.Cast();
                        }
                    }
                }
            }
        }
    }
}
