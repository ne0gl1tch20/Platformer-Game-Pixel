# Kenney asset setup

The original Kenney archives are extracted under this folder. Their original license files are kept beside the source art.

## Imported assets

- **Platformer Art Extended Tileset** supplies the grass environment tiles.
- **Platformer Characters** supplies the Adventurer poses and animation frames.
- The Unity editor utility creates Tile assets, animation clips, Animator Controllers and reusable character prefabs under `Generated/`.

Both packs include a CC0 license. The original license files remain in their source folders.

## Unity setup

1. Open the project with Unity **6000.6.7f1** and let the first import finish.
2. Wait for Unity to compile the editor utility.
3. If generated assets are missing, choose **Tools → Kenney → Configure Imported Assets**.
4. Open `Assets/Scenes/Prototype.unity` and press **Play**.

## Prototype integration

The Prototype scene now references the generated **Adventurer prefab** and the Grass **left, middle and right Tile assets**. The gameplay script builds the level layout with a Tilemap, instantiates the imported Adventurer and drives the imported Animator Controller with `MoveSpeed`, `Grounded` and `VerticalVelocity`.

The character and terrain visuals are source Kenney assets. The gameplay script does not create textures or draw replacement character/terrain art. The small collectible and goal labels are text glyphs.

## Controls

- **Move:** A/D or ←/→
- **Jump:** Space, W or ↑
- **Restart:** R
- **Touch:** Hold ◀ / ▶ to move and tap **JUMP**.

## Attribution and license

- Platformer Art Extended Tileset by Kenney (Kenney.nl), CC0.
- Platformer Characters by Kenney (Kenney.nl), CC0.
- Attribution is optional under CC0, but credit is retained here as a courtesy.

## Notes

- The generated assets and original source art should stay committed together so Unity references remain valid.
- The scene and script references are configured in GitHub. Unity Editor compilation and actual gameplay still need to be verified locally.
