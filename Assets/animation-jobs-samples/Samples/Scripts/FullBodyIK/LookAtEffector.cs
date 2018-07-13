using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtEffector : MonoBehaviour
{
    [Range(0.0f,1.0f)]
    public float eyesWeight;
    [Range(0.0f,1.0f)]
    public float headWeight;
    [Range(0.0f,1.0f)]
    public float bodyWeight;
    [Range(0.0f,1.0f)]
    public float clampWeight;

    private void Update()
    {
        float weight = (eyesWeight + headWeight + bodyWeight + clampWeight)/4.0f;
        var material = GetComponent<Renderer>().material;
        Color color = Color.magenta;
        material.color = SampleUtility.FadeEffectorColorByWeight(color, weight);
    }
}
