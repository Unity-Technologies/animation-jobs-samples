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
}
