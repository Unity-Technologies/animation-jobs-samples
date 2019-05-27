using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEditor;

#if UNITY_2019_3_OR_NEWER
using UnityEngine.Animations;
#else
using UnityEngine.Experimental.Animations;
#endif

public class FullBodyIK : MonoBehaviour
{
    public bool syncGoal = true;

    [Range(0.0f, 1.5f)]
    public float stiffness = 1.0f;

    [Range(1, 50)]
    public int maxPullIteration = 5;

    [Range(0, 1)]
    public float defaultEffectorPositionWeight = 1.0f;
    [Range(0, 1)]
    public float defaultEffectorRotationWeight = 1.0f;
    [Range(0, 1)]
    public float defaultEffectorPullWeight = 1.0f;
    [Range(0, 1)]
    public float defaultHintWeight = 0.0f;

    private GameObject m_LeftFootEffector;
    private GameObject m_RightFootEffector;
    private GameObject m_LeftHandEffector;
    private GameObject m_RightHandEffector;

    private GameObject m_LeftKneeHintEffector;
    private GameObject m_RightKneeHintEffector;
    private GameObject m_LeftElbowHintEffector;
    private GameObject m_RightElbowHintEffector;

    private GameObject m_LookAtEffector;

    private GameObject m_BodyRotationEffector;

    private Animator m_Animator;
    private PlayableGraph m_Graph;
    private AnimationScriptPlayable m_IKPlayable;

    private static GameObject CreateEffector(string name)
    {
        var go = SampleUtility.CreateEffector(name, Vector3.zero, Quaternion.identity);
        return go;
    }

    private static GameObject CreateBodyEffector(string name)
    {
        var go = SampleUtility.CreateBodyEffector(name, Vector3.zero, Quaternion.identity);
        return go;
    }

    private GameObject SetupEffector(ref FullBodyIKJob.EffectorHandle handle, string name)
    {
        var go = CreateEffector(name);
        if (go != null)
        {
            go.AddComponent<Effector>();
            handle.effector = m_Animator.BindSceneTransform(go.transform);
            handle.positionWeight = m_Animator.BindSceneProperty(go.transform, typeof(Effector), "positionWeight");
            handle.rotationWeight = m_Animator.BindSceneProperty(go.transform, typeof(Effector), "rotationWeight");
            handle.pullWeight = m_Animator.BindSceneProperty(go.transform, typeof(Effector), "pullWeight");
        }
        return go;
    }

    private GameObject SetupHintEffector(ref FullBodyIKJob.HintEffectorHandle handle, string name)
    {
        var go = CreateEffector(name);
        if (go != null)
        {
            go.AddComponent<HintEffector>();
            handle.hint = m_Animator.BindSceneTransform(go.transform);
            handle.weight = m_Animator.BindSceneProperty(go.transform, typeof(HintEffector), "weight");
        }
        return go;
    }

    private GameObject SetupLookAtEffector(ref FullBodyIKJob.LookEffectorHandle handle, string name)
    {
        var go = CreateEffector(name);
        if (go != null)
        {
            go.AddComponent<LookAtEffector>();
            handle.lookAt = m_Animator.BindSceneTransform(go.transform);
            handle.eyesWeight = m_Animator.BindSceneProperty(go.transform, typeof(LookAtEffector), "eyesWeight");
            handle.headWeight = m_Animator.BindSceneProperty(go.transform, typeof(LookAtEffector), "headWeight");
            handle.bodyWeight = m_Animator.BindSceneProperty(go.transform, typeof(LookAtEffector), "bodyWeight");
            handle.clampWeight = m_Animator.BindSceneProperty(go.transform, typeof(LookAtEffector), "clampWeight");
        }
        return go;
    }

    private GameObject SetupBodyEffector(ref FullBodyIKJob.BodyEffectorHandle handle, string name)
    {
        var go = CreateBodyEffector(name);
        if (go != null)
        {
            handle.body = m_Animator.BindSceneTransform(go.transform);
        }
        return go;
    }

    private void SetupIKLimbHandle(ref FullBodyIKJob.IKLimbHandle handle, HumanBodyBones top, HumanBodyBones middle, HumanBodyBones end)
    {
        handle.top = m_Animator.BindStreamTransform(m_Animator.GetBoneTransform(top));
        handle.middle = m_Animator.BindStreamTransform(m_Animator.GetBoneTransform(middle));
        handle.end = m_Animator.BindStreamTransform(m_Animator.GetBoneTransform(end));
    }

    private void ResetIKWeight()
    {
        m_LeftFootEffector.GetComponent<Effector>().positionWeight = defaultEffectorPositionWeight;
        m_LeftFootEffector.GetComponent<Effector>().rotationWeight = defaultEffectorRotationWeight;
        m_LeftFootEffector.GetComponent<Effector>().pullWeight = defaultEffectorPullWeight;
        m_RightFootEffector.GetComponent<Effector>().positionWeight = defaultEffectorPositionWeight;
        m_RightFootEffector.GetComponent<Effector>().rotationWeight = defaultEffectorRotationWeight;
        m_RightFootEffector.GetComponent<Effector>().pullWeight = defaultEffectorPullWeight;
        m_LeftHandEffector.GetComponent<Effector>().positionWeight = defaultEffectorPositionWeight;
        m_LeftHandEffector.GetComponent<Effector>().rotationWeight = defaultEffectorRotationWeight;
        m_LeftHandEffector.GetComponent<Effector>().pullWeight = defaultEffectorPullWeight;
        m_RightHandEffector.GetComponent<Effector>().positionWeight = defaultEffectorPositionWeight;
        m_RightHandEffector.GetComponent<Effector>().rotationWeight = defaultEffectorRotationWeight;
        m_RightHandEffector.GetComponent<Effector>().pullWeight = defaultEffectorPullWeight;

        m_LeftKneeHintEffector.GetComponent<HintEffector>().weight = defaultHintWeight;
        m_RightKneeHintEffector.GetComponent<HintEffector>().weight = defaultHintWeight;
        m_LeftElbowHintEffector.GetComponent<HintEffector>().weight = defaultHintWeight;
        m_RightElbowHintEffector.GetComponent<HintEffector>().weight = defaultHintWeight;
    }

    private void SyncIKFromPose()
    {
        var selectedTransform = Selection.transforms;

        var stream = new AnimationStream();
        if (m_Animator.OpenAnimationStream(ref stream))
        {
            AnimationHumanStream humanStream = stream.AsHuman();

            // don't sync if transform is currently selected
            if (!Array.Exists(selectedTransform, tr => tr == m_LeftFootEffector.transform))
            {
                m_LeftFootEffector.transform.position = humanStream.GetGoalPositionFromPose(AvatarIKGoal.LeftFoot);
                m_LeftFootEffector.transform.rotation = humanStream.GetGoalRotationFromPose(AvatarIKGoal.LeftFoot);
            }

            if (!Array.Exists(selectedTransform, tr => tr == m_RightFootEffector.transform))
            {
                m_RightFootEffector.transform.position = humanStream.GetGoalPositionFromPose(AvatarIKGoal.RightFoot);
                m_RightFootEffector.transform.rotation = humanStream.GetGoalRotationFromPose(AvatarIKGoal.RightFoot);
            }

            if (!Array.Exists(selectedTransform, tr => tr == m_LeftHandEffector.transform))
            {
                m_LeftHandEffector.transform.position = humanStream.GetGoalPositionFromPose(AvatarIKGoal.LeftHand);
                m_LeftHandEffector.transform.rotation = humanStream.GetGoalRotationFromPose(AvatarIKGoal.LeftHand);
            }

            if (!Array.Exists(selectedTransform, tr => tr == m_RightHandEffector.transform))
            {
                m_RightHandEffector.transform.position = humanStream.GetGoalPositionFromPose(AvatarIKGoal.RightHand);
                m_RightHandEffector.transform.rotation = humanStream.GetGoalRotationFromPose(AvatarIKGoal.RightHand);
            }

            if (!Array.Exists(selectedTransform, tr => tr == m_LeftKneeHintEffector.transform))
            {
                m_LeftKneeHintEffector.transform.position = humanStream.GetHintPosition(AvatarIKHint.LeftKnee);
            }

            if (!Array.Exists(selectedTransform, tr => tr == m_RightKneeHintEffector.transform))
            {
                m_RightKneeHintEffector.transform.position = humanStream.GetHintPosition(AvatarIKHint.RightKnee);
            }

            if (!Array.Exists(selectedTransform, tr => tr == m_LeftElbowHintEffector.transform))
            {
                m_LeftElbowHintEffector.transform.position = humanStream.GetHintPosition(AvatarIKHint.LeftElbow);
            }

            if (!Array.Exists(selectedTransform, tr => tr == m_RightElbowHintEffector.transform))
            {
                m_RightElbowHintEffector.transform.position = humanStream.GetHintPosition(AvatarIKHint.RightElbow);
            }

            if (!Array.Exists(selectedTransform, tr => tr == m_BodyRotationEffector.transform))
            {
                m_BodyRotationEffector.transform.position = humanStream.bodyPosition;
                m_BodyRotationEffector.transform.rotation = humanStream.bodyRotation;
            }

            m_Animator.CloseAnimationStream(ref stream);
        }
    }

    void OnEnable()
    {
        m_Animator = GetComponent<Animator>();

        // Setting to Always animate because on the first frame the renderer can be not visible which break syncGoal on start up
        m_Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        if (!m_Animator.avatar.isHuman)
            throw new InvalidOperationException("Avatar must be a humanoid.");

        m_Graph = PlayableGraph.Create();
        m_Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        var output = AnimationPlayableOutput.Create(m_Graph, "output", m_Animator);

        var clip = SampleUtility.LoadAnimationClipFromFbx("DefaultMale/Models/DefaultMale_Humanoid", "Idle");
        var clipPlayable = AnimationClipPlayable.Create(m_Graph, clip);
        clipPlayable.SetApplyFootIK(false);
        clipPlayable.SetApplyPlayableIK(false);

        var job = new FullBodyIKJob();
        job.stiffness = stiffness;
        job.maxPullIteration = maxPullIteration;

        SetupIKLimbHandle(ref job.leftArm, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand);
        SetupIKLimbHandle(ref job.rightArm, HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand);
        SetupIKLimbHandle(ref job.leftLeg, HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot);
        SetupIKLimbHandle(ref job.rightLeg, HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot);

        m_LeftFootEffector = SetupEffector(ref job.leftFootEffector, "LeftFootEffector");
        m_RightFootEffector = SetupEffector(ref job.rightFootEffector, "RightFootEffector");
        m_LeftHandEffector = SetupEffector(ref job.leftHandEffector, "LeftHandEffector");
        m_RightHandEffector = SetupEffector(ref job.rightHandEffector, "RightHandEffector");

        m_LeftKneeHintEffector = SetupHintEffector(ref job.leftKneeHintEffector, "LeftKneeHintEffector");
        m_RightKneeHintEffector = SetupHintEffector(ref job.rightKneeHintEffector, "RightKneeHintEffector");
        m_LeftElbowHintEffector = SetupHintEffector(ref job.leftElbowHintEffector, "LeftElbowHintEffector");
        m_RightElbowHintEffector = SetupHintEffector(ref job.rightElbowHintEffector, "RightElbowHintEffector");

        m_LookAtEffector = SetupLookAtEffector(ref job.lookAtEffector, "LookAtEffector");

        m_BodyRotationEffector = SetupBodyEffector(ref job.bodyEffector, "BodyEffector");


        m_IKPlayable = AnimationScriptPlayable.Create(m_Graph, job, 1);
        m_IKPlayable.ConnectInput(0, clipPlayable, 0, 1.0f);

        output.SetSourcePlayable(m_IKPlayable);

        m_Graph.Play();
        m_Graph.Evaluate(0);
        SyncIKFromPose();

        ResetIKWeight();
    }

    void OnDisable()
    {
        GameObject.DestroyImmediate(m_LeftFootEffector);
        GameObject.DestroyImmediate(m_RightFootEffector);
        GameObject.DestroyImmediate(m_LeftHandEffector);
        GameObject.DestroyImmediate(m_RightHandEffector);
        GameObject.DestroyImmediate(m_LeftKneeHintEffector);
        GameObject.DestroyImmediate(m_RightKneeHintEffector);
        GameObject.DestroyImmediate(m_LeftElbowHintEffector);
        GameObject.DestroyImmediate(m_RightElbowHintEffector);
        GameObject.DestroyImmediate(m_LookAtEffector);
        GameObject.DestroyImmediate(m_BodyRotationEffector);

        if (m_Graph.IsValid())
            m_Graph.Destroy();
    }

    void Update()
    {
        var job = m_IKPlayable.GetJobData<FullBodyIKJob>();
        job.stiffness = stiffness;
        job.maxPullIteration = maxPullIteration;
        m_IKPlayable.SetJobData(job);
    }

    void LateUpdate()
    {
        // Synchronize on LateUpdate to sync goal on current frame
        if (syncGoal)
        {
            SyncIKFromPose();
            syncGoal = false;
        }

        // Synchronize the body position and the body effector position
        var job = m_IKPlayable.GetJobData<FullBodyIKJob>();
        m_BodyRotationEffector.transform.position = job.bodyPosition;
    }
}
