using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace ProjectArcos
{
	public class JobDriver_Spar : JobDriver
	{
		private int myStart = -1;
		private int theirStart = -1;
		private int nextSwing;
		private int endTick;

		private Pawn Partner => this.job.GetTarget(TargetIndex.A).Thing as Pawn;

		public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOn(() => Partner.Dead || Partner.Downed || Partner.InMentalState);

			Toil recruit = ToilMaker.MakeToil("SparRecruit");
			recruit.initAction = delegate
			{
				Pawn p2 = Partner;
				if (p2.CurJobDef != PA_DefOf.PA_Spar)
					p2.jobs.StartJob(JobMaker.MakeJob(PA_DefOf.PA_Spar, pawn), JobCondition.InterruptForced);
			};
			recruit.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return recruit;

			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

			Toil bout = ToilMaker.MakeToil("SparBout");
			bout.initAction = delegate
			{
				myStart = pawn.health.hediffSet.hediffs.Count;
				theirStart = Partner.health.hediffSet.hediffs.Count;
				nextSwing = Find.TickManager.TicksGame + 60;
				endTick = Find.TickManager.TicksGame + 3000;
			};
			bout.tickAction = delegate
			{
				Pawn p2 = Partner;
				if (pawn.health.hediffSet.hediffs.Count > myStart || p2.health.hediffSet.hediffs.Count > theirStart)
				{
					EndPartnerSpar(p2, JobCondition.Succeeded);
					EndJobWith(JobCondition.Succeeded);
					return;
				}

				if (Find.TickManager.TicksGame > endTick || !pawn.Position.InHorDistOf(p2.Position, 2f))
				{
					EndPartnerSpar(p2, JobCondition.Incompletable);
					EndJobWith(JobCondition.Incompletable);
					return;
				}

				if (Find.TickManager.TicksGame < nextSwing) return;

				nextSwing = Find.TickManager.TicksGame + 120;
				pawn.rotationTracker.FaceTarget(p2);
				if (Rand.Chance(0.5f))
				{
					p2.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 6f, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, false));

					if (p2.health.hediffSet.hediffs.Count > theirStart)
					{
						if (PA_DefOf.PA_Event_Spar != null)
							Find.BattleLog.Add(new BattleLogEntry_Event(p2, PA_DefOf.PA_Event_Spar, pawn));
						PA_Memories.Witness(pawn, PA_DefOf.PA_WatchedSpar);
					}

					LifeStageUtility.PlayNearestLifestageSound(pawn, ls => ls.soundAngry, null, null, 1f);
					if (Rand.Chance(0.5f))
						LifeStageUtility.PlayNearestLifestageSound(p2, ls => ls.soundAngry, null, null, 1f);
				}
			};
			bout.defaultCompleteMode = ToilCompleteMode.Never;

			yield return bout;
		}

		private void EndPartnerSpar(Pawn p2, JobCondition condition)
		{
			if (p2 == null || p2.Dead || p2.jobs == null) return;
			if (p2.CurJobDef != PA_DefOf.PA_Spar) return;
			if (p2.CurJob == null || p2.CurJob.GetTarget(TargetIndex.A).Thing != pawn) return;
			if (p2.jobs.curDriver != null && p2.jobs.curDriver.ended) return;

			p2.jobs.EndCurrentJob(condition, true, true);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref myStart, "myStart", -1);
			Scribe_Values.Look(ref theirStart, "theirStart", -1);
			Scribe_Values.Look(ref nextSwing, "nextSwing", 0);
			Scribe_Values.Look(ref endTick, "endTick", 0);
		}
	}
}
