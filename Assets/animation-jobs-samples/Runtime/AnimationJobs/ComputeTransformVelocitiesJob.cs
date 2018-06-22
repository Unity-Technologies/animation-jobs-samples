using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Animations;
using Unity.Collections;


public struct ComputeTransformVelocitiesJob : IAnimationJob
{
    public NativeArray<TransformStreamHandle> transformStreamHandles;

    public NativeArray<Vector3> velocities;
    public NativeArray<Vector3> angularVelocities;

    public Vector3 rootVelocity;
    public Vector3 rootAngularVelocity;

    public Pose previousPose;
    public Pose localPose;
    public Pose globalPose;

    public void Setup(Animator animator, Transform[] transforms)
    {
        transformStreamHandles = new NativeArray<TransformStreamHandle>(transforms.Length, Allocator.Persistent);
        previousPose = new Pose(transforms.Length, Allocator.Persistent);
        localPose = new Pose(transforms.Length, Allocator.Persistent);
        globalPose = new Pose(transforms.Length, Allocator.Persistent);
        velocities = new NativeArray<Vector3>(transforms.Length, Allocator.Persistent);
        angularVelocities = new NativeArray<Vector3>(transforms.Length, Allocator.Persistent);
        for (int i = 0; i < transforms.Length; i++)
        {
            transformStreamHandles[i] = animator.BindStreamTransform(transforms[i]);
        }
    }
    public void WriteTransformVelocities(Rigidbody[] rigidbodies)
    {
        for(int i = 1; i < rigidbodies.Length; i++)
        {
            var rigidbody = rigidbodies[i];
            if( rigidbody != null )
            {
                Vector3 velocity = velocities[i];
                Vector3 angularVelocity = angularVelocities[i];

                Vector3 force =  velocity - rigidbody.velocity;
                Vector3 torque = angularVelocity - rigidbody.angularVelocity;

                rigidbody.AddForce(force, ForceMode.VelocityChange);
                rigidbody.AddForce(-Physics.gravity, ForceMode.Acceleration);
                rigidbody.AddTorque(torque,ForceMode.VelocityChange);
            }
        }
    }

    public void Dipose()
    {
        transformStreamHandles.Dispose();
        previousPose.Dispose();
        localPose.Dispose();
        globalPose.Dispose();

        velocities.Dispose();
        angularVelocities.Dispose();
    }

	public void ProcessRootMotion(AnimationStream stream)
    {
        Vector3 velocity = stream.velocity;
        globalPose.velocity = velocity;
        globalPose.angularVelocity = stream.angularVelocity;
        localPose.velocity = stream.velocity;
        localPose.angularVelocity = stream.angularVelocity;
    }

    public void ProcessAnimation(AnimationStream stream)
    {
        previousPose.Copy(globalPose);

        globalPose.ReadGlobalPose(ref stream, transformStreamHandles);
        localPose.ReadLocalPose(ref stream, transformStreamHandles);

        Pose.Differentiate(globalPose, previousPose, stream.deltaTime, velocities, angularVelocities);
    }
}
