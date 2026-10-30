# Platformer-Game-Pixel 🎮

A landscape-first Unity 2D platformer prototype for Android, using real Kenney pixel-art character and environment assets.

## What's inside

- Unity **6000.6.7f1** project targeting Android in landscape orientation.
- Kenney Platformer Characters pack with the Adventurer sprite poses and animation frames.
- Kenney Platformer Art Extended Tileset with imported Grass Tile assets.
- Generated Adventurer prefab, animation clips and Animator Controller.
- A Tilemap-based platform layout using the imported Kenney terrain tiles.
- Keyboard controls and on-screen touch controls.
- Jumping, collectible star glyphs, a simple HUD and restart.
- Original Kenney license files preserved in the repository. Both included packs are CC0.
- No ads, accounts, analytics or external services.

## Open the project

1. Clone this repository with GitHub Desktop or download it.
2. In Unity Hub, choose **Add → Add project from disk**.
3. Select the cloned `Platformer-Game-Pixel` folder.
4. Open it with Unity **6000.6.7f1**.
5. Open `Assets/Scenes/Prototype.unity` and press **Play**.

If the generated Kenney assets are missing, open **Tools → Kenney → Configure Imported Assets** once and commit the generated files from the Unity project.

## Controls

- **Move:** A/D or ←/→
- **Jump:** Space, W or ↑
- **Restart:** R
- **Touch:** Hold ◀ / ▶ to move and tap **JUMP**.

## Kenney asset credits

- **Platformer Art Extended Tileset** by Kenney (Kenney.nl), CC0.
- **Platformer Characters** by Kenney (Kenney.nl), CC0.

The original `license.txt` / `License.txt` files are kept with each source pack.

## Build for Android

1. In Unity Hub, install **Android Build Support**, including Android SDK & NDK Tools and OpenJDK.
2. Open the project in Unity.
3. Go to **File → Build Profiles**, select **Android** and switch to that platform.
4. In **Project Settings → Player → Resolution and Presentation**, set the default orientation to **Landscape Left** (or enable only landscape rotations).
5. In **Project Settings → Player → Other Settings**, set **Active Input Handling** to **Input Manager (Old)** or **Both**. The prototype currently uses Unity's legacy `Input` API.
6. Make sure `Assets/Scenes/Prototype.unity` is included in the build scene list.
7. Build an APK and test it on an Android device.

## GitHub Actions

The repository includes an experimental Unity Android build workflow. Cloud licensing/building has not been verified. Local Unity Editor builds are the supported development path for this project right now.

## Current status

The imported Kenney assets are wired into the Prototype scene and gameplay script in GitHub. A successful Unity Editor compile, Play Mode test and Android APK build have **not** yet been verified. Those checks still need to happen locally in Unity.
