using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ProjectArcos
{
	public class CompHoardCaster : ThingComp
	{
		private bool primed;

		public CompProperties_HoardCaster Props => (CompProperties_HoardCaster)this.props;

		public override void CompTickInterval(int delta)
		{
			if (!this.parent.IsHashIntervalTick(Props.checkInterval, delta)) return;
			if (Props.ability == null) return;

			Pawn p = this.parent as Pawn;
			if (p == null || !p.Spawned || p.Map == null || p.Dead || p.Downed) return;

			Pawn_AbilityTracker tracker = p.abilities;
			if (tracker == null) return;

			Ability ab = tracker.GetAbility(Props.ability, false);
			if (ab == null)
			{
				tracker.GainAbility(Props.ability);
				ab = tracker.GetAbility(Props.ability, false);
				if (ab == null) return;
			}

			if (!primed)
			{
				primed = true;
				ab.StartCooldown(Props.initialCooldownRange.RandomInRange);
				return;
			}

			if (!ab.CanCast) return;
			if (p.CurJob != null && p.CurJob.ability == ab) return;
			if (p.CurJobDef == JobDefOf.Ingest) return;
			if (p.InMentalState || p.GetLord() != null) return;

			List<CompAbilityEffect> effects = ab.EffectComps;
			for (int i = 0; i < effects.Count; i++)
				if (!effects[i].AICanTargetNow(p)) return;

			p.jobs.StartJob(ab.GetJob(p, p), JobCondition.InterruptForced);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref primed, "primed", false);
		}
	}
}
