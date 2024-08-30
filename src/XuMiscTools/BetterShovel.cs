using System;
using System.Linq;
using GameNetcodeStuff;
using UnityEngine;

namespace XuMiscTools;
public class BetterShovel : Shovel
{
    [NonSerialized] public int defaultForce = 0;

    [Tooltip("Setup the shovel type automatically, you STILL need to make a proper itemProperties and set it up how you see fit, this just makes sure the values are correct.")]
    public bool SetupShovelTypeAutomatically = false;

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
        if (ExperimentalShovelHitForce > 0) {
            this.shovelHitForce = 0;
        }
    }
}
