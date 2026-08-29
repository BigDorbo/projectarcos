using RimWorld;
using Verse;

namespace ProjectArcos
{
	public class CompProperties_AbilitySpawnHoard : CompProperties_AbilityEffect
	{
		public ThingDef thingToSpawn;
		public int maxCount;
		public bool requiresMapWater;
		public SoundDef spawnSound;
		public RulePackDef dropRulePack;
		public string requiredTrainable;

		public CompProperties_AbilitySpawnHoard()
		{
			this.compClass = typeof(CompAbilityEffect_SpawnHoard);
		}
	}
}
