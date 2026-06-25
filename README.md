# Steel Horse Framework

A Unity toolbox providing a lightweight service-locator architecture, pooled audio SFX playback, scene loading with a loading screen, a save system, and UI helpers.

---

## Folder Structure

```text
Steel Horse Framework/
├── Prefabs/
│   ├── Game Managers.prefab       ← Drop this into every scene
│   └── Services/
│       └── AudioManager.prefab    ← Child of Game Managers
└── Scripts/
    ├── GameManagers.cs
    ├── Editor/
    │   └── OpenPersistentData.cs
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
        ├── Save/
        │   ├── LocalSaveService.cs
        │   └── SaveEncryption.cs
        ├── SceneLoader/
        │   ├── ISceneLoader.cs
        │   ├── SceneLoader.cs
        │   └── LoadingTextAnimator.cs
        └── UI/
            ├── LanguageSwitcher.cs
            ├── MenuNavigator.cs
            ├── MenuPanel.cs
            ├── SampleMenuController.cs
            ├── SelectionGuard.cs
            ├── SystemCursorLocker.cs
            └── UIPointer.cs
```

---

## Setup

1. Copy the `Scripts/` and `Prefabs/` folders into your Unity project's `Assets/` directory.
2. Place the **Game Managers** prefab in your first (bootstrap) scene. It calls `DontDestroyOnLoad` and persists for the entire session, so you only need it in one scene.
3. Configure the child prefabs (see each section below).

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

```text
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
| --- | --- |
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

Routes `UI2D` cues to the **UiSfxPlayer** (single `AudioSource`, `spatialBlend = 0`) and `World3D` cues to the **PooledSfxPlayer** active in the current scene. Falls back to `UiSfxPlayer` when no `PooledSfxPlayer` is registered. Exposes the `AudioMixer` reference for volume/effects control.

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

Loads scenes asynchronously behind a full-screen loading panel with a configurable crossfade.

**Inspector wiring required:**

| Field | What to assign |
| --- | --- |
| Loading Panel | A `CanvasGroup` that covers the screen |
| Loading Text Animator | (Optional) `LoadingTextAnimator` component |
| Fade Duration | Seconds for the fade in/out (default `0.3`) |

Before loading the new scene the loader calls `Resources.UnloadUnusedAssets()` and `GC.Collect()` to avoid both scenes being resident in memory simultaneously.

```csharp
GameManagers.Instance.Services.SceneLoaderService.LoadScene("MainMenu");
```

### LoadingTextAnimator

`Scripts/Services/SceneLoader/LoadingTextAnimator.cs`

Cycles through an array of strings on a `TextMeshProUGUI` label at a configurable interval. Useful for animated "Loading…" dots or tips.

| Field | Description |
| --- | --- |
| Label | `TextMeshProUGUI` to update |
| Texts | Array of strings to cycle through |
| Delay | Seconds between each string |

---

## Save System

### LocalSaveService\<T\>

`Scripts/Services/Save/LocalSaveService.cs`

Generic static helper that saves and loads a single data object to `Application.persistentDataPath` as an AES-encrypted JSON file.

```csharp
// Define your save data class
[Serializable]
public class SaveData
{
    public int HighScore;
    public float MusicVolume = 1f;
}

// Load (lazy — also called automatically on first access)
LocalSaveService<SaveData>.Load();

// Read
int score = LocalSaveService<SaveData>.Current.HighScore;

// Mutate and persist
LocalSaveService<SaveData>.Current.HighScore = 9001;
LocalSaveService<SaveData>.Save();
```

Both `Load` and `Save` accept an optional `fileName` parameter (default `"save.json"`). Use different file names to maintain multiple independent save slots.

If the file is missing or corrupt, `Load` logs a warning and falls back to a default-constructed `T`.

### SaveEncryption

`Scripts/Services/Save/SaveEncryption.cs`

AES-CBC encryption layer used internally by `LocalSaveService`. Each save generates a fresh IV so identical data encrypts differently every time. The file format is `Base64( IV[16 bytes] || ciphertext )`.

You can call `SaveEncryption.Encrypt` / `SaveEncryption.Decrypt` directly if you need to encrypt data outside of `LocalSaveService`.

---

## UI Helpers

### MenuPanel

`Scripts/UI/MenuPanel.cs`

Requires a `CanvasGroup` on the same GameObject. Represents one screen or sub-screen in a menu hierarchy.

| Inspector Field | Description |
| --- | --- |
| Default Focus | `Selectable` to focus when the panel is shown |
| Poppable On Cancel | Whether the cancel action (gamepad B / Escape) pops this panel |
| On Show / On Hide | `UnityEvent` callbacks for animations or audio |

`Show()` sets `alpha = 1`, enables interaction and raycasts, and moves EventSystem focus to the default (or overridden) selectable. `Hide()` does the opposite.

### MenuNavigator

`Scripts/UI/MenuNavigator.cs`

Stack-based menu controller. Wire buttons to push sub-panels and pop back to the parent, with automatic focus management and gamepad cancel support.

| Inspector Field | Description |
| --- | --- |
| Root Panel | First `MenuPanel` pushed on `Awake` |
| Push Entries | Pairs of `Button` → `MenuPanel` to navigate forward |
| Pop Buttons | Buttons that call `Pop()` |

```csharp
// Navigate programmatically
menuNavigator.Push(settingsPanel);
menuNavigator.Pop();
menuNavigator.PopToRoot();
```

The cancel action is read from `InputSystemUIInputModule` so it works with any binding the project defines for "cancel" (gamepad B, keyboard Escape, etc.).

### SampleMenuController

`Scripts/UI/SampleMenuController.cs`

Minimal example controller for a main-menu scene. Wire up a Play button (loads a scene via `SceneLoaderService`) and a Quit button (`Application.Quit`). Use this as a starting point rather than a production component.

| Inspector Field | Description |
| --- | --- |
| Game Scene Name | Scene to load when Play is pressed |
| Play Button | `Button` reference |
| Quit Button | `Button` reference |

### SelectionGuard

`Scripts/UI/SelectionGuard.cs`

Drop on any GameObject that stays active throughout the menu lifetime. Every `Update` it checks whether the EventSystem has lost its selection (e.g. after a button is clicked or a panel is hidden) and restores it to the last valid selectable. This keeps gamepad and keyboard navigation working without extra wiring.

### SystemCursorLocker

`Scripts/UI/SystemCursorLocker.cs`

Drop on a root GameObject in any scene that should hide and lock the OS cursor. Re-locks on application focus restore so the cursor does not stay unlocked after alt-tab.

### UIPointer

`Scripts/UI/UIPointer.cs`

Animates a `RectTransform` "cursor" sprite that smoothly follows the currently selected UI element using **DOTween**. Automatically hides when nothing is selected.

| Inspector Field | Description |
| --- | --- |
| Pointer | `RectTransform` of the cursor graphic |
| Move Duration | Tween duration in seconds (default `0.15`) |

Requires **DOTween** (`com.demigiant.dotween`).

### LanguageSwitcher

`Scripts/UI/LanguageSwitcher.cs`

Requires the **Unity Localization** package (`com.unity.localization`). Persists the selected language via `PlayerPrefs` and falls back to the device language on first launch, then to the first available locale if the device language is not in the project's locale list.

Add a `LanguageButton` entry for each locale button in your UI:

| Field | Description |
| --- | --- |
| Button | UI Button reference |
| Locale | `Locale` asset from your Localization Settings |

The `Language Prefs String` key (`"SelectedLanguage"` by default) can be changed in the Inspector to avoid conflicts with other `PlayerPrefs` keys.

---

## Editor Tools

### OpenPersistentData

`Scripts/Editor/OpenPersistentData.cs`

Adds **Tools → Steel Horse → Open Persistent Data Path** to the Unity menu bar. Opens the folder where `LocalSaveService` writes save files, making it easy to inspect or delete saves during development.

---

## Dependencies

| Package | Required by |
| --- | --- |
| Unity Localization (`com.unity.localization`) | `LanguageSwitcher` |
| TextMeshPro (`com.unity.textmeshpro`) | `LoadingTextAnimator` |
| Unity Audio Mixer | `AudioManager`, `SfxCue` |
| Unity Input System (`com.unity.inputsystem`) | `MenuNavigator` |
| DOTween (`com.demigiant.dotween`) | `UIPointer` |

---

## Namespaces

| Namespace | Contents |
| --- | --- |
| `SteelHorse.Framework` | `GameManagers` |
| `SteelHorse.Framework.Services` | `ServiceLocator` |
| `SteelHorse.Framework.Services.Audio` | All audio classes |
| `SteelHorse.Framework.Services.SceneLoading` | Scene loader classes |
| `SteelHorse.Framework.Services.Save` | `LocalSaveService`, `SaveEncryption` |
| `SteelHorse.Framework.UI` | All UI helpers |
| `SteelHorse.Framework.Editor` | Editor-only tools |
