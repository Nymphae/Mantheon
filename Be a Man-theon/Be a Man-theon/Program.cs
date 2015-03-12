﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
﻿using System;
using System.Drawing.Text;
using System.Linq;
using System.Threading;
using LeagueSharp;
using SharpDX;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using Microsoft.Win32.SafeHandles;
using Color = System.Drawing.Color;



namespace Be_A_Man_theon
{
    internal class Program
    {

        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        private const String championName = "Pantheon";
        private static Menu _Menu;
        private static Orbwalking.Orbwalker _orbwalker;
        private static Menu TSMenu;
        private static Spell Q, W, E, R;
        private static bool usingE = false;


        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            {
                if (Player.ChampionName != championName)

                    return;
            }

            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R);
          

            _Menu = new Menu("Mantheon", "pan.mainmenu", true);
            TSMenu = new Menu("Target Selector", "target.selector");
            TargetSelector.AddToMenu(TSMenu);
            _Menu.AddSubMenu(TSMenu);
            _orbwalker = new Orbwalking.Orbwalker(_Menu.SubMenu("Orbwalking"));
            var comboMenu = new Menu("Combo", "kek.pan.combo");
            {
                comboMenu.AddItem(new MenuItem("kek.pan.combo.useq", "Use Q").SetValue(true));
                comboMenu.AddItem(new MenuItem("kek.pan.combo.usew", "Use W").SetValue(true));
                comboMenu.AddItem(new MenuItem("kek.pan.combo.usee", "Use E").SetValue(true));

            }

            var farmMenu = new Menu("Jungle/Lane Clear", "kek.pan.farm");
            {
                farmMenu.AddItem(new MenuItem("kek.pan.farm.farmq", "Use Q to Farm").SetValue(true));
                farmMenu.AddItem(new MenuItem("kek.pan.farm.farme", "use E to farm").SetValue(true));
                farmMenu.AddItem(new MenuItem("kek.pan.farm.farmw", "Use W to Farm").SetValue(true));
            }

            var pokeMenu = new Menu("Lane Harass", "kek.pan.harass");
            {
                pokeMenu.AddItem(new MenuItem("kek.pan.poke.pokeq", "Poke Enemy with Q").SetValue(true));
                pokeMenu.AddItem(
                    new MenuItem("kek.pan.poke.pokemanager", "% Mana Management").SetValue(new Slider(1, 1, 100)));
            }

            var ksMenu = new Menu("Kill Secure", "kek.pan.ks");
            {
                ksMenu.AddItem(new MenuItem("kek.pan.ks.Q", "Use Q to KS").SetValue(true));
                ksMenu.AddItem(new MenuItem("kek.pan.ks.WQ", "Use WQ to KS").SetValue(true));
                ksMenu.AddItem(new MenuItem("kek.pan.ks.Ecancel", "Cancel E to KS").SetValue(true));

            }

            var drawMenu = new Menu("Draw Settings", "kek.pan.draw");
            {
                drawMenu.AddItem(new MenuItem("kek.pan.drawErange", "Range Draw").SetValue(true));


            }

            _Menu.AddSubMenu(pokeMenu);
            _Menu.AddSubMenu(ksMenu);
            _Menu.AddSubMenu(drawMenu);
            _Menu.AddSubMenu(farmMenu);
            _Menu.AddSubMenu(comboMenu);
            _Menu.AddToMainMenu();
            Game.OnUpdate += Game_OnGameUpdate;
            LeagueSharp.Drawing.OnDraw += Drawing;
            Game.PrintChat("Mantheon Loaded. Be A Fucking Man.");


        }

        private static void Drawing(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }
            if (_Menu.Item("kek.pan.drawErange").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, Color.DarkCyan);
            }

        }


        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
            }

            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                JungleClear();
            }

            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                Harass();
            }
        }


        private static void JungleClear()
        {


            if (Player.HasBuff("HeartSeeker Strike"))
            {
                usingE = true;
            }

            var creeps = MinionManager.GetMinions(
                Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (creeps.Count > 0)
            {
                var minions = creeps[0];
                if (_Menu.Item("kek.pan.farm.farme").GetValue<bool>() && E.IsReady() && minions.IsValidTarget(E.Range))
                {
                    E.Cast(minions);
                }

                if (_Menu.Item("kek.pan.farm.farmw").GetValue<bool>() && W.IsReady() && minions.IsValidTarget(W.Range) &&
                    !usingE)
                {
                    W.Cast();
                }

                if (_Menu.Item("kek.pan.farm.farmq").GetValue<bool>() && minions.IsValidTarget(Q.Range) && !usingE)
                {
                    Q.Cast();
                }

            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Physical);
            var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);

            if (Player.HasBuff("HeartSeeker"))
            {
                usingE = true;
            }


           if (target != null && _Menu.Item("kek.pan.combo.usew").GetValue<bool>() && W.IsReady() && !usingE)
            {
                W.Cast(target);
            }

            if (target != null && _Menu.Item("kek.pan.combo.usee").GetValue<bool>() && E.IsReady())
            {
                {
                    _orbwalker.SetMovement(false);
                    _orbwalker.SetAttack(false);
                    E.Cast(target);
                }
            }

            if (target != null && _Menu.Item("kek.pan.combo.useq").GetValue<bool>() && Q.IsReady() && !usingE)
            {
                Q.Cast(target);
            }

            if (_Menu.Item("kek.pan.ks.Ecancel").GetValue<bool>())
            {
                if (_Menu.Item("kek.pan.ks.Q").GetValue<bool>())
                {
                    KillstealQ();
                }

                if (_Menu.Item("kek.pan.ks.WQ").GetValue<bool>())
                {
                    KillstealW();
                }
            }
            else
            {
                if (_Menu.Item("kek.pan.ks.Q").GetValue<bool>() && !usingE)
                {
                    KillstealQ();
                }

                if (_Menu.Item("kek.pan.ks.WQ").GetValue<bool>() && !usingE)
                {
                    KillstealW();
                }
            }

        }


        private static void Harass()
        {

            float manaPercent = (Player.Mana / Player.MaxMana) * 100;
            int setMana = _Menu.Item("kek.pan.poke.pokemanager").GetValue<Slider>().Value;
            var target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Physical);

            if (target != null && _Menu.Item("kek.pan.poke.pokeq").GetValue<bool>() && Q.IsReady() && manaPercent > setMana)
            {
                Q.Cast(target);
            }

            if (target != null && _Menu.Item("kek.pan.poke.pokee").GetValue<bool>() && E.IsReady() && manaPercent > setMana)
            {
                E.Cast(target);
            }
        }

        private static void KillstealW()
        {
            var target = TargetSelector.GetTarget(900, TargetSelector.DamageType.Physical);
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(E.Range)))
            {
                double DamageW = Player.GetSpellDamage(hero, SpellSlot.W);
                double DamageQ = Player.GetSpellDamage(hero, SpellSlot.Q);
                double DamageTotal = DamageQ + DamageW;
                if (W.IsReady() && hero.Distance(ObjectManager.Player) <= E.Range && DamageTotal >= hero.Health)
                    W.Cast(target);
                Q.Cast(target);
            }


        }

        private static void KillstealQ()
        {
            var target = TargetSelector.GetTarget(900, TargetSelector.DamageType.Physical);
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(E.Range)))
            {
                double DamageQ = Player.GetSpellDamage(hero, SpellSlot.Q);
                if (Q.IsReady() && hero.Distance(ObjectManager.Player) <= Q.Range && DamageQ >= hero.Health)
                {
                    Q.Cast(target);
                }
            }
        }

    }
}