# Platformer-Game-Pixel 🎮

A tiny, cute, landscape-first Unity 2D platformer prototype for Android. The first playable slice is generated from code, so it does not depend on paid asset packs.

## Prototype features
- Auto-builds a small platforming level when you press Play.
- Pixel-style hero, grass platforms, clouds, stars and a finish flag generated at runtime.
- Keyboard controls for editor testing and on-screen controls for touch devices.
- Jumping, collectible stars, a simple HUD and a restart button.
- No ads, accounts, analytics or external packages.

## Run it in Unity
1. Create/open a **Unity 2D** project.
2. Create the folder `Assets/Scripts`.
3. Copy `PrototypeBootstrap.cs` into that folder.
4. Create an empty GameObject in a new scene named `PrototypeBootstrap`.
5. Add the `PrototypeBootstrap` component to it and press **Play**.
6. For Android, switch Build Settings to Android and set Player Settings → Resolution and Presentation → Default Orientation to **Landscape Left** (or Auto Rotation with only landscape enabled).
7. In Player Settings → Other Settings, set Active Input Handling to **Input Manager (Old)** or **Both**.

## Controls
- Editor: A/D or ←/→ to move, Space/W/↑ to jump, R to restart.
- Touch: hold ◀ / ▶ to move and tap JUMP.

## Current scope
This is the first playable prototype, not a finished game or a ready-to-install APK. The scene is built at runtime to keep setup small; next steps are proper sprite sheets, animation states, sound, checkpoints and level progression.
