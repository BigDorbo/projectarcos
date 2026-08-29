using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ProjectArcos
{
	public static class PA_Memories
	{
		public static void Witness(Pawn subject, ThoughtDef def)
		{
			if (subject == null || def == null) return;

			Map map = subject.MapHeld;
			if (map == null) return;

			List<Pawn> colonists = map.mapPawns.FreeColonistsSpawned;
			for (int i = 0; i < colonists.Count; i++)
			{
				Pawn c = colonists[i];
				if (c.needs == null || c.needs.mood == null || c.needs.mood.thoughts == null) continue;
				if (c.Downed || !c.Awake()) continue;
				if (PawnUtility.IsBiologicallyOrArtificiallyBlind(c)) continue;
				if (!c.Position.InHorDistOf(subject.Position, 12f)) continue;
				if (!GenSight.LineOfSight(c.Position, subject.Position, map, true)) continue;

				c.needs.mood.thoughts.memories.TryGainMemory(def);
			}
		}

		public static void Give(Pawn p, ThoughtDef def)
		{
			if (p == null || def == null || !p.IsColonist) return;
			if (p.needs == null || p.needs.mood == null) return;

			p.needs.mood.thoughts.memories.TryGainMemory(def);
		}
	}
}
