# XuMiscTools

Mod for moon creators who need some better tools for some damage stuff, or shovel stuff, or really anything, this is technically a library I guess.

## Currently includes the following:
### BetterCooldownTrigger script.
- Requires two things: a Collider with `IsTrigger` turned on, and this script put onto that same gameobject, from there, you'll be able to specify the following:
- The cooldown interval for how long the script waits before it triggers again.
- The damage to the player every time the interval ends.
- Whether the script affects enemies too.
- The cooldown interval for affecting enemies too.
- The damage to the enemy every time the interval ends.
- Whether the cooldown is shared among different gameobjects that the script is put onto, i.e. useful if don't want to spam the player with damage because you condensed the collider on top of eachother in some places.
- Specify both the ragdoll and the cause of death when the player dies.
- Apply a force onto the player either every damage interval or on death, the direction of the force can be either relative to the object or relative to the player.
- Able to have random sounds play, give it a list of sounds and a list of audiosources and it'll play the audiosource closest to the player with a random sound.
- Able to choose to have whether you want dogs to be attracted by the sound or not.

### BetterShovel script (allows you to control reeling and hitting speed of the shovel).
- Control shovel reel and hit speed.
- Whether shovel can break trees.
- Whether shovel can crit for 2x damage.

Licensed under MIT.
