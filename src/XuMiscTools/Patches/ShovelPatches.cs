using System.Collections.Generic;
using GameNetcodeStuff;

namespace XuMiscTools.Patches;
static class ShovelPatch {
	public static System.Random? random;
	
	public static void Init() {
		On.Shovel.HitShovel += Shovel_HitShovel;
    }

    private static void Shovel_HitShovel(On.Shovel.orig_HitShovel orig, Shovel self, bool cancel)
    {
        if (random == null) {
			if (StartOfRound.Instance != null) {
				random = new System.Random(StartOfRound.Instance.randomMapSeed + 85);
			} else {
				random = new System.Random(69);
			}
		}
		if (self is BetterShovel betterShovel) {
			betterShovel.defaultForce = betterShovel.shovelHitForce;
			if (betterShovel.CritPossible) {
				betterShovel.shovelHitForce = ShovelExtensions.CriticalHit(betterShovel.shovelHitForce, random, betterShovel.CritChance);
			}
		}

		orig(self, cancel);

		if (self is BetterShovel betterShovelPost) {
			betterShovelPost.shovelHitForce = betterShovelPost.defaultForce;	
			if (betterShovelPost.CanBreakTrees) {
				//Plugin.Log.LogInfo("Tree Destroyed: " + betterShovelPost.weaponTip.position);
				RoundManager.Instance.DestroyTreeOnLocalClient(betterShovelPost.WeaponTip.position);
			}
		}
    }
}