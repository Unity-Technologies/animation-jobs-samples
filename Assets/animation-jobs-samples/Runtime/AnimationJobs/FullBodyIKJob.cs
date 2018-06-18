using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;

public struct FullBodyIKJob : IAnimationJob
{
    public struct EffectorHandle
    {
        public TransformSceneHandle effector;
        public PropertySceneHandle positionWeight;
        public PropertySceneHandle rotationWeight;
    }

    public struct HintEffectorHandle
    {
        public TransformSceneHandle hint;
        public PropertySceneHandle weight;
    }

    public struct LookEffectorHandle
    {
        public TransformSceneHandle lookAt;
        public PropertySceneHandle eyesWeight;
        public PropertySceneHandle headWeight;
        public PropertySceneHandle bodyWeight;
        public PropertySceneHandle clampWeight;
    }

    public struct BodyEffectorHandle
    {
        public TransformSceneHandle body;
    }

    public EffectorHandle leftFootEffector;
    public EffectorHandle rightFootEffector;
    public EffectorHandle leftHandEffector;
    public EffectorHandle rightHandEffector;

    public HintEffectorHandle leftKneeHintEffector;
    public HintEffectorHandle rightKneeHintEffector;
    public HintEffectorHandle leftElbowHintEffector;
    public HintEffectorHandle rightElbowHintEffector;

    public LookEffectorHandle lookAtEffector;

    public BodyEffectorHandle bodyEffector;

    private void SetEffector(AnimationStream stream, AvatarIKGoal goal, ref EffectorHandle handle)
    {
        if (handle.effector.IsValid(stream) && handle.positionWeight.IsValid(stream) && handle.rotationWeight.IsValid(stream))
        {
            AnimationHumanStream humanStream = stream.AsHuman();
            humanStream.SetGoalPosition(goal, handle.effector.GetPosition(stream));
            humanStream.SetGoalRotation(goal, handle.effector.GetRotation(stream));
            humanStream.SetGoalWeightPosition(goal, handle.positionWeight.GetFloat(stream));
            humanStream.SetGoalWeightRotation(goal, handle.rotationWeight.GetFloat(stream));
        }
    }

    private void SetHintEffector(AnimationStream stream, AvatarIKHint goal, ref HintEffectorHandle handle)
    {
        if (handle.hint.IsValid(stream) && handle.weight.IsValid(stream))
        {
            AnimationHumanStream humanStream = stream.AsHuman();
            humanStream.SetHintPosition(goal, handle.hint.GetPosition(stream));
            humanStream.SetHintWeightPosition(goal, handle.weight.GetFloat(stream));
        }
    }

    private void SetLookAtEffector(AnimationStream stream, ref LookEffectorHandle handle)
    {
        if (handle.lookAt.IsValid(stream))
        {
            AnimationHumanStream humanStream = stream.AsHuman();
            humanStream.SetLookAtPosition(handle.lookAt.GetPosition(stream));
            humanStream.SetLookAtEyesWeight(handle.eyesWeight.GetFloat(stream));
            humanStream.SetLookAtHeadWeight(handle.headWeight.GetFloat(stream));
            humanStream.SetLookAtBodyWeight(handle.bodyWeight.GetFloat(stream));
            humanStream.SetLookAtClampWeight(handle.clampWeight.GetFloat(stream));
        }
    }

    private void SetBodyEffector(AnimationStream stream, ref BodyEffectorHandle handle)
    {
        if (handle.body.IsValid(stream))
        {
            AnimationHumanStream humanStream = stream.AsHuman();
            humanStream.bodyRotation = handle.body.GetRotation(stream);
        }
    }

    private void Solve(AnimationStream stream)
    {
        AnimationHumanStream humanStream = stream.AsHuman();

        Vector3 bodyPosition = humanStream.bodyPosition;
        Vector3 bodyPositionDelta = Vector3.zero;
        float sumWeight = 0;

        for (int goalIter = 0; goalIter < 4; goalIter++)
        {
            float weight = humanStream.GetGoalWeightPosition((AvatarIKGoal)goalIter);
            weight = Mathf.Clamp01(weight);
            bodyPositionDelta += (humanStream.GetGoalPosition((AvatarIKGoal)goalIter) - humanStream.GetGoalPositionFromPose((AvatarIKGoal)goalIter)) * weight;
            sumWeight += weight;
        }

        if (sumWeight > 1)
        {
            bodyPositionDelta /= sumWeight;
        }

        bodyPosition += bodyPositionDelta;
        humanStream.bodyPosition = bodyPosition;

        if (bodyEffector.body.IsValid(stream))
        {
            bodyEffector.body.SetPosition(stream, bodyPosition);
        }

        humanStream.SolveIK();
    }

    public void ProcessRootMotion(AnimationStream stream) { }

    public void ProcessAnimation(AnimationStream stream)
    {
        SetEffector(stream, AvatarIKGoal.LeftFoot, ref leftFootEffector);
        SetEffector(stream, AvatarIKGoal.RightFoot, ref rightFootEffector);
        SetEffector(stream, AvatarIKGoal.LeftHand, ref leftHandEffector);
        SetEffector(stream, AvatarIKGoal.RightHand, ref rightHandEffector);

        SetHintEffector(stream, AvatarIKHint.LeftKnee, ref leftKneeHintEffector);
        SetHintEffector(stream, AvatarIKHint.RightKnee, ref rightKneeHintEffector);
        SetHintEffector(stream, AvatarIKHint.LeftElbow, ref leftElbowHintEffector);
        SetHintEffector(stream, AvatarIKHint.RightElbow, ref rightElbowHintEffector);

        SetLookAtEffector(stream, ref lookAtEffector);

        SetBodyEffector(stream, ref bodyEffector);

        Solve(stream);
    }
}
