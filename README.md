# FMOD-Unity Audio Manager

A Lightweight audio manager + static FMOD event and parameter containers for super easy and simple FMOD-Unity programming!


## Requirements
- Unity 2021.3+
- FMOD for Unity integration (install from https://www.fmod.com/unity or the Unity Asset Store) — the package expects the FMOD Unity integration to be installed in the project so the `FMODUnity` assembly is available. See FMOD docs for integration steps: (https://www.fmod.com/docs/)


## Installation
- In Unity: Window → Package Manager → Add package from gitURL → paste `https://github.com/tbaltaks/FMODUnityAudioManager.git`.


## How To Use
There are just *three* elements that make up the entire system:
- `AudioManager`: Responsible for all the interactions with FMOD API and contains public methods for event playback and parameter interaction.
- `FMODEvents`: Contains every event in your FMOD build as a separate variable whose name matches the name of the event.
- `FMODParameters`: Contains two enums, `LocalAudioParameter` and `GlobalAudioParameter`, each filled with values based on your FMOD build and whose names match the names of the given parameter.

These elements all exist within the `TBaltaks.FMODManagement` namespace, so make sure to add the directive `using TBaltaks.FMODManagement` for convenience. Each of the elements is also `public`, `static`, and NOT a `MonoBehaviour`, so they do not need to exist on a game object in a scene at all to function - any script from anywhere in the Unity project can access them... simply call an `AudioManager` method, passing in any arguments it may require, and away you go!

`FMODEvents` and `FMODParameters` are generated scripts to allow for automatic named code values. This means that these scripts are generated upon importing this package and regenerated whenever a change in the FMOD build is detected. 
In this way, they can be fragile - do not touch these scripts whatsoever. If there are compile errors in your Unity project upon importing this package, these scripts will not generate.
In this case, or in any event where the code generation fails, there is a manual generation button in the top bar at **Tools → FMOD Management → Regenerate FMOD Resources**. 
If this does not work, try removing all compile errors and then restarting the project.

This package builds under the **TomBaltaks.FMODUnityAudioManager** assembly definition, so make sure to add a reference to it via the `fmodunity-audio-manager.asmdef` asset for any custom assembly definitions your scripts use.


## Descriptions

### AudioManager
The `AudioManager` is a classic singleton that handles its own lifecycle and copies. 
#### Public fields
- `AudioManager Instance`: The static instance of `AudioManager`. Use `AudioManager.Instance.<SomeMethod>()` to access the public methods.
#### Public methods
- `int StartNewEvent(string eventPath)`: Starts playback on an event and returns an `int` ID for that event instance. Make sure to cache the ID in order to access that same event instance later, for example to stop the event or to set one of its local parameters. Pass in one of `FMODEvents`'s fields to play that event. This method is designed for snapshots and timeline events - for playing one shots I recommend seeing PlayOneShot(). There are a number of overloads for this method that allow spatial attenuation (i.e. attachment to a game object) by passing a reference to a game object's `Transform` or `Transform` + `Rigidbody2D`/`Rigidbody3D`.
- `void PauseEvent(int eventInstanceID)`: Pauses event playback. 
- `void UnpauseEvent(int eventInstanceID)`: Unpauses event playback.
- `void StopEvent(int eventInstanceID, bool allowFadeout = true)`: Stops and releases an event. By default, this method allows fadeout when stopping the event - pass in `false` to override this behaviour and stop event instantly. BE AWARE; once an event instance has been stopped with this method, its `int` ID is obsolete and cannot be used again.
- `void PlayOneShot(string eventPath)`: Plays and releases a oneshot type event. Use this method instead of `StartNewEvent()` for oneshots to allow FMOD to automatically handle the event instance's lifecycle. There are two separate overloads for this method; one that requires a `Vector3` position and will spatially attenuate the event on playback at that position in world space, and one that requires a `GameObject` to attach the oneshot to so the attenuation follows that object on playback.
- `void SetLocalParameter(int eventInstanceID, LocalAudioParameter parameter, float value)`: Sets the value of the given local parameter on the given event instance to the given *new* value.
- `float GetLocalParameter(int eventInstanceID, LocalAudioParameter parameter)`: Returns the current value of the given local parameter on the given event instance.
- `void SetGlobalParameter(GlobalAudioParameter parameter, float value)`: Sets the value of the given global parameter to the given *new* value.
- `float GetGlobalParameter(LocalAudioParameter parameter)`: Returns the current value of the given global parameter.

---

### FMODEvents
`FMODEvents` is simply a static class that contains a `string` field (event path) for each event it finds within your FMOD build. Use `FMODEvents.<someEvent>` to access these event paths in order to pass them into `AudioManager` methods.

---

### FMODParameters
The `FMODParameters` file simply contains two public enums used as arguments to `AudioManager`'s parameter setting and getting methods.
- `LocalAudioParameter`: Enum whose values correlate to each locally scoped parameter it finds within your FMOD build. Use `LocalAudioParameter.<SomeParameter>` to access these values.
- `GlobalAudioParameter`: Enum whose values correlate to each globally scoped parameter it finds within your FMOD build. Use `GlobalAudioParameter.<SomeParameter>` to access these values.

---

Hopefully that is all the information you need to use the package. Happy audio implementing!
