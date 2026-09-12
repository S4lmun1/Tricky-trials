Knife quick start
=================
Both GameScene and _Recovery/0 start with the knife in the main hotbar slot.
Left click attacks. Q drops. E picks up when aimed within Pickup Range.
1 selects the main slot; 2 selects the gun. Dropping leaves the main slot blank.

Replace the model on Assets/Prefabs/Knife.prefab using Model Prefab.
Tune damage/range/attack interval on Assets/Items/Knife.asset.
The shared PickupItem base handles equip/drop physics. WeaponManager controls
selection, input, and the main slot. All gameplay settings are Inspector fields.

See Assets/Editing-Guide.md for player models, new items, controls, and extension
points. Knife's placeholder is built only when Model Prefab is empty.
