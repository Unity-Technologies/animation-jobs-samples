using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effector : MonoBehaviour
{
    [Range(0.0f,1.0f)]
    public float positionWeight;
    [Range(0.0f,1.0f)]
    public float rotationWeight;
    [Range(0.0f,1.0f)]
    public float pullWeight;

    private void Update()
    {
        float averageWeight = (positionWeight + rotationWeight + pullWeight) / 3.0f;
        var material = GetComponent<Renderer>().material;
        Color color = Color.magenta;
        material.color = SampleUtility.FadeEffectorColorByWeight(color, averageWeight);
    }
}

