using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;

public struct ReadRigJob : IAnimationJob
{
    public NativeArray<TransformSceneHandle> transformSceneHandles;
    public NativeArray<TransformStreamHandle> transformStreamHandles;
    public Pose localPose;
    public bool syncPose;

    public void Setup(Animator animator, Transform[] transforms)
    {
        transformSceneHandles = new NativeArray<TransformSceneHandle>(transforms.Length, Allocator.Persistent);
        transformStreamHandles = new NativeArray<TransformStreamHandle>(transforms.Length, Allocator.Persistent);
        localPose = new Pose(transforms.Length, Allocator.Persistent);
        syncPose = false;
        for (int i = 0; i < transforms.Length; i++)
        {
            transformSceneHandles[i] = animator.BindSceneTransform(transforms[i]);
            transformStreamHandles[i] = animator.BindStreamTransform(transforms[i]);
        }
    }

    public void Dipose()
    {
        transformSceneHandles.Dispose();
        transformStreamHandles.Dispose();
        localPose.Dispose();
    }

    public void ProcessRootMotion(AnimationStream stream)
    {
    }

    public void ProcessAnimation(AnimationStream stream)
    {
        for (int i = 0; i < transformStreamHandles.Length; i++)
        {
            if(syncPose)
            {
                Vector3 localPosition = transformSceneHandles[i].GetLocalPosition(stream);
                Quaternion localRotation = transformSceneHandles[i].GetLocalRotation(stream);

                localPose[i]  = new TRX(localPosition, localRotation);
            }

            transformStreamHandles[i].SetLocalPosition(stream, localPose[i].position);
            transformStreamHandles[i].SetLocalRotation(stream, localPose[i].rotation);
        }
    }
}