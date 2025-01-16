# Get a visual for the different colliders the enemies use 

Activate it through CConsole with the command `DisplayEnemyHitboxes [true/false]`

* Green hitboxes are normal limbs
* Red hitboxes are weakpoints
* Grey hitboxes are armor

(Note that in the case of modded enemies, this color doesn't necessarily mean they do different amounts of damage, just that the limb itself is internally marked as those types. The actual damage they take to that limb will be determined by datablock values)

To be technical, this mod adds shapes to sphere, box, and capsule colliders on enemy limbs.

# Extra info
To build yourself, add `GameFolder.props` next to `Properties.props` and format it like this:
```xml
<Project>
  <PropertyGroup>
    <GameFolder>C:\Path\To\Mod Profile\Folder</GameFolder>
  </PropertyGroup>
</Project>
```
... such that `C:\Path\To\Mod Profile\Folder\BepInEx` is in fact your valid BepInEx path.

Thanks to tru0067 for contributing:
* Enemy Melee Hitboxes
* Enemy Back Multiplier visualizers
* Player Melee Hitboxes