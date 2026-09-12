# Editing the game

## Replace the player model

Open your scene and select Player. Its Player Visuals component now has a
Model Prefab field. Drag your character model prefab there. Adjust Model Position,
Model Euler Angles, and Model Scale. A model with its origin at its feet will
usually need Model Position Y around -0.5 with the current player collider.

Player > Visual Model contains the temporary cube. The cube automatically hides
when a model is assigned. Only this visual child turns with the view; the physics
root and camera stay independent. The imported visual model's colliders and
Rigidbody collisions are disabled, and Animator root motion is disabled.
Use a visual-only prefab without gameplay scripts. Animation transitions and
first-person head/arm visibility still need setup for your particular model.

Keep Player's Rigidbody, collider, PlayerMovement, camera, and PlayerVisuals.
Resize the root collider separately if you change the character's physical size.
During Play Mode, the Player Visuals component menu has Apply Model (Play Mode)
for testing a newly assigned prefab. Runtime character selection can call SetModel.

## Add a melee item

Duplicate Assets/Prefabs/Knife.prefab and Assets/Items/Knife.asset.
On the duplicated prefab, assign its Item Data and Model Prefab.
On the item asset, set Item Name, damage, attack range, attack interval (seconds),
and Held Prefab to your duplicated prefab. The item asset is the source of weapon
stats; component fallback stats are only used when Item Data is empty.

Adjust held pose, model offsets, collider size/center, mass, pickup delay, hit
layers, and swing settings on the prefab. All are in the Inspector. The
placeholder's individual pieces can also be changed under Placeholder Parts.

Drag the prefab into the scene to make a world pickup, or assign it to
WeaponManager > Starting Item to start with it. Names are read from Item Data.
Pickup/drop code has no reference to the Knife class or knife name.

## Add other item actions

PickupItem handles world physics, ownership, equip, and drop. Use it on the root
of a prefab for a generic carryable object and assign a visual Model Prefab.
For a gun pickup, add Gun on the same root and assign the same Item Data to both.
For a new action, subclass PickupItem and override Use(), or implement
IWeaponAction.Use(Camera, Transform) on another root component.
Use one action per item root. The existing Gun uses this same action interface.

The main slot intentionally holds one pickup at a time. Drop it before collecting
another. Additional inventory capacity, stacking, and new mechanics are separate
features; this refactor provides extension points, not those features themselves.

## Equipment and controls

WeaponManager > Weapons is the configurable equipment list. Each entry accepts
an existing scene object or an Item Data asset with a Held Prefab, plus a Select
Key. Set Empty Hands Index to -1 for no equipped object (there is no fists item). Assign the Main Slot button,
Player, Player Camera, and optional Held Socket. Items use the camera if there is
no socket. Main-slot pickup/selection, attack, and drop keys are independent fields.

Current keys: 1 main pickup, 2 gun, E pickup, Q drop, left-click use.
Pickups always go to the main slot and auto-equip. A full main slot rejects pickup.
Drop Distance, Drop Speed, Drop Clearance, Pickup Range, and Interaction Layers
are editable. Drop Clearance should cover the largest droppable item's collider.

PlayerMovement exposes movement, jump, sprint, mouse sensitivity, look limits,
input axis names, cursor keys, ground threshold, and optional legacy hazard/score
tags. Look Sensitivity stays at the working default 1. Do not restore the obsolete
mouseSensitivity field: old scenes stored 800 using a different scale.

InventoryManager is optional and remains disabled in these scenes. It now only
opens a panel and pauses player controls. It no longer toggles the gun behind
WeaponManager's back. Assign a separate inventory panel before enabling it.

## HUD, damage, and world

WeaponManager exposes crosshair visibility, size, outline, colors, sorting order,
slot text size/color, and empty-slot wording. No slot number or score GUI is added.
DamageableTarget exposes health, Destroy On Death, On Damaged, and On Death events.
Your own health component can implement IDamageable.

The corridor is ordinary scene cubes with colliders and shared materials under
Assets/Materials. Edit/delete those objects directly; nothing recreates the world
at runtime. Fog and sky background are scene/camera settings.

## Check after edits

Stop Play Mode, save/reopen the intended scene, then press Play. Both GameScene
and _Recovery/0 are wired, but Unity may retain unsaved scene values in memory.
Check walking/looking, jumping, slot 1 -> 2 -> 1, attack, Q drop, E pickup,
and a second drop/pickup. Test a model swap while leaving the physics root intact.

## Enemy encounter

Both scenes contain Corridor Enemy at the far end of the second corridor section,
after the right turn. It is a red block at (34, 0, -21). Select Corridor Enemy in
Unity's Hierarchy and press F over Scene view to find it.

DamageableTarget holds health: 50 on the enemy, 100 on Player. ChaseEnemy exposes
Detection Range (10), Move Speed, Attack Damage (20), Attack Interval (1 second),
stopping distance and model replacement fields. Damage requires physical contact.
Once detected, the enemy keeps chasing by default; change Keep Chasing After
Detection if it should stop when you leave the detection radius.

Navigation World builds paths at Play startup from its Geometry collider list.
When replacing the corridor, assign your new box/mesh colliders there. Keep the
navigation radius/height compatible with the enemy. Player and item colliders
are deliberately not navigation sources. There is no ranged attack; the enemy must physically touch the player.

PlayerRespawn restores the player's starting health and position after death.
Its delay, optional Respawn Point, and Respawn On Death toggle are editable.
Health Display on Player shows current/max health. Enemies have a world-space health bar.
At the current knife damage of 25, two successful knife hits defeat the enemy.
Use Assets/Scenes/GameScene.unity as the main scene going forward. The recovery
scene was kept synchronized because it was previously open in Unity.
Contact damage repeats every Attack Interval while touching (default 1 second).
Keep Stopping Distance at 0 for contact attacks so the enemy reaches the player.

## Health display styling
Select Player or Corridor Enemy and edit Health Display for colors, size, margins,
font size, world offset, and visibility distance. Add HealthDisplay to another
DamageableTarget to reuse it; enable World Space for a floating enemy bar.
Health bars use the starting health as their maximum and update after respawn.
The knife range is now 3 meters, configured on Assets/Items/Knife.asset.

## Portals and the maze

First Exit Portal sits at the end of the first corridor. Kill every enemy under
First Encounter - Add Enemies Here, then walk into its white surface. The dark
lock bars disappear when the encounter is cleared. Adding enemies under that
encounter root automatically includes them in the unlock condition; enemies
elsewhere can be assigned in EnemyGroup.Additional Enemies or registered by code.

The linked Maze Arrival Portal is at the maze entrance. It seals on arrival and
has Arrival Only enabled, so there is no return trip. The arrival Transform also
becomes PlayerRespawn's checkpoint. Death in the maze restores player health at
the maze entrance rather than bypassing the portal lock by returning to corridor 1.

Maze Geometry contains editable floor, perimeter walls, and branching dividers.
Maze Encounter has three enemies in a triangle: Front, Back Left, and Back Right.
They retain 50 health, 10-meter detection, and 20 contact damage with a one-second
interval. Their health bars and visual model replacement fields remain connected.

PortalGate exposes Destination, Arrival Portal, encounter requirements, sealing,
checkpoint update, delay, glow intensity/color, pulse, light range, and indicators.
Change those Inspector references to link different areas. Portal Bloom Only adds
bloom without enabling the previous color-grading volume; fog remains disabled
and the camera background remains white. Both portals and every maze piece are
scene objects, so none of their positions are embedded in gameplay scripts.

Navigation World includes both areas' floor/wall colliders. Update Geometry if
replacing the maze. Keep EnemyGroup enemy roots separate to avoid requiring kills
in an inaccessible future area before opening the first portal.
## Enemy prefab and wall movement

Drag `Assets/Prefabs/Basic Enemy.prefab` onto a walkable floor. Its root sits at
floor height; it finds the player and camera automatically. It includes 50 health,
20 contact damage, chasing, a replaceable Visual Model, and a floating health bar.
Parent it under the appropriate encounter (First Encounter or Maze Encounter) to
make its death count toward that level's portal and reward. Alternatively assign
its DamageableTarget in that EnemyGroup's Additional Enemies list. Place encounter
enemies before the encounter is completed. New floors/walls still need to be
included in Navigation World's Geometry list.

The Player Movement physics material has zero static/dynamic friction and Minimum
combine mode. This prevents pushing into a wall from holding the player up.
Deceleration on PlayerMovement still controls ground braking. The material is
assigned to the player collider and exposed on PlayerMovement; camera sensitivity
has not been changed.

## Level reward cards

Killing all enemies unlocks the exit portal. Cards appear only AFTER the player
travels through that portal. The equipped ItemData is captured at portal crossing,
so switching weapons before entering also changes the reward pool. Five face-down
cards deal onto the screen; click one to apply it, then Continue to resume.
Each encounter rewards once per run, even if its exit permits repeat travel.

On exit portals, enable Grant Completion Reward, assign Level Rewards, and assign
the encounter being completed. Add that encounter to Level Rewards > Encounters.
Arrival-only portals do not give rewards. The current maze has no onward exit yet,
so killing its enemies alone does not open cards; connect a new exit when extending
that level.

All card balancing lives in `Assets/Upgrades`. Create another card with Project
window > Create > Tricky Trials > Upgrades > Card. Set its description, effects,
Weight, and Max Stacks, then add it to the relevant Pool asset's Cards list. Assign
the pool to the item's ItemData > Upgrade Pool. No weapon-name checks are used.
Descriptions are author-written: update them when changing the numeric effects.

Current knife cards:
- Quick Edge: interval x0.8, damage -5; weight 8, repeatable.
- Heavy Edge: damage +10, interval x1.25; weight 8, repeatable.
- Long Reach: range +0.75 meters, damage -3; weight 8, maximum one selection.
- Katana: damage +10, range +0.5 meters, interval x0.85; weight 1, maximum one.
  Its Model Prefab is a temporary katana visual; replace that reference with your
  real model. Pickup, drop, combat, and existing upgrades stay intact.
- Flame: 5 damage every 0.5 seconds for 2 seconds; weight 1, maximum one.
  A new hit refreshes the existing burn without creating overlapping timers.
  Flame remains active when the Katana model is applied.

A smaller attack interval means faster attacks. Additions and multipliers apply
in selection order. Runtime safety floors prevent zero damage, range, or attack
interval. Shared item/card assets are never modified by gameplay.

Cards are drawn independently WITH replacement, so two hidden cards may contain
the same upgrade. This preserves rare odds even with only five definitions, and
still provides five choices after single-selection upgrades leave the pool.
Max Stacks is checked against cards already taken, not cards merely offered.
Weight 1 is eight times less likely per draw than weight 8.

Empty Hand Pool is separate: Vitality (+20 maximum/current health), Recovery
(+35 healing), Light Step (+0.5 walk/sprint speed), Runner (+1 sprint speed), and
Spring (+0.35 jump strength). Player cards are offered only when no item is equipped. Items use their own pool
exclusively; a missing/exhausted pool reports a configuration error instead of
substituting player cards. Keep a repeatable card in each pool. The existing gun
has a separate generic weapon pool.

Run Upgrades lives on Player and stores modifiers by ItemData. They survive
pickup/drop, model changes, portals, and checkpoint respawns. Stopping Play or
reloading the gameplay scene starts a new run. Current levels share one scene;
if you later split levels into separate scenes, move this run state into your
cross-scene run manager rather than restarting it on each scene load.

## Testing the new features

After Unity imports, stop Play and reopen GameScene (the recovery copy was also
updated). Jump while moving into a wall, then kill the first enemy with the knife
equipped. No cards should appear on the kill. Enter the portal with the knife
equipped, choose a knife card, and Continue. To test rare cards quickly, temporarily raise their
Weight values in the card assets. Drop and pick up the knife to check that
modifiers and the replacement visual remain. Restore the weights afterward.

Validation performed: Unity-reference compilation (zero errors/warnings), local
scene/prefab reference checks, and 16 isolated upgrade/burn logic checks using
simulated Unity services. A live Unity physics/UI playtest is still needed.

## Reward asset validation

The item reward references were repaired and saved through Unity's AssetDatabase.
Unity verification confirmed Knife -> Knife Pool -> five weapon cards, with 100
successful pool draws and no player-health cards. The one-time repair was removed;
your future Inspector edits are preserved. To inspect the loaded links again, use
Tools > Tricky Trials > Check Reward Assets. It writes a read-only diagnostic to
Temp/RewardAssetCheck.txt. This checks actual Unity-loaded assets, not just YAML.
