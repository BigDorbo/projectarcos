using RimWorld;
using Verse;

namespace ProjectArcos
{
	public class CompProperties_HoardCaster : CompProperties
	{
		public AbilityDef ability;
		public IntRange initialCooldownRange;
		public int checkInterval = 250;

		public CompProperties_HoardCaster()
		{
			this.compClass = typeof(CompHoardCaster);
		}
	}
}
