using Verse;

namespace ProjectArcos
{
	public class PA_GameComponent : GameComponent
	{
		public PA_GameComponent(Game game)
		{
		}

		public override void FinalizeInit()
		{
			base.FinalizeInit();
			JobGiver_AnimalSpar.ClearSessionCache();
			CompAbilityEffect_SpawnHoard.ClearSessionCache();
			CompLootBurst.ClearSessionCache();
		}
	}
}
