using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using TurretExtensions;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace GTM
{
    internal class GTM_Hatch : Building
    {
        public bool CanUnburrowNow
        {
            get
            {
                return (!base.Spawned || !base.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare)) && this.powerComp.PowerOn && this.insideman != "";
            }
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<string>(ref this.insideman, "insideman", "", false);
            Scribe_Values.Look<float>(ref this.insidefuel, "insidefuel", 0f, false);
            Scribe_Values.Look<bool>(ref this.insideStuff, "insideStuff", false, false);
            Scribe_Values.Look<string>(ref this.customxpath, "customxpath", "", false);
            Scribe_Values.Look<bool>(ref this.upgradedbyturretextensions, "upgradedbyturretextensions", false, false);
            Scribe_Values.Look<float>(ref this.TE_HP_Factor, "TE_HP_Factor", 1f, false);
            Scribe_Values.Look<int>(ref this.TE_HP_Offset, "TE_HP_Offset", 0, false);
        }
        public override void Draw()
        {
            if (this.customxpath != "")
            {
                if (this.customimg == null)
                {
                    this.customimg = MaterialPool.MatFrom(this.customxpath);
                }
                Mesh mesh = MeshPool.GridPlane(this.def.graphicData.drawSize);
                Graphics.DrawMesh(mesh, this.DrawPos, Quaternion.identity, this.customimg, 0);
            }
            else
            {
                base.Draw();
            }
        }
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            string newDesc = "\n" + Translator.Translate("Inside") + " : " + ThingDef.Named(this.insideman).label;
            stringBuilder.Append(newDesc);
            return stringBuilder.ToString();
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo c in base.GetGizmos())
            {
                yield return c;
            }
            bool canUnburrowNow = this.CanUnburrowNow;
            if (canUnburrowNow)
            {
                yield return new Command_Action
                {
                    defaultLabel = Translator.Translate("Unburrow"),
                    defaultDesc = Translator.Translate("explainUnburrow"),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/GTMunburrow", true),
                    iconAngle = 0f,
                    iconOffset = Vector2.zero,
                    iconDrawScale = 1,//GenUI.IconDrawScale(this.def),
                    action = delegate ()
                    {
                        this.UnburrowTurret();
                    }
                };
            }
            yield break;
        }
        private void UnburrowTurret()
        {
            SoundStarter.PlayOneShot(SoundDefOf.DropPod_Open, new TargetInfo(base.Position, base.Map, false));
            Map map = base.Map;
            IntVec3 loc = base.Position;
            float HPp = (float)this.HitPoints / (float)base.MaxHitPoints;
            Thing thing;
            if (this.insideStuff)
                thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDef.Named(this.insideman), base.Stuff), loc, map, 0);
            else
                thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDef.Named(this.insideman), null), loc, map, 0);
            thing.SetFaction(Faction.OfPlayer, null);
            thing.HitPoints = (int)Math.Ceiling((double)((float)thing.MaxHitPoints * HPp));
            if (thing.HitPoints < thing.MaxHitPoints)
            {
                thing.Map.listerBuildingsRepairable.Notify_BuildingTookDamage((Building)thing);
            }
            if (this.insidefuel >= 0f)
            {
                CompRefuelable refuelableComp = ((ThingWithComps)thing).GetComp<CompRefuelable>();
                refuelableComp.ConsumeFuel(9999f);
                refuelableComp.Refuel(this.insidefuel / refuelableComp.Props.FuelMultiplierCurrentDifficulty);
            }
            try
            {
                ((Action)delegate
                {
                    if (this.upgradedbyturretextensions)
                    {
                        CompUpgradable compUP = ((ThingWithComps)thing).GetComp<CompUpgradable>();
                        thing.HitPoints = (int)Math.Ceiling((double)((float)(thing.MaxHitPoints + this.TE_HP_Offset) * this.TE_HP_Factor * HPp));
                        compUP.upgraded = true;
                    }
                })();
            }
            catch (TypeLoadException ex)
            {
                Log.Message("error in unburrowTurret XP", false);
            }
        }
        public CompPowerTrader powerComp;
        public string insideman = "";
        public float insidefuel = -1f;
        public bool insideStuff = false;
        public string customxpath = "";
        public Material customimg = null;
        public bool upgradedbyturretextensions;
        public float TE_HP_Factor = 1f;
        public int TE_HP_Offset = 0;
    }
}
