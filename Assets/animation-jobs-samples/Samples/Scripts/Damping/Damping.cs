using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Experimental.Animations;

/// <summary>
/// Damping sample.
///
/// Remarks:
///     1. The root must be the parent of the tail;
///     2. The joints must have a simple hierarchy (i.e. joint N is parent of joint N+1).
/// </summary>
public class Damping : MonoBehaviour
{
    public Transform[] joints;

    PlayableGraph m_Graph;
    AnimationScriptPlayable m_DampingPlayable;

    NativeArray<TransformStreamHandle> m_Handles;
    NativeArray<Vector3> m_LocalPositions;
    NativeArray<Quaternion> m_LocalRotations;
    NativeArray<Vector3> m_Positions;
    NativeArray<Vector3> m_Velocities;

    List<GameObject> m_JointEffectors;

    void Initialize(Animator animator)
    {
        // Create arrays (without the root which has its own handle).
        var numJoints = joints.Length;
        m_Handles = new NativeArray<TransformStreamHandle>(numJoints, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        m_LocalPositions = new NativeArray<Vector3>(numJoints, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        m_LocalRotations = new NativeArray<Quaternion>(numJoints, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        m_Positions = new NativeArray<Vector3>(numJoints, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        for (var i = 0; i < numJoints; ++i)
        {
            m_Handles[i] = animator.BindStreamTransform(joints[i]);
            m_LocalPositions[i] = joints[i].localPosition;
            m_LocalRotations[i] = joints[i].localRotation;
            m_Positions[i] = joints[i].position;
        }

        m_Velocities = new NativeArray<Vector3>(numJoints, Allocator.Persistent);
    }

    void OnEnable()
    {
        if (joints.Length == 0)
            return;

        var animator = GetComponent<Animator>();

        // Create job.
        Initialize(animator);
        var dampingJob = new DampingJob()
        {
            rootHandle = animator.BindStreamTransform(transform),
            jointHandles = m_Handles,
            localPositions = m_LocalPositions,
            localRotations = m_LocalRotations,
            positions = m_Positions,
            velocities = m_Velocities
        };

        // Create graph.
        m_Graph = PlayableGraph.Create("Damping");
        m_Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        m_DampingPlayable = AnimationScriptPlayable.Create(m_Graph, dampingJob);

        var output = AnimationPlayableOutput.Create(m_Graph, "output", animator);
        output.SetSourcePlayable(m_DampingPlayable);

        // Start the graph.
        m_Graph.Play();

        // Create effectors for each joints.
        m_JointEffectors = new List<GameObject>(joints.Length);
        foreach (var joint in joints)
        {
            var effector = SampleUtility.CreateEffector(joint.name, joint.position, joint.rotation);
            effector.hideFlags |= HideFlags.HideInHierarchy;
            m_JointEffectors.Add(effector);
        }
    }

    void LateUpdate()
    {
        if (!m_Graph.IsValid())
            return;

        var dampingJob = m_DampingPlayable.GetJobData<DampingJob>();
        for (var i = 0; i < dampingJob.positions.Length; ++i)
            m_JointEffectors[i].transform.position = dampingJob.positions[i];
    }

    void OnDisable()
    {
        if (!m_Graph.IsValid())
            return;

        m_Handles.Dispose();
        m_LocalPositions.Dispose();
        m_LocalRotations.Dispose();
        m_Positions.Dispose();
        m_Velocities.Dispose();

        m_Graph.Destroy();
    }
}
