# Steel Horse Framework

A Unity toolbox providing a lightweight service-locator architecture, pooled audio SFX playback, scene loading with a loading screen, a minimal REST API client, a save system, and a set of UI helpers (menu navigation, pause, player options, localization, gamepad-friendly cursor).

---

## Folder Structure

```text
Steel Horse Framework/
├── Prefabs/
│   ├── Game Managers.prefab       ← Drop this into every scene
│   └── Services/
│       ├── AudioManager.prefab    ← Nested under Game Managers/Services
│       └── MusicPlayer.prefab     ← Nested under Game Managers/Services
└── Scripts/
    ├── GameManagers.cs
    ├── Editor/
    │   └── OpenPersistendData.cs
    └── Services/
        ├── ServiceLocator.cs
        ├── Audio/
        │   ├── IAudioManager.cs
        │   ├── AudioManager.cs
        │   ├── ISfxPlayer.cs
        │   ├── PooledSfxPlayer.cs
        │   ├── UiSfxPlayer.cs
        │   ├── SfxCue.cs
        │   ├── SfxHandle.cs
        │   ├── IMusicPlayer.cs
        │   ├── MusicPlayer.cs
        │   ├── MusicChannel.cs
        │   └── MusicPlaylist.cs
        ├── Networking/
        │   ├── IApiClient.cs
        │   ├── ApiClient.cs
        │   ├── ApiConfig.cs
        │   └── ApiResponse.cs
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
            ├── PauseGame.cs
            ├── PlayerOptionsController.cs
            ├── SampleMenuController.cs
            ├── SelectionGuard.cs
            ├── SystemCursorLocker.cs
            ├── TabsMenuPanel.cs
            ├── UIButton.cs
            ├── UIPointer.cs
            └── VersionLabel.cs
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
GameManagers.Instance.Services.MusicPlayerService.Play(playlist);
GameManagers.Instance.Services.SceneLoaderService.LoadScene("GameScene");
GameManagers.Instance.Services.ApiClientService.GetAsync("/api/v1/status");
```

The prefab hierarchy is:

```text
Game Managers  (GameManagers)
├── UI Canvas          (loading-screen visuals)
└── Services           (ServiceLocator)
    ├── AudioManager    (AudioManager + UiSfxPlayer)
    ├── MusicPlayer     (MusicPlayer)
    ├── SceneLoader     (SceneLoader)
    └── Api Client      (ApiClient)
```

Game-specific singletons (e.g. a session or save-data service) should **not** be added to this prefab's own scripts — instead attach them as sibling `MonoBehaviour`s on the `Game Managers` root GameObject. They inherit `DontDestroyOnLoad` from the root and manage their own `Instance` references, without coupling the Framework to game code.

---

## ServiceLocator

`Scripts/Services/ServiceLocator.cs`

Resolves `IAudioManager`, `IMusicPlayer`, `ISceneLoader`, and `IApiClient` from child GameObjects via `GetComponentInChildren`. You can swap implementations without touching any caller code — just replace the component on the prefab.

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

## Music System

### MusicPlaylist (ScriptableObject)

`Scripts/Services/Audio/MusicPlaylist.cs`

Create via **Assets → Create → Steel Horse → Audio → Music Playlist**.

| Field | Description |
| --- | --- |
| Songs | One or more `AudioClip` assets |
| Sequence Mode | `Sequential` (cycles in array order) or `Random` (no immediate repeat) |
| Fade Out Time | Seconds before a song ends when the next song starts crossfading in; also the crossfade duration used when this playlist is explicitly triggered |

### MusicPlayer

`Scripts/Services/Audio/MusicPlayer.cs`

Built into the **MusicPlayer** prefab (sibling of **AudioManager** under `Services`). Owns two `MusicChannel`s, each routed to one of the game's `AudioMixer`'s two `Music` sub-groups. Only one channel plays at a time; triggering a new playlist starts the new song on the opposite channel and crossfades between them. Auto-advances within the active playlist using the same crossfade, timed off that playlist's `Fade Out Time`.

Channel volume is driven entirely through the mixer's exposed per-channel parameters (not `AudioSource.volume`), so all audible-level control — the overall music slider and the per-channel crossfade alike — lives in the mixer graph, the same way `PlayerOptionsController` already drives `MasterVolume`/`MusicVolume`/`SfxVolume`.

**Inspector wiring required:**

| Field | What to assign |
| --- | --- |
| Mixer | The game's `AudioMixer` |
| Channel A Group | An `AudioMixerGroup` under the mixer's `Music` group (e.g. `Music Ch1`) |
| Channel B Group | The other `Music` sub-group (e.g. `Music Ch2`) |
| Channel A/B Volume Parameter | Names of two float parameters exposed on the mixer (defaults: `MusicCh1Volume`/`MusicCh2Volume`) — see below |

The target `AudioMixer` must have each channel group's Volume exposed to script (right-click the group's Volume fader → **Expose 'Volume (of \<Group\>)' to script**, then rename it under the mixer's **Exposed Parameters** view) so `MusicPlayer` can fade it via `AudioMixer.SetFloat`.

```csharp
// Play — starts instantly (no fade) if nothing is currently playing,
// otherwise crossfades from whatever is currently active. A no-op if
// this playlist is already the one playing.
GameManagers.Instance.Services.MusicPlayerService.Play(playlist);

// Stop — fades the active channel out over the playlist's own Fade Out
// Time by default; pass 0f for an instant stop.
GameManagers.Instance.Services.MusicPlayerService.Stop();
```

Overall music volume is controlled independently via the mixer's exposed `MusicVolume` parameter (see `PlayerOptionsController`) — both `Music Ch1` and `Music Ch2` inherit it as children of `Music`, so no additional volume wiring is needed there.

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

## Networking

A minimal, dependency-free REST client built on `UnityWebRequest` and Unity's `Awaitable` async model — no external HTTP library required.

### ApiConfig (ScriptableObject)

`Scripts/Services/Networking/ApiConfig.cs`

Create via **Assets → Create → Steel Horse → Networking → Api Config**. Holds a single `BaseUrl` string that every request is resolved against. Assign it on the **Api Client** component in the `Game Managers` prefab.

### ApiClient / IApiClient

`Scripts/Services/Networking/ApiClient.cs`, `IApiClient.cs`

`MonoBehaviour` implementation of `IApiClient` (resolved by `ServiceLocator` as `ApiClientService`). Supports `GET`/`POST`/`PUT`/`DELETE`, optional per-request headers, and `CancellationToken` cancellation (aborts the underlying `UnityWebRequest`).

```csharp
ApiResponse response = await GameManagers.Instance.Services.ApiClientService.PostAsync(
    "/api/v1/matches",
    jsonBody,
    cancellationToken: token
);

if (response.Success)
{
    var result = response.ParseAs<MatchResultDto>(); // JsonUtility under the hood
}
else
{
    Debug.LogWarning($"{response.StatusCode}: {response.ErrorMessage}");
}
```

### ApiResponse

`Scripts/Services/Networking/ApiResponse.cs`

Immutable result wrapper — `Success`, `StatusCode`, `RawBody`, `ErrorMessage`, and a generic `ParseAs<T>()` helper (returns `null` and logs a warning on parse failure rather than throwing). On an HTTP error status the response body is still preserved on `RawBody` in case the server returned a JSON error payload.

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

`[RequireComponent(typeof(CanvasGroup))]`. Represents one screen or sub-screen in a menu hierarchy and owns its own button wiring — pop/push buttons live on the panel itself, not on the navigator.

| Inspector Field | Description |
| --- | --- |
| Default Focus | `Selectable` focused when the panel is shown with no override |
| Poppable On Cancel | Whether the cancel action (gamepad B / Escape) pops this panel while it's on top of the stack |
| Pop Buttons | Buttons that fire `PopRequested` when clicked |
| Push Entries | Pairs of `Button Trigger` → `MenuPanel Target` that fire `PushRequested` when the trigger is clicked |
| On Show / On Hide | `UnityEvent` callbacks for animations or audio |

`Show()` sets `alpha = 1`, enables interaction and raycasts, moves EventSystem focus to the default (or overridden) selectable, and fires `OnShow`. `Hide()` does the opposite and fires `OnHide`. Both are `virtual` so subclasses (e.g. `TabsMenuPanel`) can extend them. Call `Pop()` directly from script (e.g. after a successful form submission) to request a pop without a wired button.

### MenuNavigator

`Scripts/UI/MenuNavigator.cs`

Stack-based menu controller with no knowledge of game state — it just tracks which `MenuPanel` is on top. Subscribes to a panel's `PopRequested`/`PushRequested` events when it enters the stack and unsubscribes when it leaves.

| Inspector Field | Description |
| --- | --- |
| Root Panel | First `MenuPanel` pushed on `Awake`; bottom of the stack |

```csharp
// Navigate programmatically
menuNavigator.Push(settingsPanel, returnFocusOnPop: settingsButton);
menuNavigator.Pop();
menuNavigator.PopToRoot();
```

`Pop()` is a no-op when only the root frame remains, so the stack can never be emptied. The cancel action is read from `InputSystemUIInputModule.cancel.action` (resolved from `EventSystem.current` in `Start`) so it works with any binding the project defines for "cancel" (gamepad B, keyboard Escape, etc.), and only pops when the top panel's `Poppable On Cancel` is true.

### TabsMenuPanel

`Scripts/UI/TabsMenuPanel.cs`

Derives from `MenuPanel`. Manages an ordered list of tab buttons paired with content `CanvasGroup`s — one tab's content is shown at a time via `alpha`/`interactable`/`blocksRaycasts` (not `SetActive`, so `Update`/coroutines keep running on inactive tabs). Use this instead of pushing/popping through the `MenuNavigator` for tab-style screens (tabs replace each other rather than stack).

| Inspector Field | Description |
| --- | --- |
| Tabs | List of `Button TabButton` → `CanvasGroup Content` pairs |
| Default Tab Index | Which tab is active when the panel first opens (default `0`) |
| Prev/Next Tab Button | Optional buttons that cycle tabs, wrapping around |

The last-selected tab persists across hide/show cycles (`Show()` re-selects `_currentTabIndex`). To always reopen on the first tab instead, reset the index from an `On Hide` UnityEvent.

### PauseGame

`Scripts/UI/PauseGame.cs`

Pushes a `MenuPanel` onto a `MenuNavigator` in response to a pause input action, freezing the game (`Time.timeScale = 0`, `AudioListener.pause = true`) while it's up.

| Inspector Field | Description |
| --- | --- |
| Navigator | `MenuNavigator` to push the pause panel onto |
| Pause Panel | `MenuPanel` shown while paused |
| Pause Action Reference | `InputActionReference` that triggers `Pause()` |
| Resume Button / Quit Button | Optional buttons wired to `Resume()` / quit-to-scene |
| Quit Scene Name | Scene loaded via `SceneLoaderService` when Quit is clicked |

```csharp
if (PauseGame.IsPaused) { /* ... */ }

// Prevent pausing while a blocking screen (results, a cutscene, etc.) is up —
// PauseGame itself stays ignorant of what that screen is.
PauseGame.IsPauseBlocked = true;
```

`Pause()` is a no-op while already paused or while `IsPauseBlocked` is `true`. `Resume()` resets time scale/audio and clears the navigator stack.

### PlayerOptionsController

`Scripts/UI/PlayerOptionsController.cs`

Wires a settings screen's Master/SFX/Music volume sliders to an `AudioMixer` (via exposed float parameters, linear-to-decibel converted) and a quality-level dropdown to `QualitySettings`. Both are persisted to `PlayerPrefs` and restored on `Awake`.

| Inspector Field | Description |
| --- | --- |
| Mixer | Target `AudioMixer` |
| Master/SFX/Music Volume | Each: a `Slider`, the mixer's exposed parameter name, and a `PlayerPrefs` key |
| Quality Dropdown | `TMP_Dropdown` populated from `QualitySettings.names` |
| Quality Prefs Key | `PlayerPrefs` key for the saved quality index (default `"quality_level"`) |

### UIButton

`Scripts/UI/UIButton.cs`

`[RequireComponent(typeof(CanvasGroup))]`. Set **Mobile Button** to hide (alpha/interactable/raycasts) a button on non-mobile platforms — useful for touch-only controls that shouldn't appear on desktop.

### VersionLabel

`Scripts/UI/VersionLabel.cs`

Sets a `TMP_Text` label to `Application.version` on `Awake`. Drop on a build/version label anywhere in a menu scene.

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

Animates a `RectTransform` "cursor" sprite that smoothly follows the currently selected UI element using **DOTween**. Automatically hides when nothing is selected. Lives on its own `Canvas` and re-projects the selected element's rect through screen space, so it lines up correctly regardless of which canvas (render mode, camera, or `CanvasScaler` factor) the selected element belongs to.

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

### OpenPersistendData

`Scripts/Editor/OpenPersistendData.cs`

Adds **Tools → Steel Horse → Open Persistent Data Path** to the Unity menu bar. Opens the folder where `LocalSaveService` writes save files, making it easy to inspect or delete saves during development.

---

## Dependencies

| Package | Required by |
| --- | --- |
| Unity Localization (`com.unity.localization`) | `LanguageSwitcher` |
| TextMeshPro (`com.unity.textmeshpro`) | `LoadingTextAnimator`, `VersionLabel` |
| Unity Audio Mixer | `AudioManager`, `SfxCue`, `PlayerOptionsController`, `MusicPlayer`, `MusicPlaylist` |
| Unity Input System (`com.unity.inputsystem`) | `MenuNavigator`, `PauseGame` |
| DOTween (`com.demigiant.dotween`) | `UIPointer` |

`ApiClient` only depends on `UnityEngine.Networking` (`UnityWebRequest`), which ships with Unity — no additional package required.

---

## Namespaces

| Namespace | Contents |
| --- | --- |
| `SteelHorse.Framework` | `GameManagers` |
| `SteelHorse.Framework.Services` | `ServiceLocator` |
| `SteelHorse.Framework.Services.Audio` | All audio classes |
| `SteelHorse.Framework.Services.Networking` | `ApiClient`, `IApiClient`, `ApiConfig`, `ApiResponse` |
| `SteelHorse.Framework.Services.SceneLoading` | Scene loader classes |
| `SteelHorse.Framework.Services.Save` | `LocalSaveService`, `SaveEncryption` |
| `SteelHorse.Framework.UI` | All UI helpers |
| `SteelHorse.Framework.Editor` | Editor-only tools |
