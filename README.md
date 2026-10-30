# Platformer-Game-Pixel 🎮

A small, landscape-first Unity 2D platformer prototype for Android. The project includes a basic Unity scene with runtime-generated pixel-style art and simple platforming mechanics.

## What's inside
- A Unity project layout with `Assets/`, `Packages/` and `ProjectSettings/`.
- A `Prototype` scene that runs the prototype bootstrap script.
- Runtime-generated pixel-style hero, grass platforms, clouds, stars and finish flag.
- Keyboard controls for testing in the Unity Editor and on-screen touch controls.
- Jumping, collectible stars, a simple HUD and a restart button.
- No paid asset packs, ads, accounts, analytics or external services.

## Development setup

Development is currently being done locally on a laptop using the Unity Editor. GitHub Actions is an optional build experiment, not a requirement for opening or testing the project.

- **Unity Editor:** `6000.6.7f1`
- **Project type:** 2D
- **Target platform:** Android
- **Orientation:** Landscape

Use the same Unity Editor version where possible to avoid unnecessary project upgrades.

## Open the project on your laptop
1. Clone this repository with GitHub Desktop or download the repository.
2. Open Unity Hub and choose **Add → Add project from disk**.
3. Select the cloned `Platformer-Game-Pixel` folder.
4. Open it with Unity `6000.6.7f1`.
5. Open `Assets/Scenes/Prototype.unity` and press **Play**.

## Controls
- **Move:** A/D or ←/→
- **Jump:** Space, W or ↑
- **Restart:** R
- **Touch:** Hold ◀ / ▶ to move and tap **JUMP**.

## Build for Android
1. In Unity Hub, install **Android Build Support**, including the Android SDK & NDK Tools and OpenJDK modules.
2. Open the project in Unity.
3. Go to **File → Build Profiles**, select **Android** and switch to that platform.
4. In **Project Settings → Player → Resolution and Presentation**, set **Default Orientation** to **Landscape Left** (or enable only landscape rotations).
5. In **Project Settings → Player → Other Settings**, set **Active Input Handling** to **Input Manager (Old)** or **Both**. The prototype currently uses Unity's legacy `Input` API.
6. Make sure `Assets/Scenes/Prototype.unity` is included in the build scene list.
7. Build an APK and test it on an Android device.

## GitHub Actions

The repository includes an experimental Unity Android build workflow. It needs a valid Unity license secret and the required Unity account secrets before the build step can run. A recent run stopped at the secret check because `UNITY_LICENSE` was missing, so a successful cloud build has **not** been verified.

You can continue developing and building locally in Unity without relying on that workflow.

## Current scope

This is an early prototype, not a finished game or a verified ready-to-install APK. The level and art are generated at runtime to keep the repository small. The project still needs a successful Unity Editor compile and gameplay testing. Future work could include proper sprite sheets, animation, sound, checkpoints and level progression.
