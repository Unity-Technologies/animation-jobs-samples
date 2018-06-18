using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

using UnityEngine.Experimental.Animations;

public class WeightedMaskMixer : MonoBehaviour
{
    [Serializable]
    public struct BoneTransformWeight
    {
        public Transform transform;

        [Range(0.0f, 1.0f)]
        public float weight;
    }

    [Range(0.0f, 1.0f)]
    public float weight = 1.0f;

    public BoneTransformWeight[] boneTransformWeights;

    List<List<int>> m_BoneChildrenIndices;

    NativeArray<TransformStreamHandle> m_Handles;
    NativeArray<float> m_BoneWeights;

    PlayableGraph m_Graph;
    AnimationScriptPlayable m_CustomMixerPlayable;

    void UpdateWeights()
    {
        for (var i = 0; i < boneTransformWeights.Length; ++i)
        {
            var boneWeight = boneTransformWeights[i].weight;
            var childrenIndices = m_BoneChildrenIndices[i];
            foreach (var index in childrenIndices)
                m_BoneWeights[index] = boneWeight;
        }
    }

    void OnEnable()
    {
        // Load animation clips.
        var idleClip = SampleUtility.LoadAnimationClipFromFbx("DefaultMale/Models/DefaultMale_Generic", "Idle");
        var romClip = SampleUtility.LoadAnimationClipFromFbx("DefaultMale/Models/DefaultMale_Generic", "ROM");
        if (idleClip == null || romClip == null)
            return;

        var animator = GetComponent<Animator>();

        // Get all the transforms in the hierarchy.
        var allTransforms = animator.transform.GetComponentsInChildren<Transform>();
        var numTransforms = allTransforms.Length - 1;

        // Fill native arrays (all the bones have a weight of 0.0).
        m_Handles = new NativeArray<TransformStreamHandle>(numTransforms, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        m_BoneWeights = new NativeArray<float>(numTransforms, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        for (var i = 0; i < numTransforms; ++i)
            m_Handles[i] = animator.BindStreamTransform(allTransforms[i + 1]);

        // Set bone weights for selected transforms and their hierarchy.
        m_BoneChildrenIndices = new List<List<int>>(boneTransformWeights.Length);
        foreach (var boneTransform in boneTransformWeights)
        {
            var childrenTransforms = boneTransform.transform.GetComponentsInChildren<Transform>();
            var childrenIndices = new List<int>(childrenTransforms.Length);
            foreach (var childTransform in childrenTransforms)
            {
                var boneIndex = Array.IndexOf(allTransforms, childTransform);
                Debug.Assert(boneIndex > 0, "Index can't be less or equal to 0");
                childrenIndices.Add(boneIndex - 1);
            }

            m_BoneChildrenIndices.Add(childrenIndices);
        }

        // Create job.
        var job = new MixerJob()
        {
            handles = m_Handles,
            boneWeights = m_BoneWeights,
            weight = 1.0f
        };

        // Create graph with custom mixer.
        m_Graph = PlayableGraph.Create("CustomMixer");

        m_CustomMixerPlayable = AnimationScriptPlayable.Create(m_Graph, job);
        m_CustomMixerPlayable.SetProcessInputs(false);
        m_CustomMixerPlayable.AddInput(AnimationClipPlayable.Create(m_Graph, idleClip), 0, 1.0f);
        m_CustomMixerPlayable.AddInput(AnimationClipPlayable.Create(m_Graph, romClip), 0, 1.0f);

        var output = AnimationPlayableOutput.Create(m_Graph, "output", animator);
        output.SetSourcePlayable(m_CustomMixerPlayable);

        m_Graph.Play();
    }

    void Update()
    {
        var job = m_CustomMixerPlayable.GetJobData<MixerJob>();

        UpdateWeights();
        job.weight = weight;
        job.boneWeights = m_BoneWeights;

        m_CustomMixerPlayable.SetJobData(job);
    }

    void OnDisable()
    {
        m_Graph.Destroy();
        m_Handles.Dispose();
        m_BoneWeights.Dispose();
    }
}
