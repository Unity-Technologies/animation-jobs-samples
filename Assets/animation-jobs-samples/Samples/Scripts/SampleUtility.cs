using System.Linq;
using UnityEngine;

public static class SampleUtility
{
    public static AnimationClip LoadAnimationClipFromFbx(string fbxName, string clipName)
    {
        var clips = Resources.LoadAll<AnimationClip>(fbxName);
        return clips.FirstOrDefault(clip => clip.name == clipName);
    }

    public static GameObject CreateEffector(string name, Vector3 position, Quaternion rotation)
    {
        var effector = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        effector.name = name;
        effector.transform.position = position;
        effector.transform.rotation = rotation;
        effector.transform.localScale = Vector3.one * 0.15f;
        var meshRenderer = effector.GetComponent<MeshRenderer>();
        meshRenderer.material.color = Color.magenta;
        return effector;
    }
}
