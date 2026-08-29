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

			Ability ab = EnsureAbility(p);
			if (ab == null) return;

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

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (!DebugSettings.ShowDevGizmos || Props.ability == null) yield break;

			Pawn p = this.parent as Pawn;
			if (p == null || !p.Spawned) yield break;

			yield return new Command_Action
			{
				defaultLabel = "DEV: Cast " + Props.ability.label,
				action = delegate
				{
					Ability ab = EnsureAbility(p);
					if (ab == null) return;

					primed = true;
					ab.Activate(p, p);
				}
			};

			yield return new Command_Action
			{
				defaultLabel = "DEV: Reset hoard cooldown",
				action = delegate
				{
					Ability ab = EnsureAbility(p);
					if (ab == null) return;

					primed = true;
					ab.ResetCooldown();
				}
			};
		}

		private Ability EnsureAbility(Pawn p)
		{
			if (p.abilities == null) p.abilities = new Pawn_AbilityTracker(p);

			Ability ab = p.abilities.GetAbility(Props.ability, false);
			if (ab == null)
			{
				p.abilities.GainAbility(Props.ability);
				ab = p.abilities.GetAbility(Props.ability, false);
			}

			return ab;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref primed, "primed", false);
		}
	}
}
