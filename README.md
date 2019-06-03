Animation C# Jobs Samples
=========================

The Animation C# Jobs Samples repository includes examples of Playable graphs
using Animation C# Jobs. For more information, see [AnimationScriptPlayable](https://docs.unity3d.com/ScriptReference/Experimental.Animations.AnimationScriptPlayable.html).

_Compatible with Unity 2019.3 or later._

The samples
-----------

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

Installation
------------

Simply `git clone` the repo somewhere and open the newly created directory with
Unity.

Optionally, the [GraphVisualizer](https://github.com/Unity-Technologies/graph-visualizer)
can be used to have a visual representation of the graphs in the samples.

Project contents
----------------

The following table indicates the structure of the `Assets/animation-jobs-samples` directory:

| Location                  | Description                                                                       |
|---------------------------|-----------------------------------------------------------------------------------|
| `Resources`               | Contains the resources for the samples (e.g. meshes, textures, animations, ...).  |
| `Runtime/AnimationJobs`   | Contains the animation C# jobs.                                                   |
| `Samples/Scripts`         | Contains the sample C# scripts.                                                   |
| `Scenes`                  | Contains the sample scenes.                                                       |
