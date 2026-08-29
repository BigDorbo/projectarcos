using System.Collections.Generic;
using RimWorld;
using Verse;

namespace ProjectArcos
{
	public class CompLootBurst : ThingComp
	{
		private static Dictionary<CompProperties, List<ThingDef>> pools = new Dictionary<CompProperties, List<ThingDef>>();

		internal static void ClearSessionCache() => pools.Clear();

		public Pawn owner;
		private Pawn lastAttacker;

		public CompProperties_LootBurst Props => (CompProperties_LootBurst)this.props;

		public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			base.PostPostApplyDamage(dinfo, totalDamageDealt);

			Pawn p = dinfo.Instigator as Pawn;
			if (p != null) lastAttacker = p;
		}

		public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
		{
			base.Notify_Killed(prevMap, dinfo);

			if (prevMap == null) return;

			Pawn credited = null;
			if (dinfo.HasValue) credited = dinfo.Value.Instigator as Pawn;
			if (credited == null) credited = lastAttacker;

			if (credited != null && owner != null && owner != credited && !owner.Dead && owner.Spawned && owner.Map == prevMap && Props.angerOwner && Rand.Chance(Props.angerChance))
			{
				bool started = owner.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);
				if (started && Props.angerLetterLabel != null)
					Find.LetterStack.ReceiveLetter(Props.angerLetterLabel, Props.angerLetterText, LetterDefOf.ThreatSmall, owner);
			}

			PA_Memories.Give(credited, Props.openedThought);

			if (Props.guaranteedDefName != null && Props.guaranteedCount > 0)
			{
				Thing bonus = ThingMaker.MakeThing(Props.guaranteedDefName, null);
				bonus.stackCount = Props.guaranteedCount;

				GenPlace.TryPlaceThing(bonus, this.parent.Position, prevMap, ThingPlaceMode.Near, null, null, default(Rot4));
			}

			List<ThingDef> loot = Pool();
			if (loot.Count > 0)
			{
				Thing prize = ThingMaker.MakeThing(loot[Rand.Range(0, loot.Count)], null);
				prize.stackCount = Props.countRange.RandomInRange;

				GenPlace.TryPlaceThing(prize, this.parent.Position, prevMap, ThingPlaceMode.Near, null, null, default(Rot4));
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();

			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (owner != null && owner.Destroyed) owner = null;
				if (lastAttacker != null && lastAttacker.Destroyed) lastAttacker = null;
			}

			Scribe_References.Look(ref owner, "owner", false);
			Scribe_References.Look(ref lastAttacker, "lastAttacker", false);
		}

		private List<ThingDef> Pool()
		{
			List<ThingDef> pool;
			if (pools.TryGetValue(this.props, out pool)) return pool;

			pool = new List<ThingDef>();
			if (Props.lootDefNames != null)
				for (int i = 0; i < Props.lootDefNames.Count; i++)
				{
					ThingDef d = DefDatabase<ThingDef>.GetNamedSilentFail(Props.lootDefNames[i]);
					if (d != null) pool.Add(d);
				}

			if (Props.lootCategory != null)
			{
				List<ThingDef> all = DefDatabase<ThingDef>.AllDefsListForReading;
				for (int i = 0; i < all.Count; i++)
				{
					ThingDef d = all[i];
					if (d.thingCategories == null || !d.thingCategories.Contains(Props.lootCategory)) continue;
					if (Props.excludeDefNames != null && Props.excludeDefNames.Contains(d.defName)) continue;

					pool.Add(d);
				}
			}

			pools[this.props] = pool;

			return pool;
		}
	}
}
