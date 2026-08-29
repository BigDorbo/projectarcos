using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ProjectArcos
{
	public class CompProperties_LootBurst : CompProperties
	{
		public List<string> lootDefNames;
		public ThingCategoryDef lootCategory;
		public ThingDef guaranteedDefName;
		public int guaranteedCount;
		public List<string> excludeDefNames;
		public IntRange countRange;
		public float angerChance;
		public string angerLetterLabel;
		public string angerLetterText;
		public ThoughtDef openedThought;
		public bool angerOwner;

		public CompProperties_LootBurst()
		{
			this.compClass = typeof(CompLootBurst);
		}
	}
}
