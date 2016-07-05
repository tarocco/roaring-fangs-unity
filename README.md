Roaring Fangs Multipurpose Library for Unity
=====
License
-----
MIT License

Full Documentation
-----
Check out the [Wiki](https://github.com/Tarocco/roaring-fangs-unity/wiki) for documentation, examples and guides.

Features Overview (why aren't you using it yet?)
-----

- Various improvements and fixes to the Unity animation editor (renaming paths, updating with hierarchy changes)
- `[AutoProperty]` lets you write _full_ properties that work with the inspector. Use it on your backing fields!
- `[Layer]` lets you use the editor dropdown to specify a _single_ layer `int` (unlike `UnityEngine.LayerMask`)
- Highly customizeable audio envelope follower that lets you make sound visualizers, etc.
- Abstraction of `UnityEvent` methods for editor and player
- Simple input re-mapping controller
- Non-canvas mesh 9-patch controller
- Object pooling with a circular buffer and TTL `CBPool`
- Hexidecimal color parsing and serialization
- Under/critically/over-damped motion controller `BouncyMove`
- Post-processing utilities
- Editor utilities and helpers like `EditorHelper.HierarchyObjectPathChanged`
- Physical camera scripts (some features require [FluffyUnderware Curvy](http://fluffyunderware.com/curvy/start))
- MIDI format parsing library by Cristoph Fabritz with additional helpers
- Includes JsonFx 2 assembly (may be removed in future releases)
- Various backports from newer versions of Mono `Mono Supplement`
- Math functions
- Misc scene and coroutine helpers
- Makes toast (coming soon, maybe)

Installing
-----
You can download this repository and copy it into your Unity project manually or use this repository as a git subproject.

### As a Git Subtree
Optionally fork your own copy before you start.

1. Commit or stash any changes before adding.
2. Add roaring-fangs-unity as a subtree

  `git subtree add --prefix Assets/Packages/RoaringFangs git@github.com:Tarocco/roaring-fangs-unity.git master`
  
3. Add `ROARING_FANGS_UNITY` to the Scripting Define Symbols in your Unity project. Separate define symbols with semicolons.

  Select
  
  `Edit > Project Settings > Player`
  
  then
  
  `Inspector > [Platform Settings] > Other Settings > Scripting Define Symbols`
  
  where `[Platform Settings]` is any and every platform you are targeting. 
  
  This define symbol can be used to disable polyfills in libraries that copy bits and pieces of roaring-fangs-unity independently under the same namespace by using `#if`/`#endif`.

Building Dependencies (optional)
-----
This library depends on projects in the `/.Projects` directory. First and foremost, build them, or you can just rely on the prebuilt assemblies in `/Assemblies`. The choice is yours. There is a `Libraries.sln` solution that you can use to build the projects. If you update or modify the libraries in `/.Projects`, you will need to rebuild them as Unity will not build their source files. Instead, they are built separately and dynamically linked.

Contributing
-----
Please contact me, @Tarocco, about questions on general usage and contributing.

🐰🐇
