using System;
using System.Collections.Generic;
using GameNetcodeStuff;
using UnityEngine;

namespace XuMiscTools.Patches;
static class EnemyAIPatches
{
    public static Dictionary<EnemyAI, float> BetterShovelDamageDealtOnEachEnemy = new();

    public static void Init()
    {
        On.EnemyAI.HitEnemyOnLocalClient += EnemyAI_HitEnemyOnLocalClient;
        On.EnemyAI.HitEnemyClientRpc += EnemyAI_HitEnemyClientRpc;
        On.EnemyAI.Start += EnemyAI_Start;
    }

    private static void EnemyAI_HitEnemyClientRpc(On.EnemyAI.orig_HitEnemyClientRpc orig, EnemyAI self, int force, int playerWhoHit, bool playHitSFX, int hitID)
    {
        PlayerControllerB playerWhoHita = StartOfRound.Instance.allPlayerScripts[(int)playerWhoHit];
        Plugin.Log.LogInfo($"[BetterShovel] HitEnemyClientRpc: playerWhoHita: {playerWhoHita}");
        if (playerWhoHita != null && playerWhoHita.currentlyHeldObjectServer != null && playerWhoHita.currentlyHeldObjectServer.TryGetComponent<BetterShovel>(out BetterShovel betterShovel)) 
        {
            BetterShovelDamageDealtOnEachEnemy[self] += betterShovel.ExperimentalShovelHitForce;
            force = Mathf.FloorToInt(BetterShovelDamageDealtOnEachEnemy[self]);
            BetterShovelDamageDealtOnEachEnemy[self] -= force;
            orig(self, force, playerWhoHit, playHitSFX, hitID);
        } 
        else 
        {
            orig(self, force, playerWhoHit, playHitSFX, hitID);
        }
    }

    private static void EnemyAI_HitEnemyOnLocalClient(On.EnemyAI.orig_HitEnemyOnLocalClient orig, EnemyAI self, int force, Vector3 hitDirection, PlayerControllerB playerWhoHit, bool playHitSFX, int hitID)
    {
        
        if (playerWhoHit != null && playerWhoHit.currentlyHeldObjectServer != null && playerWhoHit.currentlyHeldObjectServer.TryGetComponent<BetterShovel>(out BetterShovel betterShovel))
        {
            BetterShovelDamageDealtOnEachEnemy[self] += betterShovel.ExperimentalShovelHitForce;
            force = Mathf.FloorToInt(BetterShovelDamageDealtOnEachEnemy[self]);
            BetterShovelDamageDealtOnEachEnemy[self] -= force;
            Plugin.Log.LogInfo($"[BetterShovel] Applying {force} damage to enemy {self.name}");
            Plugin.Log.LogInfo($"[BetterShovel] Health: {self.enemyHP}");
            self.HitEnemy(force, playerWhoHit, playHitSFX, hitID);
            self.HitEnemyServerRpc(force, (int)playerWhoHit.playerClientId, playHitSFX, hitID);
        } else {
            orig(self, force, hitDirection, playerWhoHit, playHitSFX, hitID);
        }
    }

    private static void EnemyAI_Start(On.EnemyAI.orig_Start orig, EnemyAI self)
    {
        orig(self);
        BetterShovelDamageDealtOnEachEnemy[self] = 0f;
    }
}
