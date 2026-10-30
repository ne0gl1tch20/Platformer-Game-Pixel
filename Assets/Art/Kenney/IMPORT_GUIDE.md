# Kenney asset setup

The original Kenney archives have been extracted under this folder. Their original license files are kept beside the source art.

## Unity setup

1. Open the project with Unity **6000.6.7f1** and let the first import finish.
2. Wait for Unity to compile the editor utility.
3. In the Unity menu, choose **Tools → Kenney → Configure Imported Assets**.
4. Wait for the setup to finish. Check the Console for the completion message or any import errors.

The editor utility is at `Assets/Editor/KenneyAssetSetup.cs`.

## What it configures

- Sprite texture type for the Kenney PNGs.
- Point filtering, no mipmaps, uncompressed textures, clamp wrapping and transparent-pixel handling.
- 70 pixels per unit for the 70×70 environment tiles and 18 pixels per unit for the character pack.
- Multiple-sprite slicing for each environment `sheet.png`, using its matching `sheet.xml` atlas coordinates.
- Reusable Unity `Tile` assets for individual environment PNGs and sliced atlas sprites.
- Character animation clips from the supplied individual pose PNGs. Numbered frames such as `walk1` and `walk2` are grouped into one looping clip.
- Animator Controllers with basic idle/walk/jump/fall/hurt transitions and trigger-driven optional poses.
- Reusable character prefabs with SpriteRenderer, Animator, Rigidbody2D and CapsuleCollider2D.

Generated Unity assets are placed in `Assets/Art/Kenney/Generated/`.

## Important

- The original PNGs, XML files and license files are not modified.
- The character prefabs are reusable starting points. The game controller still needs to set Animator parameters such as `MoveSpeed`, `Grounded`, `VerticalVelocity` and the optional pose triggers.
- The tool does not rewrite the prototype scene or replace its gameplay script. Review the generated prefabs and tile assets in Unity before wiring them into a level.
- Unity Editor execution is required to generate Unity's imported sprite metadata, animation clips, controllers, prefabs and Tile assets. These generated files are created locally by the menu command and should be committed from the Unity project after reviewing them.
