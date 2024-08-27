using System;
using GameNetcodeStuff;
using UnityEngine;

namespace XuMiscTools;
public class BetterShovel : Shovel
{
    private bool _animatorSpeedCurrentlyModified;
    private PlayerControllerB previouslyHeldBy = null!;
    [NonSerialized] public int defaultForce = 0;

    [Tooltip("Setup the shovel type automatically, you STILL need to make a proper itemProperties and set it up how you see fit, this just makes sure the values are correct.")]
    public bool SetupShovelTypeAutomatically = false;

    [Tooltip("The speed of the shovel hitting.")]
    public float ShovelSpeedMultiplier = 1f;

    [Tooltip("If true, the player will be able to crit with this weapon, dealing 2x damage.")]
    public bool CritPossible = false;
    [Tooltip("The chance that the player will crit with this weapon.")]
    public float CritChance = 0;

    [Tooltip("If true, the player will be able to break trees using this weapon, uses vehicle mechanics.")]
    public bool CanBreakTrees = false;
    [Tooltip("The position of the tip of the shovel.")]
    public Transform WeaponTip = null!;

    [Tooltip("Experimental damage that can be a float, set the ShovelHitForce to 0 if you want to use this.")]
    public float ExperimentalShovelHitForce = 0f;

    private AnimatorOverrideController? _animatorOverrideController;
    private AnimationClip _ShovelReelUpClip = null!;
    private AnimationClip _HitShovelClip = null!;

    public override void Start()
    {
        base.Start();
        if (SetupShovelTypeAutomatically)
        {
            this.itemProperties.twoHandedAnimation = true;
            this.itemProperties.weight = Mathf.Clamp(this.itemProperties.weight, 1, 9);
            this.itemProperties.grabAnim = "HoldLung";
            this.itemProperties.isDefensiveWeapon = true;
            this.itemProperties.holdButtonUse = true;
        }

        // Retrieve the required animation clips
        foreach (AnimationClip clip in StartOfRound.Instance.allPlayerScripts[0].playerBodyAnimator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == "ShovelReelUp")
            {
                _ShovelReelUpClip = clip;
            }
            else if (clip.name == "HitShovel")
            {
                _HitShovelClip = clip;
            }
        }
    }

    public override void EquipItem()
    {
        base.EquipItem();

        // Setup the AnimatorOverrideController if not already done
        if (_animatorOverrideController == null)
        {
            _animatorOverrideController = new AnimatorOverrideController(playerHeldBy.playerBodyAnimator.runtimeAnimatorController);
            playerHeldBy.playerBodyAnimator.runtimeAnimatorController = _animatorOverrideController;
        }

        // Modify the animation speed by overriding the clips
        ModifyAnimationSpeed("ShovelReelUp", _ShovelReelUpClip, ShovelSpeedMultiplier);
        ModifyAnimationSpeed("HitShovel", _HitShovelClip, ShovelSpeedMultiplier);

        previouslyHeldBy = playerHeldBy;
    }

    public override void DiscardItem()
    {
        base.DiscardItem();
        RestoreOriginalAnimationSpeed();
    }

    public override void PocketItem()
    {
        base.PocketItem();
        RestoreOriginalAnimationSpeed();
    }

    private void ModifyAnimationSpeed(string clipName, AnimationClip originalClip, float speedMultiplier)
    {
        if (_animatorOverrideController == null || originalClip == null) return;

        AnimationClip overrideClip = originalClip;
        overrideClip.name = originalClip.name;
        overrideClip.frameRate = originalClip.frameRate * speedMultiplier;

        // Replace the clip in the AnimatorOverrideController
        _animatorOverrideController[clipName] = overrideClip;

        _animatorSpeedCurrentlyModified = true;
    }

    private void RestoreOriginalAnimationSpeed()
    {
        if (_animatorOverrideController != null && _animatorSpeedCurrentlyModified)
        {
            _animatorOverrideController["ShovelReelUp"] = null;
            _animatorOverrideController["HitShovel"] = null;
            _animatorSpeedCurrentlyModified = false;
        }
    }
}
