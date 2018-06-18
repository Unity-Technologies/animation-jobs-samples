using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;

public class LookAt : MonoBehaviour
{
    public enum Axis
    {
        Forward,
        Back,
        Up,
        Down,
        Left,
        Right
    }

    public Transform joint;
    public Axis axis = Axis.Forward;
    public float minAngle = -60.0f;
    public float maxAngle = 60.0f;

    GameObject m_Target;

    PlayableGraph m_Graph;
    AnimationScriptPlayable m_LookAtPlayable;

    Vector3 GetAxisVector(Axis axis)
    {
        switch (axis)
        {
            case Axis.Forward:
                return Vector3.forward;
            case Axis.Back:
                return Vector3.back;
            case Axis.Up:
                return Vector3.up;
            case Axis.Down:
                return Vector3.down;
            case Axis.Left:
                return Vector3.left;
            case Axis.Right:
                return Vector3.right;
        }

        return Vector3.forward;
    }

    void OnEnable()
    {
        var idleClip = SampleUtility.LoadAnimationClipFromFbx("Chomper/Animations/@ChomperIdle", "Cooldown");
        if (idleClip == null)
            return;

        if (joint == null)
            return;

        var targetPosition = joint.position + gameObject.transform.rotation * Vector3.forward;

        m_Target = SampleUtility.CreateEffector("Effector_" + joint.name, targetPosition, Quaternion.identity);

        m_Graph = PlayableGraph.Create("TwoBoneIK");
        var output = AnimationPlayableOutput.Create(m_Graph, "ouput", GetComponent<Animator>());

        var animator = GetComponent<Animator>();
        animator.fireEvents = false;
        var lookAtJob = new LookAtJob()
        {
            joint = animator.BindStreamTransform(joint),
            target = animator.BindSceneTransform(m_Target.transform),
            axis = GetAxisVector(axis),
            minAngle = Mathf.Min(minAngle, maxAngle),
            maxAngle = Mathf.Max(minAngle, maxAngle)
        };

        m_LookAtPlayable = AnimationScriptPlayable.Create(m_Graph, lookAtJob);
        m_LookAtPlayable.AddInput(AnimationClipPlayable.Create(m_Graph, idleClip), 0, 1.0f);

        output.SetSourcePlayable(m_LookAtPlayable);
        m_Graph.Play();
    }

    void OnDisable()
    {
        m_Graph.Destroy();
        Object.Destroy(m_Target);
    }
}
