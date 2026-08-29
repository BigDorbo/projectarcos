using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.Sound;

namespace ProjectArcos
{
	public class CompAbilityEffect_SpawnHoard : CompAbilityEffect
	{
		private struct WaterCacheEntry
		{
			public int computedAtTick;
			public bool present;
		}

		private const int WaterRecheckIntervalTicks = 60000;
		private const int GateCacheTicks = 60;
		private const int PlacementRetryTicks = 2500;

		private static Dictionary<int, WaterCacheEntry> waterByMap = new Dictionary<int, WaterCacheEntry>();

		internal static void ClearSessionCache() => waterByMap.Clear();

		private TrainableDef reqTrainable;
		private bool reqLooked;
		private int gateEvalTick = -1;
		private bool gateOk;
		private string gateReason;

		public new CompProperties_AbilitySpawnHoard Props => (CompProperties_AbilitySpawnHoard)this.props;

		public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
		{
			string reason;
			return CanSpawnNow(this.parent.pawn, out reason) && base.Valid(target, throwMessages);
		}

		public override bool AICanTargetNow(LocalTargetInfo target)
		{
			string reason;
			return CanSpawnNow(this.parent.pawn, out reason);
		}

		public override bool GizmoDisabled(out string reason)
		{
			return !CanSpawnNow(this.parent.pawn, out reason);
		}

		public override bool ShouldHideGizmo
		{
			get
			{
				Pawn p = this.parent.pawn;
				TrainableDef req = ReqTrainable();
				return p != null && p.Faction != null && req != null && (p.training == null || !p.training.HasLearned(req));
			}
		}

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);

			Pawn p = this.parent.pawn;
			if (p == null || !p.Spawned || p.Map == null) return;
			if (Props.thingToSpawn == null) return;

			if (CapReached(p.Map))
			{
				RetrySoon();
				return;
			}

			ThingDef sd = Props.thingToSpawn;
			IntVec3 spot;
			if (!CellFinder.TryFindRandomCellNear(p.Position, p.Map, 3, c => ValidSpot(c, p.Map, sd), out spot))
			{
				RetrySoon();
				return;
			}

			Thing spawned = GenSpawn.Spawn(ThingMaker.MakeThing(sd, null), spot, p.Map, WipeMode.Vanish);
			CompLootBurst mark = spawned.TryGetComp<CompLootBurst>();
			if (mark != null) mark.owner = p;

			if (Props.dropRulePack != null)
				Find.BattleLog.Add(new BattleLogEntry_Event(p, Props.dropRulePack, null));

			if (Props.spawnSound != null)
				Props.spawnSound.PlayOneShot(SoundInfo.InMap(new TargetInfo(spot, p.Map, false), MaintenanceType.None));
		}

		private bool CanSpawnNow(Pawn p, out string reason)
		{
			int now = Find.TickManager.TicksGame;
			if (gateEvalTick >= 0 && now >= gateEvalTick && now - gateEvalTick < GateCacheTicks)
			{
				reason = gateReason;
				return gateOk;
			}

			gateOk = EvaluateGate(p, out gateReason);
			gateEvalTick = now;
			reason = gateReason;
			return gateOk;
		}

		private bool EvaluateGate(Pawn p, out string reason)
		{
			reason = null;
			if (p == null || !p.Spawned || p.Map == null || Props.thingToSpawn == null) return false;

			TrainableDef req = ReqTrainable();
			if (p.Faction != null && req != null && (p.training == null || !p.training.HasLearned(req)))
			{
				reason = "Not trained.";
				return false;
			}

			if (Props.requiresMapWater && !WaterPresent(p.Map))
			{
				reason = "No water on this map.";
				return false;
			}

			if (CapReached(p.Map))
			{
				reason = "Hoard limit reached.";
				return false;
			}

			ThingDef sd = Props.thingToSpawn;
			IntVec3 spot;
			if (!CellFinder.TryFindRandomCellNear(p.Position, p.Map, 3, c => ValidSpot(c, p.Map, sd), out spot))
			{
				reason = "No clear ground nearby.";
				return false;
			}

			return true;
		}

		private static bool WaterPresent(Map map)
		{
			int now = Find.TickManager.TicksGame;
			WaterCacheEntry e;
			if (waterByMap.TryGetValue(map.uniqueID, out e) && now >= e.computedAtTick && now - e.computedAtTick < WaterRecheckIntervalTicks) return e.present;

			bool present = false;
			TerrainDef[] grid = map.terrainGrid.topGrid;
			for (int i = 0; i < grid.Length; i++)
				if (grid[i] != null && grid[i].IsWater)
				{
					present = true;
					break;
				}

			waterByMap[map.uniqueID] = new WaterCacheEntry { computedAtTick = now, present = present };

			return present;
		}

		private static bool ValidSpot(IntVec3 rc, Map map, ThingDef sd)
		{
			CellRect rect = GenAdj.OccupiedRect(rc, Rot4.North, sd.size);
			foreach (IntVec3 c in rect.ExpandedBy(1))
			{
				bool inRect = rect.Contains(c);
				if (!c.InBounds(map))
				{
					if (inRect) return false;
					continue;
				}
				TerrainDef t = c.GetTerrain(map);
				if (t != null && t.IsWater) return false;
				if (inRect && (!c.Standable(map) || c.GetEdifice(map) != null)) return false;
			}

			return true;
		}

		private TrainableDef ReqTrainable()
		{
			if (!reqLooked)
			{
				reqTrainable = (Props.requiredTrainable == null) ? null : DefDatabase<TrainableDef>.GetNamedSilentFail(Props.requiredTrainable);
				reqLooked = true;
			}

			return reqTrainable;
		}

		private void RetrySoon()
		{
			this.parent.ResetCooldown();
			this.parent.StartCooldown(PlacementRetryTicks);
		}

		private bool CapReached(Map map) => map.listerThings.ThingsOfDef(Props.thingToSpawn).Count >= Props.maxCount;
	}
}
