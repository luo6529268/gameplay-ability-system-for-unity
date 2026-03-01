# Agent Guide (Unity / EX-GAS)
This repository is a Unity project that contains **EX Gameplay Ability System (EX-GAS)** (Unity port inspired by Unreal GAS) plus additional third-party assets.

Unity version
- Unity Editor: `2022.3.4f1c1` (from `ProjectSettings/ProjectVersion.txt`)

Key code locations
- EX-GAS runtime code: `Assets/GAS/Runtime/`
- EX-GAS editor tooling: `Assets/GAS/Editor/`
- Shared/general code: `Assets/GAS/General/`
- Assemblies (asmdefs):
  - Runtime: `Assets/GAS/Runtime/com.exhard.exgas.runtime.asmdef`
  - Editor: `Assets/GAS/Editor/com.exhard.exgas.editor.asmdef`
  - General: `Assets/GAS/General/com.exhard.exgas.general.asmdef`

Third-party dependencies / notes
- The project README states it depends on **Odin Inspector (paid)**.
- Unity packages include `com.cysharp.unitask` (UniTask) and `com.unity.test-framework`.

Notes
- Avoid changing third-party code under `Assets/Plugins/` or `Assets/NTSD/` unless necessary.

## Build / Test / Lint

There is no Node/Gradle build in this repo; builds and tests are driven by Unity.

### Open the project
- Open with Unity Hub using this folder as the project root.
- Confirm the Editor version matches `2022.3.4f1c1` (or a compatible 2022.3 LTS).

### Run tests (Unity Test Framework)

This repo includes Unity Test Framework packages, but it may not contain many actual test assemblies checked in.
Still, agents should use the standard Unity CLI patterns below.

Required: set `UNITY_EXE` to your Unity editor path.

Windows PowerShell example:
```powershell
$env:UNITY_EXE = "C:\Program Files\Unity\Hub\Editor\2022.3.4f1c1\Editor\Unity.exe"
```

Run all EditMode tests:
```powershell
& $env:UNITY_EXE -batchmode -nographics -quit `
  -projectPath "$PWD" `
  -runTests -testPlatform EditMode `
  -testResults "$PWD\TestResults-EditMode.xml" `
  -logFile "$PWD\UnityTest-EditMode.log"
```

Run all PlayMode tests:
```powershell
& $env:UNITY_EXE -batchmode -nographics -quit `
  -projectPath "$PWD" `
  -runTests -testPlatform PlayMode `
  -testResults "$PWD\TestResults-PlayMode.xml" `
  -logFile "$PWD\UnityTest-PlayMode.log"
```

Run a single test / subset:
- Use `-testFilter` with a full test name or namespace/class prefix.
```powershell
& $env:UNITY_EXE -batchmode -nographics -quit `
  -projectPath "$PWD" `
  -runTests -testPlatform EditMode `
  -testFilter "MyNamespace.MyFixture.MyTest" `
  -testResults "$PWD\TestResults-One.xml" `
  -logFile "$PWD\UnityTest-One.log"
```

If Unity returns a non-zero exit code, inspect `UnityTest-*.log` first.

### Build player (batchmode)

No canonical build script is checked in.
If you need CI builds, add an editor-only entry point (example path: `Assets/GAS/Editor/Build/BuildCli.cs`) and invoke it with Unity's `-executeMethod`.

### Lint / formatting

No repo-wide linter configuration is present (no `.editorconfig` found).
Use these defaults:
- Rely on IDE analyzers (Rider/Visual Studio) and Unity compiler warnings.
- If you add analyzers/formatters, prefer `.editorconfig` at repo root.

## Coding Conventions (C# / Unity)

These guidelines are inferred from existing EX-GAS code (e.g. `Assets/GAS/Runtime/Component/AbilitySystemComponent.cs`).

### Formatting
- Indentation: 4 spaces.
- Braces: Allman style (opening brace on new line).
- Keep methods short; split long methods into private helpers.
- Use blank lines to separate logical blocks.

### Namespaces
- Runtime code commonly uses `namespace GAS.Runtime`.
- Keep new types in the appropriate assembly folder and namespace.

### Files / folder placement
- Runtime types (used at runtime) go in `Assets/GAS/Runtime/`.
- Editor-only tooling goes in `Assets/GAS/Editor/` and must compile only in Editor.
- Prefer adding new asmdefs rather than widening existing ones, unless the change is clearly within EX-GAS.

### Naming
- Types (classes/structs/enums): `PascalCase`.
- Methods / properties: `PascalCase`.
- Local variables: `camelCase`.
- Private fields: `camelCase` (existing code also uses underscore-prefixed fields like `_ready`; follow nearby conventions in the file you touch).
- Constants: `PascalCase` (or `ALL_CAPS` only if the surrounding code already uses it).

### Imports (`using`)
- Keep `using` directives at the top of the file.
- Typical order: `System.*` -> other .NET -> `UnityEngine`/`UnityEditor` -> project namespaces.
- Remove unused usings.

### Unity patterns
- Prefer explicit lifecycle boundaries (`Awake`/`OnEnable` init, `OnDisable`/`OnDestroy` cleanup).
- Avoid allocations in per-frame code; cache collections; prefer pooling when it matters.
- Wrap editor-only behavior/logs with `#if UNITY_EDITOR`.

### Types and null-handling
- Validate public API inputs (especially Unity object references) and fail fast.
- Prefer returning `null` / empty results where existing APIs do so; do not introduce new exception behavior unless the surrounding code already throws.
- When calling into reflection/Activator patterns (existing code uses `Activator.CreateInstance`):
  - Log a clear message including the problematic type.
  - Re-throw only when it is truly unrecoverable.

### Error handling and logging
- Use `Debug.LogWarning` / `Debug.LogError` with a consistent prefix (existing code uses `[EX]`).
- Avoid spamming logs in tight loops.
- If you must suppress a warning, scope it narrowly and add a short rationale.

### Unity serialization
- Use `[SerializeField] private` for inspector-configured fields.
- Prefer properties for read-only public access (`public X Foo => foo;`).
- Do not rename serialized fields lightly; it can break existing scenes/prefabs.

### Performance notes
- README mentions GC concerns (e.g. avoiding `Type.Name` allocations); follow existing caching patterns.
- Prefer UniTask (`com.cysharp.unitask`) for async flows where applicable.

## Repo Rules Files

No Cursor rules found (`.cursorrules` / `.cursor/rules/` not present).
No GitHub Copilot instructions found (`.github/copilot-instructions.md` not present).

## Practical Agent Workflow
- Before changes: identify the relevant asmdef and ensure the type belongs to Runtime vs Editor.
- After changes:
  - Ensure Unity compiles (no console errors).
  - If tests exist, run EditMode/PlayMode via Unity Test Framework.
  - If you add a build/test CLI method, document it in this file.

## NTSD Module Structure

The `Assets/NTSD/Scripts/` directory contains the NTSD game replica code. This is the **primary active development area**.

### Key sub-directories

| Path | Purpose |
|------|---------|
| `Animation/LF2Objects/` | LF2 object runtime (LF2Character, LF2WeaponBase). **Current active work area.** Uses C# partial classes. |
| `Animation/Character/` | Per-character logic: IdUpdate, HitCounters, ItrRestTracker, CollisionGizmos |
| `Animation/LF2Tasks/` | Async task base for LF2 object operations |
| `Animation/Manager/` | CharacterAnimatorManager — manages all active character animators |
| `Animation/` (root) | Data models (LF2CharacterData), parsers, loaders, CharacterAnimator |
| `DatParser/` | Parses NTSD `.dat` files (runtime models + editor tooling) |
| `Input/` | Input system: ComboConfig, KeyEventPool, InputBase |
| `Simulation/` | Deterministic sim tick: SimContext, ISimTickable, SimInputBuffer |
| `Define/` | Shared enums/constants (CharacterState) |
| `NTSD_Extensions/` | NTSD-specific GAS extensions (stats, equipment, buff) |
| `Gen/` | **Auto-generated** GAS attribute/ability/tag libs — do not edit manually |
| `App/` | App lifecycle: AppManager, BattleBootstrap, MatchConfig |
| `Load/` | Resource loading pipeline: NTSDResourceLoader, GlobalTickDriver |
| `UI/` | All UI controllers (menus, character select, settings) |
| `Tools/` | Utility classes: ReferencePool, Log, SingletonBehaviour |
| `TimeWheel/` | Timer scheduling |
| `LevelEditor/` | Editor-only boundary wall tooling |

### Partial class pattern (LF2Character)

`LF2Character` is split across multiple files:
- `LF2Character.cs` — core class definition and main logic
- `LF2Character.Generic.partial.cs` — generic/shared behaviours
- `LF2Character.States.partial.cs` — state machine logic
- `LF2CharacterStateModule.cs` — state module helper

### Do not modify
- `Assets/NTSD/Scripts/Gen/` — auto-generated, regenerated by tooling
- `Assets/Plugins/` — third-party packages

---

## Common Pitfalls
- Unity generated files: do not commit `Library/`, `Temp/`, `Logs/`, `obj/`, `Build/` (see `.gitignore`).
- ScriptableObject/serialization: avoid renaming serialized fields; prefer adding new fields and migration paths.
