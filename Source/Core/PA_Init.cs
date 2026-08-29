using RimWorld;
using Verse;

namespace ProjectArcos
{
	[StaticConstructorOnStartup]
	public static class PA_Init
	{
		static PA_Init()
		{
			Require<ThingDef>("PA_Loraptora", "PA_Nightgorger", "PA_MudlurkerCache", "PA_NightgorgerPellet");
			Require<RulePackDef>("PA_Event_CacheDrop", "PA_Event_PelletDrop");
			Require<ThoughtDef>("PA_PlunderedCache", "PA_OpenedPellet");
			Require<TrainableDef>("PA_PelletCraft");
		}

		public static bool Spars(Pawn p) => p.def == PA_DefOf.PA_Clubtail;

		private static void Require<T>(params string[] names) where T : Def
		{
			for (int i = 0; i < names.Length; i++)
				if (DefDatabase<T>.GetNamedSilentFail(names[i]) == null)
					Log.Error("[Project Arcos] required def missing: " + names[i] + " (files out of sync, re-extract the mod)");
		}
	}
}
