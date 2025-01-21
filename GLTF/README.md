# IL2CPP GLTF
## What?
A modified version of the [KhronosGroup GLTF Unity Plugin](https://github.com/KhronosGroup/UnityGLTF) that works with IL2CPP and stripped editor code.  

## Why?
The motivation behind it is to make GLTF a good alternative to asset bundles for anything that doesn't require custom shaders.  
The project that makes use of this plugin should be able to provide all custom shaders the game uses as well as handlers for how to interpret custom metadata in the GLTF file in order to build model hierarchies that may include script components with all materials correctly set up.

Currently the plugin is written for [Redloader](https://github.com/ToniMacaroni/RedLoader) and IL2CPP, but can be easily adapted to work with Mono and other mod loaders.

## Credits
- https://github.com/KhronosGroup/UnityGLTF