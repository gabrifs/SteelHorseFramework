# Steel Horse Framework

A Unity toolbox providing a lightweight service-locator architecture, pooled audio SFX playback, scene loading with a loading screen, a minimal REST API client, a save system, and a set of UI helpers (menu navigation, pause, player options, localization, gamepad-friendly cursor).

---

## Folder Structure

```text
Steel Horse Framework/
├── Assets/
│   ├── GameMixer.mixer                            ← Shared AudioMixer (Master/SFX/Music/...)
│   ├── Prefabs/
│   │   ├── Standard Game Managers.prefab          ← Drop this into every scene
│   │   └── Services/
│   │       └── AudioManager.prefab                ← Nested under Standard Game Managers/Services
│   └── ScriptableObjects/
│       └── Resolution Options.asset               ← Sample ResolutionOptions asset
└── Scripts/
    ├── FrameRateController.cs
    ├── GameManagers.cs
    ├── PlatformUtility.cs
    ├── Editor/
    │   ├── OpenPersistendData.cs
    │   ├── TagDatabaseEditor.cs
    │   ├── TagDatabaseLocator.cs
    │   └── TagReferenceDrawer.cs
    ├── Database/
    │   ├── Database.cs
    │   ├── DatabaseEntry.cs
    │   └── GameDatabase.cs
    ├── Tags/
    │   ├── TagDatabase.cs
    │   ├── TagDefinition.cs
    │   └── TagReference.cs
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
        │   ├── SfxSpatialSettings.cs
        │   ├── SoundConfig.cs
        │   ├── IMusicPlayer.cs
        │   ├── MusicPlayer.cs
        │   ├── MusicChannel.cs
        │   ├── MusicPlaylist.cs
        │   └── PlaylistAutoStarter.cs
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
        │   ├── LoadingTextAnimator.cs
        │   └── SkippableSceneLoader.cs
        ├── Database/
        │   ├── IDatabaseService.cs
        │   └── DatabaseService.cs
        ├── Input/
        │   ├── IInputDeviceService.cs
        │   └── InputDeviceService.cs
        ├── Options/
        │   ├── IOptionsService.cs
        │   └── OptionsService.cs
        └── UI/
            ├── DisplayOnPlatform.cs
            ├── LanguageSwitcher.cs
            ├── MenuNavigator.cs
            ├── MenuPanel.cs
            ├── PauseGame.cs
            ├── PlayerOptionsController.cs
            ├── ResolutionOptions.cs
            ├── ResolutionSetting.cs
            ├── SampleMenuController.cs
            ├── SelectionGuard.cs
            ├── SystemCursorLocker.cs
            ├── TabsMenuPanel.cs
            ├── UIButton.cs
            ├── UIDropdown.cs
            ├── UIPointer.cs
            ├── UISelectableBase.cs
            ├── UISlider.cs
            ├── UIToggle.cs
            └── VersionLabel.cs
```

---

## Setup

1. Copy the `Scripts/` and `Assets/` folders into your Unity project's `Assets/` directory.
2. Place the **Standard Game Managers** prefab in your first (bootstrap) scene. It calls `DontDestroyOnLoad` and persists for the entire session, so you only need it in one scene.
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
GameManagers.Instance.Services.DatabaseService.Get<TagDatabase>().TryGetTag("Enemy", out TagDefinition tag);
GameManagers.Instance.Services.InputDeviceService.CurrentMode; // InputDeviceMode.Pointer or .Navigation
GameManagers.Instance.Services.OptionsService.SetVolume("Master", 0.8f);
```

The prefab hierarchy is:

```text
Standard Game Managers  (GameManagers)
├── UI Canvas             (loading-screen visuals)
├── Framerate Controller  (FrameRateController)
└── Services              (ServiceLocator)
    ├── AudioManager    (nested prefab: Assets/Prefabs/Services/AudioManager.prefab — AudioManager + UiSfxPlayer)
    ├── MusicPlayer     (MusicPlayer)
    ├── SceneLoader     (SceneLoader)
    ├── Api Client      (ApiClient)
    ├── Database Service (DatabaseService)
    ├── Input Device Service (InputDeviceService)
    └── Options Service (OptionsService)
```

Game-specific singletons (e.g. a session or save-data service) should **not** be added to this prefab's own scripts — instead attach them as sibling `MonoBehaviour`s on the `Standard Game Managers` root GameObject. They inherit `DontDestroyOnLoad` from the root and manage their own `Instance` references, without coupling the Framework to game code.

---

## FrameRateController

`Scripts/FrameRateController.cs`

Drop as a sibling `MonoBehaviour` on the `Standard Game Managers` prefab root (already wired on the shipped prefab). On mobile (`PlatformUtility.IsMobilePlatform()`), sets `Application.targetFrameRate` to the device's actual refresh rate (`Screen.currentResolution.refreshRateRatio.value`) in `Awake`. Does nothing on desktop.

This exists because `QualitySettings.vSyncCount` is ignored on Android/iOS — the OS controls presentation timing there, not Unity — so without an explicit target frame rate, mobile builds render uncapped and unevenly (reads as stutter/lag) no matter what Quality Settings' vSync toggle says. Desktop already vsyncs correctly via `QualitySettings.vSyncCount`, so it's left alone here.

---

## PlatformUtility

`Scripts/PlatformUtility.cs`

Static helper with a single method, `IsMobilePlatform()` (true on `RuntimePlatform.Android`/`IPhonePlayer`). Shared by everything in the Framework that behaves differently on touch devices: `DisplayOnPlatform` (hides GameObjects per platform), `UIPointer` and `SystemCursorLocker` (both disable themselves on mobile — see below).

Reads `UnityEngine.Device.Application.platform`, not plain `UnityEngine.Application.platform` — the Device Simulator package only overrides the `UnityEngine.Device` variant, so testing mobile-only behavior via the Simulator window requires going through this API. Code that reads `Application.platform` directly always reports the real Editor platform and never appears mobile in the Simulator.

---

## ServiceLocator

`Scripts/Services/ServiceLocator.cs`

Resolves `IAudioManager`, `IMusicPlayer`, `ISceneLoader`, `IApiClient`, `IDatabaseService`, `IInputDeviceService`, and `IOptionsService` from child GameObjects via `GetComponentInChildren`. You can swap implementations without touching any caller code — just replace the component on the prefab.

Every service interface exposes a `Setup()` method, which `ServiceLocator.Setup()` calls explicitly right after resolving each service — deterministically, exactly once, only on the surviving singleton (see [GameManagers](#gamemanagers)). Services must not use `Awake`/`Start` for their own initialization: the `Standard Game Managers` prefab is placed in every scene as a duplicate-protect singleton, so `Awake`/`Start` on a service component can still run on a short-lived duplicate before `GameManagers` destroys it — `Setup()` sidesteps that entirely by only ever running through the one `ServiceLocator.Setup()` call.

---

## Audio System

### SoundConfig

`Scripts/Services/Audio/SoundConfig.cs`

Plain serializable class (not a `ScriptableObject`) pairing one `AudioClip` with the **Base Volume** it should play back at. Used as the element type of `SfxCue.Clips` and `MusicPlaylist.Songs` so each clip in a cue/playlist can have its own baseline loudness instead of sharing one fixed volume.

### SfxCue (ScriptableObject)

`Scripts/Services/Audio/SfxCue.cs`

Create via **Assets → Create → Steel Horse → Audio → SFX Cue**.

| Field | Description |
| --- | --- |
| Clips | One or more `SoundConfig` entries (`AudioClip` + Base Volume) |
| Selection Mode | `Random` (no immediate repeat) or `Ordered` (sequential) |
| Looped | Loops the cue until explicitly stopped |
| Output Group | Target `AudioMixerGroup` |
| Playback Mode | `World3D` (spatialised) or `UI2D` (non-spatialised) |
| Volume Range | Random multiplier (min/max) applied on top of the picked clip's Base Volume — final volume is `Random.Range(min, max) * clip.BaseVolume` |
| Pitch Range | Random pitch between min/max |
| 3D Sound Settings | A `SfxSpatialSettings` block (Min/Max Distance, Rolloff Mode, Custom Rolloff Curve, Spread, Doppler Level) — see below. Ignored for `UI2D` cues. |

### SfxSpatialSettings

`Scripts/Services/Audio/SfxSpatialSettings.cs`

Plain serializable class (not a `ScriptableObject`) mirroring `AudioSource`'s built-in "3D Sound Settings" inspector group, so each `World3D` `SfxCue` can control its own falloff instead of every pooled voice sharing one fixed configuration.

| Field | Description |
| --- | --- |
| Min/Max Distance | `AudioSource.minDistance`/`maxDistance` |
| Rolloff Mode | `Logarithmic`, `Linear`, or `Custom` |
| Custom Rolloff Curve | Used only when Rolloff Mode is `Custom` — applied via `AudioSource.SetCustomCurve` |
| Spread | `AudioSource.spread` (0–360) |
| Doppler Level | `AudioSource.dopplerLevel` (0–5) |

`Apply(AudioSource)` writes all of the above onto a source; `PooledSfxPlayer.Play()` calls it on every play so a shared pooled voice always matches whichever cue is currently using it.

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
| Songs | One or more `SoundConfig` entries (`AudioClip` + Base Volume) |
| Sequence Mode | `Sequential` (cycles in array order) or `Random` (no immediate repeat) |
| Fade Out Time | Seconds before a song ends when the next song starts crossfading in; also the crossfade duration used when this playlist is explicitly triggered |

### MusicPlayer

`Scripts/Services/Audio/MusicPlayer.cs`

Built into the **MusicPlayer** prefab (sibling of **AudioManager** under `Services`). Owns two `MusicChannel`s, each routed to one of the game's `AudioMixer`'s two `Music` sub-groups. Only one channel plays at a time; triggering a new playlist starts the new song on the opposite channel and crossfades between them. Auto-advances within the active playlist using the same crossfade, timed off that playlist's `Fade Out Time`.

Two independent volume knobs multiply together for the final audible level:

- **Mixer fader** (`AudioMixer.SetFloat` on the channel's exposed parameter) — drives the crossfade transition (0→1→0) and, through the mixer graph, the overall `MusicVolume`/`MasterVolume` sliders the same way `PlayerOptionsController` already drives them.
- **`AudioSource.volume`** — set directly from the playing song's `SoundConfig.BaseVolume` (never touches the mixer), so a quieter song stays quieter rather than being compensated back up by the mixer graph.

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

### PlaylistAutoStarter

`Scripts/Services/Audio/PlaylistAutoStarter.cs`

Drop on any GameObject in a scene to start a `MusicPlaylist` playing as soon as the scene loads, with no other script required. Assign the **Playlist** field; on `Start` it calls `GameManagers.Instance.Services.MusicPlayerService.Play(playlist)`. Useful for scenes (e.g. a main menu) that just need their music playing unconditionally, without a scene-specific controller.

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

### SkippableSceneLoader

`Scripts/Services/SceneLoader/SkippableSceneLoader.cs`

Drives a Timeline-based scene transition (e.g. an intro/logo scene): call the public, parameterless `LoadNextScene()` from the Timeline itself (a Signal Emitter/Receiver at its end), and/or let the player trigger it early via any of the configured skip inputs. Guarded so only the first call (Timeline end or a skip input, whichever comes first) actually loads the scene.

| Field | What to assign |
| --- | --- |
| Next Scene Name | Scene to load, passed to `ISceneLoader.LoadScene` |
| Skip Actions | `InputActionReference[]` - add/remove whichever inputs (e.g. `UI/Submit`) should skip the transition early |

---

## Networking

A minimal, dependency-free REST client built on `UnityWebRequest` and Unity's `Awaitable` async model — no external HTTP library required.

### ApiConfig (ScriptableObject)

`Scripts/Services/Networking/ApiConfig.cs`

Create via **Assets → Create → Steel Horse → Networking → Api Config**. Holds a single `BaseUrl` string that every request is resolved against. Assign it on the **Api Client** component in the `Standard Game Managers` prefab.

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

## Database System

A generic way to register and resolve game-wide data collections ("databases") at runtime, without hand-rolling a new `IXManager`/`XManager`/`ServiceLocator` property for every one. `TagDatabase` is one instance of this system; game-specific databases (portraits, factions, etc.) are others.

### Database / Database\<TEntry\> (ScriptableObject)

`Scripts/Database/Database.cs`

`Database` is a non-generic abstract marker — every concrete database asset type derives from it (directly or via `Database<TEntry>`), which is what lets `GameDatabase` hold a single heterogeneous list of them. `Database<TEntry>` adds the actual list: `[SerializeField] protected List<TEntry> _entries`, exposed as `IReadOnlyList<TEntry> Entries`. A concrete database is a small subclass supplying `TEntry` and its own `[CreateAssetMenu]`, e.g. `FactionsDatabase : Database<FactionData>`. `TEntry` can be a plain `[Serializable]` class embedded directly in the list or a `ScriptableObject` asset reference — both serialize fine through the same `List<TEntry>` field.

### DatabaseEntry (ScriptableObject) / KeyedDatabase\<TEntry\>

`Scripts/Database/DatabaseEntry.cs`, `Scripts/Database/Database.cs`

`DatabaseEntry` is an abstract `ScriptableObject` base carrying one field: a hand-authored `string Key` ("Stable identifier used to reference this entry in code and other assets. Must be unique."). `KeyedDatabase<TEntry> : Database<TEntry> where TEntry : DatabaseEntry` builds on that constraint to provide, once, what every keyed database needs: `TryGetByKey(string key, out TEntry entry)` and an editor-only `OnValidate` that warns on duplicate keys (the same "list '+' button duplicates the previous reference" hazard `TagDatabase` originally guarded against on its own). Use `KeyedDatabase<TEntry>` instead of `Database<TEntry>` whenever `TEntry` is `ScriptableObject`-based and needs code/asset-referenceable identity — a concrete subclass then shrinks to just its `[CreateAssetMenu]` and a friendly `IReadOnlyList<TEntry>` alias property:

```csharp
[CreateAssetMenu(menuName = "Steel Horse/Factions/Factions Database", fileName = "Factions Database")]
public class FactionsDatabase : KeyedDatabase<FactionData>
{
    public IReadOnlyList<FactionData> Factions { get { return Entries; } }
}
```

`Database<TEntry>` itself stays unconstrained on purpose, so it can still back a database of plain `[Serializable]` entries that have no `Key` at all — `KeyedDatabase<TEntry>` is an opt-in specialization, not a requirement.

### GameDatabase (ScriptableObject)

`Scripts/Database/GameDatabase.cs`

```csharp
[CreateAssetMenu(menuName = "Steel Horse/Database/Game Database", fileName = "Game Database")]
```

Holds every `Database` the game needs as one flat, heterogeneous `List<Database>`. Look one up by its concrete type:

```csharp
FactionsDatabase factions = myGameDatabase.Get<FactionsDatabase>();
```

Adding a new database to the game only means adding it to this list in the Inspector — no code changes to `GameDatabase`, `DatabaseService`, or `ServiceLocator` are needed.

### DatabaseService / IDatabaseService

`Scripts/Services/Database/DatabaseService.cs`, `IDatabaseService.cs`

`MonoBehaviour` implementation of `IDatabaseService` (resolved by `ServiceLocator` as `DatabaseService`). Holds a `[SerializeField] private GameDatabase _gameDatabase` pointing at whichever `GameDatabase` asset is meant to ship, assigned on the **Database Service** component in the `Standard Game Managers` prefab, and forwards `Get<TDatabase>()`/`TryGet<TDatabase>()` to it.

```csharp
if (GameManagers.Instance.Services.DatabaseService.TryGet(out TagDatabase tags) && tags.TryGetTag("Enemy", out TagDefinition tag))
    Debug.Log(tag.DisplayName.GetLocalizedString());
```

---

## Tag System

A designer-authorable tagging system: define each tag as its own asset, add it to a shared database asset, then assign it to any other asset or component via an enum-like dropdown — no free-typed strings scattered across the project.

### TagDefinition (ScriptableObject)

`Scripts/Tags/TagDefinition.cs`

```csharp
[CreateAssetMenu(menuName = "Steel Horse/Tags/Tag Definition", fileName = "New Tag Definition")]
```

One tag = one asset, deriving from `DatabaseEntry` (see [Database System](#database-system)) for its `Key` (the stable identifier used in code and for equality — must be unique), plus a `LocalizedString DisplayName` (the player-facing text, e.g. for a filter UI) and a `Color` (e.g. for tinting chips/labels in UI — one canonical color per tag, defined here rather than per-usage). `Key` is intentionally not localized, since identity must stay stable across languages. Entries in a `TagDatabase`'s list are references to these assets, not embedded data — so an unassigned list slot is `null`; `TagDatabase`/`TagReferenceDrawer` both guard against that.

### TagDatabase (ScriptableObject)

`Scripts/Tags/TagDatabase.cs`

```csharp
[CreateAssetMenu(menuName = "Steel Horse/Tags/Tag Database", fileName = "Tag Database")]
public class TagDatabase : KeyedDatabase<TagDefinition>
```

Holds a list of `TagDefinition` asset references for a project (via the [Database System](#database-system)'s `KeyedDatabase<TEntry>` base, which is where the duplicate-key `OnValidate` warning and the key lookup now live). Multiple `TagDatabase` assets can exist (testing, backups, etc.), but only one is used at a time — see **Setting the active database** below. `TryGetTag(string, out TagDefinition)` and `TryGetTag(TagReference, out TagDefinition)` are thin, Tag-flavored wrappers over the inherited `TryGetByKey`, kept for call-site readability.

```csharp
if (myDatabase.TryGetTag("Enemy", out TagDefinition tag))
    Debug.Log(tag.DisplayName.GetLocalizedString());
```

### TagReference

`Scripts/Tags/TagReference.cs`

The field type to put on other assets/components — a single tag slot, rendered as a dropdown by `TagReferenceDrawer`. Use an array/list of these to let a designer assign multiple tags:

```csharp
[SerializeField] private TagReference[] _tags;
```

Resolve one back into a `TagDefinition` at runtime through the `DatabaseService` (see above) — there is no dedicated Tag service anymore, `TagDatabase` is just another `Database` fetched by type.

### Setting the active database

`Scripts/Editor/TagDatabaseLocator.cs`, `Scripts/Editor/TagDatabaseEditor.cs`

Tag dropdowns read from a single "active" `TagDatabase`, tracked via `EditorBuildSettings`' named config-object slot (the same mechanism packages like Addressables use for singleton settings assets) rather than by searching the project — so having several `TagDatabase` assets around never causes ambiguity. Select a `TagDatabase` asset and click **Set as Active Tag Database** in its Inspector to designate it. If no database is active yet, tag dropdowns show a message prompting you to set one.

---

## Input Device Detection

### InputDeviceService

`Scripts/Services/Input/InputDeviceService.cs`, `Scripts/Services/Input/IInputDeviceService.cs`

Tracks whether the player is currently driving the UI with the mouse (`InputDeviceMode.Pointer`) or with keyboard/gamepad (`InputDeviceMode.Navigation`), switching correctly at runtime with no scene reload required. Classifies input at the raw **Input System** device level (`Mouse`/`Keyboard`/`Gamepad`, via `InputSystem.onEvent`) rather than reading a scene's `InputSystemUIInputModule` action references — so it works no matter which `InputActionAsset` a scene's `EventSystem` is configured with, and keeps tracking correctly across scene loads since it lives on the persistent `GameManagers` singleton rather than a per-scene `EventSystem`.

```csharp
GameManagers.Instance.Services.InputDeviceService.CurrentMode; // InputDeviceMode.Pointer or .Navigation
GameManagers.Instance.Services.InputDeviceService.ModeChanged += mode => { /* ... */ };
```

`UIPointer` (see below) is the primary consumer — it uses this to hide/show itself and the OS cursor in tandem.

---

## Options System

### OptionsService

`Scripts/Services/Options/OptionsService.cs`, `Scripts/Services/Options/IOptionsService.cs`

Owns every player-facing setting's persistence (`PlayerPrefs`) and system application (`AudioMixer`/`QualitySettings`/`Screen`), applying all of them once from `Setup` — called from `GameManagers.Awake` like every other service, so settings take effect on boot whether or not the player ever opens an options screen. `PlayerOptionsController` (see below) is the UI-only counterpart: it never touches `PlayerPrefs` or these systems directly, it only reads/writes through this service.

| Inspector Field | Description |
| --- | --- |
| Mixer | Target `AudioMixer` |
| Volume Channels | Array of `{ Name, MixerParameter, PrefsKey }` — `Name` is an arbitrary lookup key `PlayerOptionsController` references (e.g. `"Master"`), decoupled from the actual mixer parameter/prefs key so the UI never needs to know either |
| Quality Prefs Key | `PlayerPrefs` key for the saved quality index (default `"QualityLevel"`) |
| Resolution Options | `ResolutionOptions` asset — the curated list of selectable resolutions |
| Resolution Prefs Key | `PlayerPrefs` key for the saved resolution index (default `"ResolutionIndex"`) |

```csharp
GameManagers.Instance.Services.OptionsService.GetVolume("Master"); // 0..1 linear
GameManagers.Instance.Services.OptionsService.SetVolume("Master", 0.8f);
GameManagers.Instance.Services.OptionsService.SetQuality(2);
GameManagers.Instance.Services.OptionsService.SetResolution(0);
GameManagers.Instance.Services.OptionsService.ResolutionChanged += (width, height) => { /* ... */ };
```

**Volume timing:** applying the real saved volume as early as `Setup` (itself called from `GameManagers.Awake`) risks `AudioMixer.SetFloat` calls not being reliably audible yet — the same category of AudioMixer init-order quirk `MusicChannel`'s constructor works around by priming with silence instead of a real value (see its comment). `OptionsService` works around it by applying volumes one frame later via a coroutine started from `Setup` — safe to do there specifically, since `Setup` only ever runs once `GameManagers.Awake` has confirmed the object is the surviving singleton, not a short-lived duplicate. Quality and resolution have no such quirk and apply immediately.

**Volume drag stutter:** `SetVolume` never calls `AudioMixer.SetFloat` directly — it only records the latest pending value per channel, applied by a single `Update` pass at most once per frame. A dragged `Slider`'s `onValueChanged` can fire many times within one frame, and `AudioMixer.SetFloat` has real per-call overhead (it synchronizes with the audio DSP thread); calling it unthrottled on every tick is what caused audible/drag stutter, not the `PlayerPrefs.Save()` disk flush (already debounced separately). `GetVolume` still reads from `PlayerPrefs`, updated immediately by `SetVolume`, so it stays consistent even though the mixer apply itself is deferred.

**Resolution behavior:** the dropdown's options come from the assigned `ResolutionOptions` asset, filtered down to entries that actually fit the player's monitor (any curated preset wider or taller than the monitor's native resolution is dropped — e.g. a 2560x1440 preset is excluded on a 1920x1080 display), plus the monitor's native resolution appended automatically if it isn't already one of the remaining entries — so the player can always run at their native resolution, and always has at least that one option even if every curated preset gets filtered out. Both the filter and that fallback entry are read from `Display.main.systemWidth`/`systemHeight` — the OS-reported native resolution, not `Screen.currentResolution`, which reflects the game's own current display mode instead and would drift from the real monitor ceiling once the game applies a resolution in exclusive fullscreen. On first run (nothing saved to `PlayerPrefs` yet), that monitor resolution is adopted as the default and immediately persisted, rather than falling back to whatever the first curated entry happens to be.

**`ResolutionChanged`** only fires from `SetResolution` (a runtime change) — not during `Setup`'s boot-time apply, since that runs from `GameManagers.Awake`, before any other object's `OnEnable` has had a chance to subscribe. Read `CurrentResolution` directly for the initial value instead:

```csharp
private void OnEnable()
{
    var service = GameManagers.Instance.Services.OptionsService;
    service.ResolutionChanged += OnResolutionChanged;
    OnResolutionChanged(service.CurrentResolution.x, service.CurrentResolution.y); // initial sync
}

private void OnDisable() => GameManagers.Instance.Services.OptionsService.ResolutionChanged -= OnResolutionChanged;

private void OnResolutionChanged(int width, int height) { /* ... */ }
```

---

## UI Helpers

### MenuPanel

`Scripts/UI/MenuPanel.cs`

`[RequireComponent(typeof(CanvasGroup))]`. Represents one screen or sub-screen in a menu hierarchy and owns its own button wiring — pop/push buttons live on the panel itself, not on the navigator.

| Inspector Field | Description |
| --- | --- |
| Default Focus | `Selectable` focused when the panel is shown with no override |
| Poppable On Cancel | Whether the cancel action (gamepad B / Escape) pops this panel while it's on top of the stack |
| Always Active | Stays visible (`alpha = 1`) but non-interactive while covered by a new push, instead of hiding — see below |
| Pop Buttons | Buttons that fire `PopRequested` when clicked |
| Push Entries | Pairs of `Button Trigger` → `MenuPanel Target` that fire `PushRequested` when the trigger is clicked |
| Button Events | Pairs of `Button Button` → `UnityEvent Event`, for one-off button actions (e.g. `MenuActions.QuitApplication`) that don't need a push/pop |
| On Show / On Hide | `UnityEvent` callbacks for animations or audio |

`Show()` sets `alpha = 1`, enables interaction and raycasts, moves EventSystem focus to the default (or overridden) selectable, and fires `OnShow`. `Hide(bool covering = false)` disables interaction/raycasts and fires `OnHide`; it also zeroes `alpha` unless the panel is both **Always Active** and being hidden because a new panel was pushed on top of it (`covering: true` — that's what `MenuNavigator.Push` passes for the panel it's covering). Popping a panel always calls `Hide()` with `covering` left `false`, so an **Always Active** panel still disappears normally once it's the one being closed, rather than staying stuck on screen. Both `Show`/`Hide` are `virtual` so subclasses (e.g. `TabsMenuPanel`) can extend them. Call `Pop()` directly from script (e.g. after a successful form submission) to request a pop without a wired button.

Use **Always Active** for a panel meant to stay visible as a backdrop while things stack on top of it (e.g. a main menu behind a Settings overlay) — interaction is always disabled while covered, so clicks/navigation still go to whichever panel is actually on top.

On mobile (`PlatformUtility.IsMobilePlatform()`), `Show()` clears the EventSystem selection instead of setting focus — there's no gamepad/keyboard navigation on touch, and leaving a button auto-selected would show it highlighted despite nobody touching it. `SelectionGuard` disables itself on mobile for the same reason, so it doesn't restore the selection `Show()` just cleared.

### MenuActions

`Scripts/UI/MenuActions.cs`

Grab-bag of plain public methods meant to be wired to a `MenuPanel`'s Button Events (or any other `UnityEvent`) from the Inspector, so a panel doesn't need a bespoke controller for a single Quit or Load Scene button. Currently exposes `QuitApplication()` and `LoadScene(string sceneName)`. Coexists with `SampleMenuController` rather than replacing it.

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

Pure UI layer over [`OptionsService`](#optionsservice) (see above): wires a settings screen's Master/SFX/Music volume sliders, a quality-level dropdown, and a resolution dropdown, reading initial values from the service and forwarding player interaction back into it. Never touches `PlayerPrefs`, `AudioMixer`, `QualitySettings`, or `Screen` directly — that's all `OptionsService`'s job. Each group is independently optional: leaving a field unwired (e.g. no quality dropdown, no resolution dropdown) just skips that `Init*` call instead of erroring, so a game can opt out of any individual control.

| Inspector Field | Description |
| --- | --- |
| Master/SFX/Music Volume | Each: a `Slider` and a Channel Name matching one of `OptionsService`'s configured volume channels |
| Quality Dropdown | `TMP_Dropdown` populated from `QualitySettings.names` |
| Resolution Dropdown | `TMP_Dropdown` populated from `OptionsService.Resolutions` |

### ResolutionOptions / ResolutionSetting

`Scripts/UI/ResolutionOptions.cs`, `Scripts/UI/ResolutionSetting.cs`

`ResolutionOptions` is a `ScriptableObject` (create via **Assets → Create → Steel Horse → UI → Resolution Options**) holding an array of `ResolutionSetting` entries — each just a `Vector2` resolution plus an inspector-only display name for recognizing entries while editing the asset (the dropdown shown to players is built from the resolution values, not this name). Assign one to `PlayerOptionsController`'s **Resolution Options** field to drive its resolution dropdown.

### UISelectableBase

`Scripts/UI/UISelectableBase.cs`

`[RequireComponent(typeof(Selectable))]`. Abstract base class shared by `UIButton`, `UISlider`, `UIDropdown`, and `UIToggle`. Not used directly — implements the plumbing common to every Selectable-derived widget:

- **Select Sfx Cue** — plays through `IAudioManager.PlaySfx` on `ISelectHandler.OnSelect`, which fires for both pointer hover-to-select and gamepad/keyboard navigation. Leave empty to skip.
- **Mouse hover-to-select** — `IPointerEnterHandler.OnPointerEnter` moves EventSystem selection to the hovered widget (if interactable), so mouse hover drives the same highlight/select path as gamepad/keyboard navigation instead of only clicking. Skipped on mobile (`PlatformUtility.IsMobilePlatform()`) — touch fires `PointerEnter` alongside the tap itself, which would re-enable the auto-highlight look that `MenuPanel.Show()` and `SelectionGuard` deliberately clear on touch.

Subclasses call the protected `PlaySfx(SfxCue)` helper (a no-op if the cue is null) to play their own control-specific interaction cue. For platform-conditional visibility, add `DisplayOnPlatform` alongside it instead — it isn't part of this class.

### DisplayOnPlatform

`Scripts/UI/DisplayOnPlatform.cs`

Hides its `GameObject` (via `SetActive`) on platforms disabled in the Inspector. Two independent bools, **Desktop** and **Mobile** (both default `true`), checked against `PlatformUtility.IsMobilePlatform()` in `Awake`. Works on any `GameObject` — a single `Selectable` widget, a whole panel, a group of prompts — not just widgets derived from `UISelectableBase`. Use it for desktop/gamepad-only prompts, touch-only controls, or any element that shouldn't appear on one platform family.

### UIButton

`Scripts/UI/UIButton.cs`

`: UISelectableBase`, `[RequireComponent(typeof(Button))]`. Adds **Click Sfx Cue**, played on the `Button`'s `onClick` — which fires for both pointer clicks and gamepad/keyboard Submit, so no separate handling is needed for either input method.

### UISlider

`Scripts/UI/UISlider.cs`

`: UISelectableBase`, `[RequireComponent(typeof(Slider))]`. Adds **Value Changed Sfx Cue**, played on `Slider.onValueChanged`, rate-limited by **Value Changed Sfx Min Interval** (default `0.1`s, `Time.unscaledTime`-based so it still applies while paused) — a dragged Slider can fire `onValueChanged` many times per frame, and without the rate limit that plays the cue just as many times, layering/cutting off the same clip into an audible stutter.

### UIDropdown

`Scripts/UI/UIDropdown.cs`

`: UISelectableBase`, `[RequireComponent(typeof(TMP_Dropdown))]`. Adds **Value Changed Sfx Cue**, played on `TMP_Dropdown.onValueChanged` (fires once per option selected).

### UIToggle

`Scripts/UI/UIToggle.cs`

`: UISelectableBase`, `[RequireComponent(typeof(Toggle))]`. Adds **Value Changed Sfx Cue**, played on `Toggle.onValueChanged` (fires on both mouse and gamepad/keyboard interaction via Toggle's native input handling). Use for binary UI options (e.g. fullscreen, vsync).

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

Drop on any GameObject that stays active throughout the menu lifetime. Every `Update` it checks whether the EventSystem has lost its selection (e.g. after a button is clicked or a panel is hidden) and restores it to the last valid selectable. This keeps gamepad and keyboard navigation working without extra wiring. Disables itself on mobile (`PlatformUtility.IsMobilePlatform()`) — `MenuPanel.Show()` deliberately clears selection there, and this component would otherwise restore it on the very next frame.

### SystemCursorLocker

`Scripts/UI/SystemCursorLocker.cs`

Drop on a root GameObject in any scene that should hide and lock the OS cursor. Re-locks on application focus restore so the cursor does not stay unlocked after alt-tab. Does nothing on mobile (`PlatformUtility.IsMobilePlatform()`) — there's no OS cursor to lock there, and skipping it keeps testing in the Editor's Device Simulator unaffected.

Only ever touches `Cursor.lockState`, never `Cursor.visible` — it's a pure lock-to-window utility, independent from `UIPointer`'s mouse/keyboard-driven cursor **visibility** toggling below. The two can be combined freely.

### UIPointer

`Scripts/UI/UIPointer.cs`

Animates a `RectTransform` "cursor" sprite that smoothly follows the currently selected UI element using **DOTween**. Automatically hides when nothing is selected. Lives on its own `Canvas` and re-projects the selected element's rect through screen space, so it lines up correctly regardless of which canvas (render mode, camera, or `CanvasScaler` factor) the selected element belongs to. Disables its whole GameObject on mobile (`PlatformUtility.IsMobilePlatform()`) — this is a gamepad/keyboard-navigation affordance with no touch equivalent.

| Inspector Field | Description |
| --- | --- |
| Pointer | `RectTransform` of the cursor graphic |
| Move Duration | Tween duration in seconds (default `0.15`) |
| Hide Pointer For Mouse Input | When enabled (default), reads [`InputDeviceService`](#input-device-detection): while the player is using the mouse, this Pointer is hidden and the OS cursor (`Cursor.visible`) is shown; while navigating via keyboard/gamepad, the OS cursor is hidden and this Pointer takes over. Disable to keep the previous always-on behavior. |

Requires **DOTween** (`com.demigiant.dotween`) and, for the input-device toggle, **Unity Input System** (`com.unity.inputsystem`).

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
| Unity Localization (`com.unity.localization`) | `LanguageSwitcher`, `TagDefinition` |
| TextMeshPro (`com.unity.textmeshpro`) | `LoadingTextAnimator`, `VersionLabel` |
| Unity Audio Mixer | `AudioManager`, `SfxCue`, `OptionsService`, `MusicPlayer`, `MusicPlaylist` |
| Unity Input System (`com.unity.inputsystem`) | `MenuNavigator`, `PauseGame`, `SkippableSceneLoader`, `InputDeviceService`, `UIPointer` (Hide Pointer For Mouse Input) |
| DOTween (`com.demigiant.dotween`) | `UIPointer` |

`ApiClient` only depends on `UnityEngine.Networking` (`UnityWebRequest`), which ships with Unity — no additional package required.

---

## Namespaces

| Namespace | Contents |
| --- | --- |
| `SteelHorse.Framework` | `GameManagers`, `PlatformUtility` |
| `SteelHorse.Framework.Services` | `ServiceLocator` |
| `SteelHorse.Framework.Services.Audio` | All audio classes |
| `SteelHorse.Framework.Services.Networking` | `ApiClient`, `IApiClient`, `ApiConfig`, `ApiResponse` |
| `SteelHorse.Framework.Services.SceneLoading` | Scene loader classes, incl. `SkippableSceneLoader` |
| `SteelHorse.Framework.Services.Save` | `LocalSaveService`, `SaveEncryption` |
| `SteelHorse.Framework.Services.Database` | `DatabaseService`, `IDatabaseService` |
| `SteelHorse.Framework.Services.Input` | `InputDeviceService`, `IInputDeviceService`, `InputDeviceMode` |
| `SteelHorse.Framework.Services.Options` | `OptionsService`, `IOptionsService` |
| `SteelHorse.Framework.Database` | `Database`, `Database<TEntry>`, `DatabaseEntry`, `KeyedDatabase<TEntry>`, `GameDatabase` |
| `SteelHorse.Framework.Tags` | `TagDatabase`, `TagDefinition`, `TagReference` |
| `SteelHorse.Framework.UI` | All UI helpers |
| `SteelHorse.Framework.Editor` | Editor-only tools, incl. `TagDatabaseEditor`, `TagDatabaseLocator`, `TagReferenceDrawer` |
