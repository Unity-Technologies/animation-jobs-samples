using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effector : MonoBehaviour
{
    [Range(0.0f,1.0f)]
    public float positionWeight;
    [Range(0.0f,1.0f)]
    public float rotationWeight;
}

