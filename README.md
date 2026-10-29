# Platformer-Game-Pixel 🎮

A tiny, cute, landscape-first Unity 2D platformer prototype for Android. This repository now includes a basic Unity project structure and a first prototype scene.

## What's inside
- A Unity project layout with `Assets/`, `Packages/` and `ProjectSettings/`.
- A `Prototype` scene that automatically runs the prototype bootstrap script.
- Runtime-generated pixel-style hero, grass platforms, clouds, stars and finish flag.
- Keyboard controls for editor testing and on-screen controls for touch devices.
- Jumping, collectible stars, a simple HUD and restart button.
- No paid asset packs, ads, accounts, analytics or external services.

## Open the project
1. Clone this repository with GitHub Desktop or Unity Hub's **Add → Add from repository** option.
2. In Unity Hub, choose **Add → Add project from disk** and select the cloned `Platformer-Game-Pixel` folder if it is not already listed.
3. Open the project. Unity may ask to upgrade the project version to match your installed Editor; accept only if you want to use that installed version.
4. Open `Assets/Scenes/Prototype.unity` and press **Play**.

The project metadata starts at Unity `6000.0.0f1` (Unity 6.0). The prototype has not yet been compiled or gameplay-tested in the Unity Editor, so treat this as the initial project scaffold.

## Controls
- Editor: A/D or ←/→ to move, Space/W/↑ to jump, R to restart.
- Touch: hold ◀ / ▶ to move and tap JUMP.

## Android setup
1. Install the Android Build Support module for your Unity Editor in Unity Hub.
2. In **File → Build Profiles** (or **Build Settings** in older Editors), select Android and switch to it.
3. In **Project Settings → Player → Resolution and Presentation**, set **Default Orientation** to **Landscape Left** (or enable only landscape rotations).
4. In **Project Settings → Player → Other Settings**, set **Active Input Handling** to **Input Manager (Old)** or **Both**. The current prototype uses Unity's legacy `Input` API.
5. Add `Assets/Scenes/Prototype.unity` to the build scene list if it is not already listed, then build to a test device.

## Current scope
This is an early playable-slice scaffold, not a finished game or a ready-to-install APK. The scene builds the level and art at runtime to keep the repository small. Next steps: compile and test in Unity, fix any input/physics issues, then add proper sprite sheets, animation, sound, checkpoints and level progression.
