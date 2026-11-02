# Magnet Fishing

## Project Overview

Magnet Fishing is a polished 2D mobile arcade game built in Unity 6, designed to be played in landscape orientation.

The core gameplay is inspired by the core timing and retrieval loop of Gold Miner, adapted into a magnet-fishing game set in a river.

The game should have its own visual identity, objects, progression, and magnet-fishing mechanics, and should not be treated as a direct clone.

Current development scope:
1. Main Menu
2. Level 1
3. Core magnet and rope gameplay
4. Basic collectible objects
5. Timer, score, win and lose states
6. Polish after the gameplay loop is stable

Do not implement future systems such as shops, upgrades, multiple levels, progression, achievements, or online systems unless explicitly requested.

## Platform

- Primary platform: mobile.
- Designed to be played in landscape orientation.
- Touch is the primary input method.
- Keep keyboard/mouse support only as a development/testing fallback in the Unity Editor when useful.
- Use Unity's new Input System. Do not revert the project to the legacy Input Manager.
- Unity 6
- 2D project
- Universal Render Pipeline / Universal 2D

All menus and HUD elements must be touch-friendly.

UI must be responsive across different landscape mobile aspect ratios.

Important UI elements must respect mobile safe areas/notches.

Use appropriate anchors and Canvas Scaler configuration rather than relying on fixed pixel positions.

Do not design gameplay or UI around keyboard navigation as a primary requirement.

## Current Scenes

- Assets/Scenes/MainMenu.unity
- Assets/Scenes/Level01.unity

## Project Structure

Use the existing folders:

Assets/
- _Game/
  - Art/
  - Audio/
  - Materials/
  - Prefabs/
  - Scripts/
  - UI/
  - VFX/
- Scenes/

Do not create duplicate folders such as another Scenes or Scripts folder unless necessary.

## Core Gameplay

The player character stands above the river and operates a magnet attached to a rope.

When idle:
- The magnet/rope aiming direction continuously swings left and right.
- The movement should be smooth and predictable.

Input:
- The intended mobile interaction is a screen tap to launch the magnet at its current angle.
- SPACE may remain as an Editor/desktop testing fallback, but it is not the primary player control.
- ENTER is NOT used for launching.

When the launch input is triggered:
1. Lock the current aiming direction.
2. Extend the magnet and rope in that direction.
3. The magnet continues until it hits the first valid catchable target or reaches its maximum distance.
4. If it catches a target, attach the target.
5. Retract the magnet toward the player.
6. The caught object is pulled back with the magnet.
7. When retrieval completes, award the object's value and reset the magnet to its idle swinging state.

Object weight should affect retrieval speed:
- Light objects return faster.
- Heavy objects return slower.

## Level 1 Theme

The scene takes place inside a river.

The underwater area should eventually feel alive through:
- Water background
- Riverbed
- Plants
- Bubbles
- Particles
- Fish movement
- Lighting and depth
- Environmental detail

Visual polish comes after the basic gameplay is working.

## Catchable Objects

Level 1 can include:

- Fish
- Bicycle
- Metal scrap
- Old metal objects
- Old weapons / weapon-like metal junk
- Gold or jewelry attached to metal
- Treasure chest with metallic parts

Do not use ordinary rocks as standard catchable objects.

### Fish

Fish remain part of the game.

A fish is not attracted magnetically by its body.

Each catchable fish has a visible metallic fishing hook in or near its mouth.

The magnet catches the metal hook.

Fish should move through the river, making them moving targets.

## Valuable Objects

Diamonds should normally be represented as part of a metallic item such as:
- Ring
- Necklace
- Jewelry
- Metal jewelry box

Avoid a free-floating diamond being directly attracted to the magnet.

## Game Values

Exact values are not finalized yet.

Do not invent permanent values for:
- Level duration
- Target score
- Item prices
- Item weights
- Magnet speed
- Rope length
- Swing angle

Use clearly exposed serialized tuning variables or temporary values when required.

## Development Rules

Work in small, testable increments.

Before implementing a feature:
1. Inspect the current project and relevant scripts.
2. Avoid changing unrelated systems.
3. Prefer the simplest architecture that supports the current requirement.

After implementing a feature:
1. Wait for Unity compilation to finish.
2. Check the Unity Console for compile errors and exceptions.
3. Verify the relevant GameObjects/components through the live Unity Editor connection.
4. Run Play Mode when the feature requires runtime verification.
5. Report exactly what was changed and what was tested.

Do not declare a task complete only because code was written.

## Unity Editor Rules

Prefer live Unity Editor / Unity CLI / Pipeline tools for:
- Creating GameObjects
- Adding or removing Components
- Editing scenes
- Creating prefabs
- Inspecting hierarchy
- Verifying serialized references

Do not manually edit .unity scene YAML or prefab YAML unless there is a specific reason.

Do not install or remove Unity packages without explicit approval.

Do not modify ProjectSettings unless required for the current task.

## Code Rules

- Use C#.
- Keep scripts focused and reasonably small.
- Prefer composition over large manager classes.
- Use clear descriptive names.
- Use `[SerializeField] private` for Inspector configuration where appropriate.
- Avoid hard-coded scene object searches when references can be serialized.
- Avoid unnecessary singletons.
- Do not introduce third-party frameworks unless explicitly approved.
- Add comments only where they explain non-obvious behavior.

## Git Rules

The project is under Git.

Do not:
- Delete unrelated files.
- Rewrite large areas unnecessarily.
- Commit automatically unless explicitly asked.
- Push automatically unless explicitly asked.

Keep each implementation step small enough to review and revert.

## Current Development Priority

The immediate priority is:

1. Establish project architecture.
2. Build Main Menu.
3. Build the basic Level 1 environment.
4. Implement the magnet swing.
5. Launch magnet with SPACE.
6. Extend and retract the rope.
7. Catch one simple test object.
8. Only then expand to real Level 1 objects and fish.