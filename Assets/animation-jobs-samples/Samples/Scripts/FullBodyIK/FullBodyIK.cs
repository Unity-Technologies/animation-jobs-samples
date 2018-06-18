using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Experimental.Animations;
using UnityEditor;

public class FullBodyIK : MonoBehaviour
{
    private GameObject leftFootEffector;
    private GameObject rightFootEffector;
    private GameObject leftHandEffector;
    private GameObject rightHandEffector;

    private GameObject leftKneeHintEffector;
    private GameObject rightKneeHintEffector;
    private GameObject leftElbowHintEffector;
    private GameObject rightElbowHintEffector;

    private GameObject lookAtEffector;

    private GameObject bodyRotationEffector;

    private Animator animator;
    private PlayableGraph graph;
    private AnimationScriptPlayable ikPlayable;

    private static GameObject CreateEffector(string name)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        if (go != null)
        {
            go.name = name;
            go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }

        return go;
    }

    private static GameObject CreateBodyEffector(string name)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        if (go != null)
        {
            go.name = name;
            go.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        }

        return go;
    }

    private GameObject SetupEffector(ref FullBodyIKJob.EffectorHandle handle, string name)
    {
        var go = CreateEffector(name);
        if (go != null)
        {
            go.AddComponent<Effector>();
            handle.effector = animator.BindSceneTransform(go.transform);
            handle.positionWeight = animator.BindSceneProperty(go.transform, typeof(Effector), "positionWeight");
            handle.rotationWeight = animator.BindSceneProperty(go.transform, typeof(Effector), "rotationWeight");
        }
        return go;
    }

    private GameObject SetupHintEffector(ref FullBodyIKJob.HintEffectorHandle handle, string name)
    {
        var go = CreateEffector(name);
        if (go != null)
        {
            go.AddComponent<HintEffector>();
            handle.hint = animator.BindSceneTransform(go.transform);
            handle.weight = animator.BindSceneProperty(go.transform, typeof(HintEffector), "weight");
        }
        return go;
    }

    private GameObject SetupLookAtEffector(ref FullBodyIKJob.LookEffectorHandle handle, string name)
    {
        var go = CreateEffector(name);
        if (go != null)
        {
            go.AddComponent<LookAtEffector>();
            handle.lookAt =  animator.BindSceneTransform(go.transform);
            handle.eyesWeight = animator.BindSceneProperty(go.transform, typeof(LookAtEffector), "eyesWeight");
            handle.headWeight = animator.BindSceneProperty(go.transform, typeof(LookAtEffector), "headWeight");
            handle.bodyWeight = animator.BindSceneProperty(go.transform, typeof(LookAtEffector), "bodyWeight");
            handle.clampWeight = animator.BindSceneProperty(go.transform, typeof(LookAtEffector), "clampWeight");
        }
        return go;
    }

    private GameObject SetupBodyEffector(ref FullBodyIKJob.BodyEffectorHandle handle, string name)
    {
        var go = CreateBodyEffector(name);
        if (go != null)
        {
            handle.body =  animator.BindSceneTransform(go.transform);
        }
        return go;
    }

    private void ResetIKWeight()
    {
        leftFootEffector.GetComponent<Effector>().positionWeight = 1.0f;
        leftFootEffector.GetComponent<Effector>().rotationWeight = 1.0f;
        rightFootEffector.GetComponent<Effector>().positionWeight = 1.0f;
        rightFootEffector.GetComponent<Effector>().rotationWeight = 1.0f;
        leftHandEffector.GetComponent<Effector>().positionWeight = 1.0f;
        leftHandEffector.GetComponent<Effector>().rotationWeight = 1.0f;
        rightHandEffector.GetComponent<Effector>().positionWeight = 1.0f;
        rightHandEffector.GetComponent<Effector>().rotationWeight = 1.0f;

        leftKneeHintEffector.GetComponent<HintEffector>().weight = 1.0f;
        rightKneeHintEffector.GetComponent<HintEffector>().weight = 1.0f;
        leftElbowHintEffector.GetComponent<HintEffector>().weight = 1.0f;
        rightElbowHintEffector.GetComponent<HintEffector>().weight = 1.0f;
        bodyRotationEffector.GetComponent<HintEffector>().weight = 1.0f;
    }

    private void SyncIKFromPose()
    {
        var selectedTransform = Selection.transforms;

        var stream = new AnimationStream();
        if(animator.OpenAnimationStream(ref stream))
        {
            AnimationHumanStream humanStream = stream.AsHuman();

            // don't sync if transform is currently selected
            if( !Array.Exists(selectedTransform, transform => transform == leftFootEffector.transform) )
            {
                leftFootEffector.transform.position = humanStream.GetGoalPositionFromPose(AvatarIKGoal.LeftFoot);
                leftFootEffector.transform.rotation = humanStream.GetGoalRotationFromPose(AvatarIKGoal.LeftFoot);
            }

            if( !Array.Exists(selectedTransform, transform => transform == rightFootEffector.transform) )
            {
                rightFootEffector.transform.position = humanStream.GetGoalPositionFromPose(AvatarIKGoal.RightFoot);
                rightFootEffector.transform.rotation = humanStream.GetGoalRotationFromPose(AvatarIKGoal.RightFoot);
            }

            if( !Array.Exists(selectedTransform, transform => transform == leftHandEffector.transform) )
            {
                leftHandEffector.transform.position = humanStream.GetGoalPositionFromPose(AvatarIKGoal.LeftHand);
                leftHandEffector.transform.rotation = humanStream.GetGoalRotationFromPose(AvatarIKGoal.LeftHand);
            }

            if( !Array.Exists(selectedTransform, transform => transform == rightHandEffector.transform) )
            {
                rightHandEffector.transform.position = humanStream.GetGoalPositionFromPose(AvatarIKGoal.RightHand);
                rightHandEffector.transform.rotation = humanStream.GetGoalRotationFromPose(AvatarIKGoal.RightHand);
            }

            if( !Array.Exists(selectedTransform, transform => transform == leftKneeHintEffector.transform) )
            {
                leftKneeHintEffector.transform.position = humanStream.GetHintPosition(AvatarIKHint.LeftKnee);
            }

            if( !Array.Exists(selectedTransform, transform => transform == rightKneeHintEffector.transform) )
            {
                rightKneeHintEffector.transform.position = humanStream.GetHintPosition(AvatarIKHint.RightKnee);
            }

            if( !Array.Exists(selectedTransform, transform => transform == leftElbowHintEffector.transform) )
            {
                leftElbowHintEffector.transform.position = humanStream.GetHintPosition(AvatarIKHint.LeftElbow);
            }

            if( !Array.Exists(selectedTransform, transform => transform == rightElbowHintEffector.transform) )
            {
                rightElbowHintEffector.transform.position = humanStream.GetHintPosition(AvatarIKHint.RightElbow);
            }

            if( !Array.Exists(selectedTransform, transform => transform == bodyRotationEffector.transform) )
            {
                bodyRotationEffector.transform.position = humanStream.bodyPosition;
                bodyRotationEffector.transform.rotation = humanStream.bodyRotation;
            }

            animator.CloseAnimationStream(ref stream);
        }
    }

    void OnEnable()
    {
        animator = GetComponent<Animator>();

        if (!animator.avatar.isHuman)
            throw new InvalidOperationException("Avatar must be a humanoid.");

        graph = PlayableGraph.Create();
        var output = AnimationPlayableOutput.Create(graph, "output", animator);

        var job = new FullBodyIKJob();

        leftFootEffector = SetupEffector(ref job.leftFootEffector, "leftFootEffector");
        rightFootEffector = SetupEffector(ref job.rightFootEffector, "rightFootEffector");
        leftHandEffector = SetupEffector(ref job.leftHandEffector, "leftHandEffector");
        rightHandEffector = SetupEffector(ref job.rightHandEffector, "rightHandEffector");

        leftKneeHintEffector = SetupHintEffector(ref job.leftKneeHintEffector, "leftKneeHintEffector");
        rightKneeHintEffector = SetupHintEffector(ref job.rightKneeHintEffector, "rightKneeHintEffector");
        leftElbowHintEffector = SetupHintEffector(ref job.leftElbowHintEffector, "leftElbowHintEffector");
        rightElbowHintEffector = SetupHintEffector(ref job.rightElbowHintEffector, "rightElbowHintEffector");

        lookAtEffector = SetupLookAtEffector(ref job.lookAtEffector, "lookAtEffector");

        bodyRotationEffector = SetupBodyEffector(ref job.bodyEffector, "bodyEffector");

        ikPlayable = AnimationScriptPlayable.Create<FullBodyIKJob>(graph, job);

        output.SetSourcePlayable(ikPlayable);

        // Sync IK goal on original pose and activate ik weight
        SyncIKFromPose();
        ResetIKWeight();

        graph.Play();
    }

    void OnDisable()
    {
        GameObject.DestroyImmediate(leftFootEffector);
        GameObject.DestroyImmediate(rightFootEffector);
        GameObject.DestroyImmediate(leftHandEffector);
        GameObject.DestroyImmediate(rightHandEffector);
        GameObject.DestroyImmediate(leftKneeHintEffector);
        GameObject.DestroyImmediate(rightKneeHintEffector);
        GameObject.DestroyImmediate(leftElbowHintEffector);
        GameObject.DestroyImmediate(rightElbowHintEffector);
        GameObject.DestroyImmediate(lookAtEffector);
        GameObject.DestroyImmediate(bodyRotationEffector);

        if(graph.IsValid())
            graph.Destroy();
    }
}
