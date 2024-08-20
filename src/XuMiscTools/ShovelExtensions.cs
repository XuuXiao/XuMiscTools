using System;

namespace XuMiscTools;

public static class ShovelExtensions {
	public static int CriticalHit(int force, System.Random random, float critChance) {
		if (((float)random.NextDouble() * 100) < Math.Clamp(critChance, 0, 100)) {
            //Plugin.Log.LogInfo("Critical Hit!");
            return force * 2;
        }
        return force;
	}
}