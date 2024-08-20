using System.Linq;
using UnityEngine;

namespace XuMiscTools;
public class BetterShovel : Shovel {
    public float ShovelSpeedMultiplier = 1f;
    private bool _animatorSpeedCurrentlyModified;
    private float _originalPlayerAnimatorSpeed;


    public override void Start()
    {
        base.Start();
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
            this.shovelAudio.pitch = newSpeed;
            playerHeldBy.playerBodyAnimator.speed = newSpeed;
        }
        previousPlayerHeldBy = playerHeldBy;
    }

    public override void DiscardItem()
    {
        base.DiscardItem();
        // Get the reel up animation clip
        AnimationClip reelingUpClip = previousPlayerHeldBy.playerBodyAnimator.runtimeAnimatorController.animationClips
            .FirstOrDefault(clip => clip.name == "ShovelReelUp");
        AnimationClip swingDownClip = previousPlayerHeldBy.playerBodyAnimator.runtimeAnimatorController.animationClips
            .FirstOrDefault(clip => clip.name == "HitShovel");

        // Check if we found the clip successfully.
        if (reelingUpClip != null && swingDownClip != null &&  _animatorSpeedCurrentlyModified)
        {
            // Get the current player body animator speed.
            previousPlayerHeldBy.playerBodyAnimator.speed = _originalPlayerAnimatorSpeed;
            this.shovelAudio.pitch /= ShovelSpeedMultiplier;
            _animatorSpeedCurrentlyModified = false;
        }
    }

    public override void PocketItem()
    {
        base.PocketItem();
        // Get the reel up animation clip
        AnimationClip reelingUpClip = previousPlayerHeldBy.playerBodyAnimator.runtimeAnimatorController.animationClips
            .FirstOrDefault(clip => clip.name == "ShovelReelUp");
        AnimationClip swingDownClip = previousPlayerHeldBy.playerBodyAnimator.runtimeAnimatorController.animationClips
            .FirstOrDefault(clip => clip.name == "HitShovel");

        // Check if we found the clip successfully.
        if (reelingUpClip != null && swingDownClip != null && _animatorSpeedCurrentlyModified)
        {
            // Get the current player body animator speed.
            previousPlayerHeldBy.playerBodyAnimator.speed = _originalPlayerAnimatorSpeed;
            this.shovelAudio.pitch /= ShovelSpeedMultiplier;
            _animatorSpeedCurrentlyModified = false;
        }
    }
}