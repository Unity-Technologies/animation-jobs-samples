About Animation C# Jobs Samples
===============================

The Animation C# Jobs Samples package includes examples of Playable graphs
using Animation C# Jobs. For more information, see
[AnimationScriptPlayable](https://docs.unity3d.com/ScriptReference/Experimental.Animations.AnimationScriptPlayable.html).

Installing Animation C# Jobs Samples
====================================

To install this package, follow the instructions in the
[Package Manager documentation](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@latest/index.html). 

Using Animation C# Jobs Samples
===============================

Because package content is read-only, it is not possible to open scenes
directly from a package directory, but it is possible to copy the scene files
locally, in the `Assets` folder.

To do so, go to `Packages/Animation C# Jobs Samples/Samples/Scenes`, then
select the sample scenes you want to try, and copy them into your `Assets`
folder.

Once copied, go to your `Assets` folder and open the Unity scene that you would
like to test.

Samples
-------

### SimpleMixer

This sample shows how to mix two clips together on a generic character.

### WeightedMaskMixer

This sample is based on the SimpleMixer, but you can change the weights of some
joints (left arm, right arm, and head), so that the second clip only applies
based on the joint weights.

### LookAt

This sample adds a look-at behaviour on a Chomper, a quadruped creature from the
[3D Game Kit](https://assetstore.unity.com/packages/essentials/tutorial-projects/3d-game-kit-115747).

### TwoBoneIK

This sample adds a two-bone IK on a generic human character.

### FullBodyIK

This sample create a fullbody IK on a humanoid character.

Technical details
=================

Requirements
------------

This version of Animation C# Jobs Samples is compatible with Unity 2018.2 and
later.

The [GraphVisualizer](https://github.com/Unity-Technologies/graph-visualizer)
can be used to have a visual representation of the graphs in the samples.

Package contents
----------------

The following table indicates the structure of the package:

| Location                  | Description                                                                                   |
|---------------------------|-----------------------------------------------------------------------------------------------|
| `Resources`               | Contains all the resources that the samples need (e.g. meshes, textures, animations, ...).    |
| `Runtime/AnimationJobs`   | Contains all the animation C# jobs.                                                           |
| `Samples/Scenes`          | Contains all the sample scenes.                                                               |
| `Samples/Scripts`         | Contains all the sample scripts.                                                              |

Document revision history
-------------------------
 
| Date          | Reason                                |
|---------------|---------------------------------------|
| June 01, 2018 | New way of testing the samples.       |
| May 30, 2018  | First public release: 0.5.0-preview.  |
| May 07, 2018  | Creation of the package.              |
