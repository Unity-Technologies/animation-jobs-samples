using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System.Runtime.InteropServices;
using UnityEngine.Experimental.Animations;

[StructLayout (LayoutKind.Sequential)]
public struct TRX
{
    public Vector3	  position;
    public Quaternion rotation;

    public TRX (Vector3 p, Quaternion r)
    {
        this.position = p;
        this.rotation = r;
    }

    public TRX (TRX rhs)
    {
        this.position = rhs.position;
        this.rotation = rhs.rotation;
    }
}

public struct Pose
{
    public Vector3 velocity;
    public Vector3 angularVelocity;

    NativeArray<TRX> m_Pose;

    public Pose(int size, Allocator allocator)
    {
        m_Pose = new NativeArray<TRX>(size, allocator);
        velocity = Vector3.zero;
        angularVelocity = Vector3.zero;
    }

    public void Dispose()
    {
        m_Pose.Dispose();
    }

    public TRX this[int i]
    { 
        get { return m_Pose[i]; } 
        set { m_Pose[i] = value; }
    }

    public int Length { get { return m_Pose.Length; } }

    public void Copy(Pose source)
    {
        for (int i = 0; i < Length; i++)
        {
            m_Pose[i] = source[i];
        }
    }

    public void ReadGlobalPose(ref AnimationStream stream, NativeArray<TransformStreamHandle> handle)
    {
        for (int i = 0; i < Length; i++)
        {
            Vector3 position = handle[i].GetPosition(stream);
            Quaternion rotation = handle[i].GetRotation(stream);

            m_Pose[i] = new TRX(position, rotation);
        }
    }

    public void ReadLocalPose(ref AnimationStream stream, NativeArray<TransformStreamHandle> handle)
    {
        for (int i = 0; i < Length; i++)
        {
            Vector3 position = handle[i].GetLocalPosition(stream);
            Quaternion rotation = handle[i].GetLocalRotation(stream);

            m_Pose[i] = new TRX(position, rotation);
        }
    }

    public static void Differentiate(Pose p1, Pose p2, float deltaTime, NativeArray<Vector3> velocities, NativeArray<Vector3> angularVelocities)
    {
        float inverseDeltaTime = 1.0f / deltaTime;

        velocities[0] = p1.velocity;
        angularVelocities[0] = p1.angularVelocity;

        for(int i = 1; i < p1.Length; i++)
        {
            TRX p1trX = p1.m_Pose[i];
            TRX p2trX = p2.m_Pose[i];

            Vector3 dt = p1trX.position - p2trX.position;
            Quaternion dq = p1trX.rotation * Quaternion.Inverse(p2trX.rotation);

            velocities[i] = dt * inverseDeltaTime;
            angularVelocities[i] = quatToAngularDisplacement(dq) * inverseDeltaTime;
        }
    }

    private static Vector3 quatToAngularDisplacement(Quaternion q)
    {
        Quaternion qn = q.normalized;
        Vector3 xyz = new Vector3(qn.x, qn.y, qn.z);
        float len = xyz.magnitude;
        if (len == 0.0f)
            return Vector3.zero;

        float angle = 2.0f * Mathf.Asin(len);
        return xyz * angle / len;
    }
}
