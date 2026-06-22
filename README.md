# Steel Horse Framework

A Unity toolbox providing a lightweight service-locator architecture, pooled audio SFX playback, scene loading with a loading screen, and UI helpers.

---

## Folder Structure

```
Steel Horse Framework/
├── Prefabs/
│   ├── Game Managers.prefab       ← Drop this into every scene
│   └── Services/
│       └── AudioManager.prefab    ← Child of Game Managers
└── Scripts/
    ├── GameManagers.cs
    └── Services/
        ├── ServiceLocator.cs
        ├── Audio/
        │   ├── IAudioManager.cs
        │   ├── AudioManager.cs
        │   ├── ISfxPlayer.cs
        │   ├── PooledSfxPlayer.cs
        │   ├── UiSfxPlayer.cs
        │   ├── SfxCue.cs
        │   └── SfxHandle.cs
        ├── SceneLoader/
        │   ├── ISceneLoader.cs
        │   ├── SceneLoader.cs
        │   └── LoadingTextAnimator.cs
        └── UI/
            ├── MainMenuController.cs
            └── LanguageSwitcher.cs
```

---

## Setup

### Option 1 — Unity Package (recommended)

1. Download `SteelHorseFramework-Package.unitypackage`.
2. In Unity, go to **Assets → Import Package → Custom Package…** and select the downloaded file.
3. Import all assets, then continue with step 2 below.

### Option 2 — Manual

1. Copy the `Scripts/` and `Prefabs/` folders into your Unity project's `Assets/` directory.

### After importing (both options)

1. Place the **Game Managers** prefab in your first (bootstrap) scene. It calls `DontDestroyOnLoad` and persists for the entire session, so you only need it in one scene.
2. Configure the child prefabs (see each section below).

---

## GameManagers

`Scripts/GameManagers.cs`

Singleton entry point. Holds a reference to the `ServiceLocator` and initialises all services on `Awake`.

```csharp
// Access from anywhere
GameManagers.Instance.Services.AudioManagerService.PlaySfx(cue);
GameManagers.Instance.Services.SceneLoaderService.LoadScene("GameScene");
```

The prefab hierarchy must be:

```
Game Managers  (GameManagers)
└── Services   (ServiceLocator)
    ├── AudioManager   (AudioManager + UiSfxPlayer)
    └── SceneLoader    (SceneLoader + LoadingTextAnimator [optional])
```

---

## ServiceLocator

`Scripts/Services/ServiceLocator.cs`

Resolves `IAudioManager` and `ISceneLoader` from child GameObjects via `GetComponentInChildren`. You can swap implementations without touching any caller code — just replace the component on the prefab.

---

## Audio System

### SfxCue (ScriptableObject)

`Scripts/Services/Audio/SfxCue.cs`

Create via **Assets → Create → Steel Horse → Audio → SFX Cue**.

| Field | Description |
|---|---|
| Clips | One or more `AudioClip` assets |
| Selection Mode | `Random` (no immediate repeat) or `Ordered` (sequential) |
| Looped | Loops the cue until explicitly stopped |
| Output Group | Target `AudioMixerGroup` |
| Playback Mode | `World3D` (spatialised) or `UI2D` (non-spatialised) |
| Volume Range | Random volume between min/max |
| Pitch Range | Random pitch between min/max |

### Playing and Stopping SFX

```csharp
// Play — returns a handle
SfxHandle handle = GameManagers.Instance.Services.AudioManagerService.PlaySfx(
    cue,
    parent: transform,       // optional: source follows this Transform
    position: Vector3.zero   // optional: one-shot world position
);

// Stop (works for both looped and one-shot)
GameManagers.Instance.Services.AudioManagerService.StopSfx(handle);
```

### AudioManager

`Scripts/Services/Audio/AudioManager.cs`

Routes `UI2D` cues to the **UiSfxPlayer** (single `AudioSource`, `spatialBlend = 0`) and `World3D` cues to the **PooledSfxPlayer** active in the current scene. Exposes the `AudioMixer` reference for volume/effects control.

### PooledSfxPlayer

`Scripts/Services/Audio/PooledSfxPlayer.cs`

Add this component to a GameObject in your gameplay scene. It creates a pool of `AudioSource` children at startup, registers itself with `AudioManager`, and unregisters on destroy.

- Default pool size: **24 voices** (configurable in the Inspector).
- Tracks moving parents each `Update` so spatialised sounds follow their emitter.
- Uses a generation counter so stale `SfxHandle`s can never stop a recycled voice.

### UiSfxPlayer

`Scripts/Services/Audio/UiSfxPlayer.cs`

Built into the **AudioManager** prefab. Handles all `UI2D` cues and is the fallback when no `PooledSfxPlayer` is present in the scene.

---

## Scene Loading

### SceneLoader

`Scripts/Services/SceneLoader/SceneLoader.cs`

Loads scenes asynchronously behind a full-screen loading panel.

**Inspector wiring required:**

| Field | What to assign |
|---|---|
| Loading Panel | A `CanvasGroup` that covers the screen |
| Loading Text Animator | (Optional) `LoadingTextAnimator` component |

```csharp
GameManagers.Instance.Services.SceneLoaderService.LoadScene("MainMenu");
```

### LoadingTextAnimator

`Scripts/Services/SceneLoader/LoadingTextAnimator.cs`

Cycles through an array of strings on a `TextMeshProUGUI` label at a configurable interval. Useful for animated "Loading…" dots or tips.

| Field | Description |
|---|---|
| Label | `TextMeshProUGUI` to update |
| Texts | Array of strings to cycle through |
| Delay | Seconds between each string |

---

## UI Helpers

### MainMenuController

`Scripts/UI/MainMenuController.cs`

Wire up a main menu with Play and Quit buttons. Set `Game Scene Name` in the Inspector to the scene you want to load.

```
MainMenuController
  ├── Game Scene Name  →  "GameScene"
  ├── Play Button      →  (Button reference)
  └── Quit Button      →  (Button reference)
```

### LanguageSwitcher

`Scripts/UI/LanguageSwitcher.cs`

Requires the **Unity Localization** package (`com.unity.localization`). Persists the selected language via `PlayerPrefs` and falls back to the device language on first launch.

Add a `LanguageButton` entry for each locale button in your UI:

| Field | Description |
|---|---|
| Button | UI Button reference |
| Locale | `Locale` asset from your Localization Settings |

The `Language Prefs String` key (`"SelectedLanguage"` by default) can be changed in the Inspector to avoid conflicts with other `PlayerPrefs` keys.

---

## Dependencies

| Package | Required by |
|---|---|
| Unity Localization (`com.unity.localization`) | `LanguageSwitcher` |
| TextMeshPro (`com.unity.textmeshpro`) | `LoadingTextAnimator` |
| Unity Audio Mixer | `AudioManager`, `SfxCue` |

---

## Namespaces

| Namespace | Contents |
|---|---|
| `SteelHorse.Framework` | `GameManagers` |
| `SteelHorse.Framework.Services` | `ServiceLocator` |
| `SteelHorse.Framework.Services.Audio` | All audio classes |
| `SteelHorse.Framework.Services.SceneLoading` | Scene loader classes |
| `SteelHorse.Framework.UI` | UI helpers |
