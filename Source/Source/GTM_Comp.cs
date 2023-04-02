using System;
using System.Collections.Generic;
using RimWorld;
using TurretExtensions;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;
using Verse.Sound;

namespace GTM
{
    public class GTM_Comp : ThingComp
    {
        public Comp_GTM_Base Props
        {
            get
            {
                return (Comp_GTM_Base)this.props;
            }
        }
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.refuelableComp = this.parent.GetComp<CompRefuelable>();
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo c in base.CompGetGizmosExtra())
            {
                yield return c;
            }
            if (this.parent.Faction == Faction.OfPlayer)
            {
                yield return new Command_Action
                {
                    defaultLabel = Translator.Translate("Burrow"),
                    defaultDesc = Translator.Translate("explainBurrow"),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/GTMburrow", true),
                    iconAngle = 0f,
                    iconOffset = Vector2.zero,
                    iconDrawScale = 1,//GenUI.IconDrawScale(this.parent.def),
                    action = delegate ()
                    {
                        this.burrowTurret();
                    }
                };
            }
            yield break;
        }
        private void burrowTurret()
        {
            SoundStarter.PlayOneShot(SoundDefOf.DropPod_Open, new TargetInfo(this.parent.Position, this.parent.Map, false));
            Map map = this.parent.Map;
            string name = this.parent.def.defName;
            IntVec3 loc = this.parent.Position;
            float HPp = (float)this.parent.HitPoints / (float)this.parent.MaxHitPoints;
            float FUEL = 0f;
            if (this.refuelableComp != null)
            {
                FUEL = this.refuelableComp.Fuel;
            }
            bool needStuff = true;
            ThingDef thatstuff = this.parent.Stuff;
            if (thatstuff == null)
            {
                thatstuff = ThingDefOf.Steel;
                needStuff = false;
            }
            bool UsualSize = (this.parent.def.size.x == this.parent.def.size.z);
            Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDef.Named("GTM_Hatch"), thatstuff), loc, map, 0);
            if (this.Props.Customhatch != "")
                thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDef.Named(this.Props.Customhatch), thatstuff), loc, map, 0);
            else if (UsualSize)
            {
                for (int i = this.Props.SizeList.Length; i >= 0; i--)
                {
                    if (this.parent.def.size.x > i)
                    {
                        thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDef.Named(this.Props.SizeList[i]), thatstuff), loc, map, 0);
                        break;
                    }
                }
            }
            // To do: Add your unusual-sized makelist here
            // Example:
            // if (!UsualSize && this.parent.def.size.x > 4 && this.parent.def.size.z > 6)
            //     thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDef.Named("GTM_Hatch_unusualHuge"), thatstuff), loc, map, 0);
            // End to do
            thing.SetFaction(Faction.OfPlayer, null);
            thing.HitPoints = (int)Math.Ceiling((double)((float)thing.MaxHitPoints * HPp));
            if (thing.HitPoints < thing.MaxHitPoints)
            {
                thing.Map.listerBuildingsRepairable.Notify_BuildingTookDamage((Building)thing);
            }
            ((GTM_Hatch)thing).insideman = name;
            if (this.refuelableComp != null)
            {
                ((GTM_Hatch)thing).insidefuel = FUEL;
            }
            ((GTM_Hatch)thing).insideStuff = needStuff;
            try
            {
                ((Action)delegate
                {
                    bool turretExtensionsIsActive = ModCompatibilityCheck.TurretExtensionsIsActive;
                    if (turretExtensionsIsActive)
                    {
                        CompUpgradable comp = this.parent.GetComp<CompUpgradable>();
                        if (comp != null)
                        {
                            bool upgraded = comp.upgraded;
                            if (upgraded)
                            {
                                ((GTM_Hatch)thing).upgradedbyturretextensions = true;
                                if (StatUtility.GetStatFactorFromList(comp.Props.statFactors, StatDefOf.MaxHitPoints) != 1f)
                                {
                                    ((GTM_Hatch)thing).TE_HP_Factor = StatUtility.GetStatFactorFromList(comp.Props.statFactors, StatDefOf.MaxHitPoints);
                                }
                                if (StatUtility.GetStatFactorFromList(comp.Props.statOffsets, StatDefOf.MaxHitPoints) > 0f)
                                {
                                    ((GTM_Hatch)thing).TE_HP_Offset = (int)StatUtility.GetStatFactorFromList(comp.Props.statOffsets, StatDefOf.MaxHitPoints);
                                }
                            }
                        }
                    }
                })();
            }
            catch (TypeLoadException ex)
            {
                Log.Message("error in burrowTurret XP", false);
            }
        }
        public CompRefuelable refuelableComp;
        public string insideman = "";
    }
}
