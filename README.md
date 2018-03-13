# Overview

Unity has a fairly in depth system for writing post effects through the use of 'OnRenderImage', and allows those effects to access the camera depth buffer, so it is in theory possible to reconstruct the world space position of a pixel from within a post effect shader. This can be extremely useful for writing cool effects (such as volumetric fog), but is tricky to setup in unity due to some confusion over various matrices. This repo aims to be a definitive catch all approach to world space based post effects.

# Features

* Works in VR and none VR
* Supports both multi pass and single pass stereo rendering
* Reads/stores whether build uses single pass stereo (not used but useful in general!)
* Written / tested in unity 2017.3
* Updated techniques from below to utilize the new built in unity_StereoEyeIndex shader keyword
* Doesn't require reconstruction of a ray which can lose precision
* No dependencies on anything! :)

# Contents

* Full repo is functioning unity project with demo that will work with / without vr 
* worldspaceposteffect_withdemo.unitypackage contains code + demo scene 
* worldspaceposteffect_withoutdemo.unitypackage contains just the code (1 shader + 1 c# component)

# Credit

Huge credit goes to the internet, especially here https://gamedev.stackexchange.com/questions/131978/shader-reconstructing-position-from-depth-in-vr-through-projection-matrix and here https://forum.unity.com/threads/solved-reconstruct-world-position-from-depth-texture-in-single-pass-stereo-vr.480003/. This repo is largely combining and expanding upon the hard work done by others, especially https://forum.unity.com/members/equalsequals.25126/.

# Fixes / updates

Whilst I don't want to start trying to support old unity versions, I'd love for this to 'just work' for anyone in future, so please let me know if you find bugs / want to add support for something. 
