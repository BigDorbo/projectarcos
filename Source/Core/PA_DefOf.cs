using RimWorld;
using Verse;

namespace ProjectArcos
{
	[DefOf]
	public static class PA_DefOf
	{
		public static JobDef PA_Spar;
		public static RulePackDef PA_Event_Spar;
		public static ThoughtDef PA_WatchedSpar;
		public static ThingDef PA_Clubtail;

		[MayRequireAnyOf("Ludeon.RimWorld.Odyssey,VanillaExpanded.VCEF")]
		public static TrainableDef PA_CacheCraft;

		static PA_DefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PA_DefOf));
		}
	}
}
