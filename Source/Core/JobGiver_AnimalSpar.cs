using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ProjectArcos
{
	public class JobGiver_AnimalSpar : ThinkNode_JobGiver
	{
		private const int TryIntervalTicks = 2500;

		private static Dictionary<int, int> nextTryTick = new Dictionary<int, int>();

		internal static void ClearSessionCache() => nextTryTick.Clear();

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!PA_Init.Spars(pawn)) return null;
			if (PA_DefOf.PA_Spar == null) return null;
			if (!pawn.ageTracker.CurLifeStage.reproductive) return null;

			int now = Find.TickManager.TicksGame;
			int next;
			if (nextTryTick.TryGetValue(pawn.thingIDNumber, out next) && now < next) return null;
			nextTryTick[pawn.thingIDNumber] = now + TryIntervalTicks;

			if (!Rand.Chance(0.15f)) return null;
			if (pawn.health.hediffSet.BleedRateTotal > 0f) return null;
			if (pawn.needs == null || pawn.needs.food == null || pawn.needs.food.CurLevelPercentage < 0.5f) return null;

			Pawn best = FindPartner(pawn, null);
			if (best == null) return null;

			if (!pawn.CanReach(best, PathEndMode.Touch, Danger.None))
			{
				best = FindPartner(pawn, best);
				if (best == null || !pawn.CanReach(best, PathEndMode.Touch, Danger.None)) return null;
			}

			return JobMaker.MakeJob(PA_DefOf.PA_Spar, best);
		}

		private static Pawn FindPartner(Pawn pawn, Pawn exclude)
		{
			float bestDistSq = 144f;
			Pawn best = null;
			ScanForPartner(pawn, pawn.Map.mapPawns.SpawnedPawnsInFaction(null), exclude, ref best, ref bestDistSq);
			if (pawn.Faction != null)
				ScanForPartner(pawn, pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction), exclude, ref best, ref bestDistSq);

			return best;
		}

		private static void ScanForPartner(Pawn pawn, List<Pawn> candidates, Pawn exclude, ref Pawn best, ref float bestDistSq)
		{
			for (int i = 0; i < candidates.Count; i++)
			{
				Pawn o = candidates[i];
				if (o == pawn || o == exclude || o.def != pawn.def) continue;

				float distSq = (o.Position - pawn.Position).LengthHorizontalSquared;
				if (distSq > bestDistSq) continue;

				if (o.Dead || o.Downed || o.InMentalState || !o.Awake()) continue;
				if (!o.ageTracker.CurLifeStage.reproductive) continue;
				if (o.CurJobDef == PA_DefOf.PA_Spar || o.CurJobDef == JobDefOf.Ingest) continue;
				if (o.health.hediffSet.BleedRateTotal > 0f) continue;
				if (o.needs == null || o.needs.food == null || o.needs.food.CurLevelPercentage < 0.5f) continue;

				best = o;
				bestDistSq = distSq;
			}
		}
	}
}
