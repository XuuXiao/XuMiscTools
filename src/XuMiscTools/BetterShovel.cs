using System;
using System.Linq;
using GameNetcodeStuff;
using UnityEngine;

namespace XuMiscTools;
public class BetterShovel : Shovel {
    private bool _animatorSpeedCurrentlyModified;
    private float _originalPlayerAnimatorSpeed;
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


    public override void Start()
    {
        base.Start();
        if (SetupShovelTypeAutomatically) {
            this.itemProperties.twoHandedAnimation = true;
            this.itemProperties.weight = Mathf.Clamp(this.itemProperties.weight, 1, 9);
            this.itemProperties.grabAnim = "HoldLung";
            this.itemProperties.isDefensiveWeapon = true;
            this.itemProperties.holdButtonUse = true;
        }
    }
    public override void EquipItem() {
        base.EquipItem();
        // Get the reel up animation clip
        AnimationClip reelingUpClip = playerHeldBy.playerBodyAnimator.runtimeAnimatorController.animationClips
            .FirstOrDefault(clip => clip.name == "ShovelReelUp");
        AnimationClip swingDownClip = playerHeldBy.playerBodyAnimator.runtimeAnimatorController.animationClips
            .FirstOrDefault(clip => clip.name == "HitShovel");

        // Check if we found the clip successfully.
        if (reelingUpClip != null && swingDownClip != null && !_animatorSpeedCurrentlyModified)
        {
            // Get the current player body animator speed.
            _originalPlayerAnimatorSpeed = playerHeldBy.playerBodyAnimator.speed;
            
            // Get the length of the reel up animation.
            float animationOrigionalLength = reelingUpClip.length;
            
            // Calculate the new speed of the reel up.
            float newSpeed = animationOrigionalLength * ShovelSpeedMultiplier;
            
            // Set the player body animator to use the new speed.
            _animatorSpeedCurrentlyModified = true;
            playerHeldBy.playerBodyAnimator.speed = newSpeed;
        }
        previouslyHeldBy = playerHeldBy;
    }

    public override void DiscardItem()
    {
        base.DiscardItem();
        // Get the reel up animation clip
        AnimationClip reelingUpClip = previouslyHeldBy.playerBodyAnimator.runtimeAnimatorController.animationClips
            .FirstOrDefault(clip => clip.name == "ShovelReelUp");
        AnimationClip swingDownClip = previouslyHeldBy.playerBodyAnimator.runtimeAnimatorController.animationClips
            .FirstOrDefault(clip => clip.name == "HitShovel");

        // Check if we found the clip successfully.
        if (reelingUpClip != null && swingDownClip != null &&  _animatorSpeedCurrentlyModified)
        {
            // Get the current player body animator speed.
            previouslyHeldBy.playerBodyAnimator.speed = _originalPlayerAnimatorSpeed;
            _animatorSpeedCurrentlyModified = false;
        }
    }

    public override void PocketItem()
    {
        base.PocketItem();
        // Get the reel up animation clip
        AnimationClip reelingUpClip = previouslyHeldBy.playerBodyAnimator.runtimeAnimatorController.animationClips
            .FirstOrDefault(clip => clip.name == "ShovelReelUp");
        AnimationClip swingDownClip = previouslyHeldBy.playerBodyAnimator.runtimeAnimatorController.animationClips
            .FirstOrDefault(clip => clip.name == "HitShovel");

        // Check if we found the clip successfully.
        if (reelingUpClip != null && swingDownClip != null && _animatorSpeedCurrentlyModified)
        {
            // Get the current player body animator speed.
            previouslyHeldBy.playerBodyAnimator.speed = _originalPlayerAnimatorSpeed;
            _animatorSpeedCurrentlyModified = false;
        }
    }
}