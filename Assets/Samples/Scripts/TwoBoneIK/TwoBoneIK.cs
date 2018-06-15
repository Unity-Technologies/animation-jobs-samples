using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;

public class TwoBoneIK : MonoBehaviour
{
    public Transform endJoint;

    Transform m_TopJoint;
    Transform m_MidJoint;
    GameObject m_Effector;

    PlayableGraph m_Graph;
    AnimationScriptPlayable m_IKPlayable;

    void OnEnable()
    {
        var idleClip = SampleUtility.LoadAnimationClipFromFbx("DefaultMale/Models/DefaultMale_Generic", "Idle");
        if (idleClip == null)
            return;

        if (endJoint == null)
            return;

        m_MidJoint = endJoint.parent;
        if (m_MidJoint == null)
            return;

        m_TopJoint = m_MidJoint.parent;
        if (m_TopJoint == null)
            return;

        m_Effector = SampleUtility.CreateEffector("Effector_" + endJoint.name, endJoint.position, endJoint.rotation);

        m_Graph = PlayableGraph.Create("TwoBoneIK");
        var output = AnimationPlayableOutput.Create(m_Graph, "ouput", GetComponent<Animator>());

        var twoBoneIKJob = new TwoBoneIKJob();
        twoBoneIKJob.Setup(GetComponent<Animator>(), m_TopJoint, m_MidJoint, endJoint, m_Effector.transform);

        m_IKPlayable = AnimationScriptPlayable.Create(m_Graph, twoBoneIKJob);
        m_IKPlayable.AddInput(AnimationClipPlayable.Create(m_Graph, idleClip), 0, 1.0f);

        output.SetSourcePlayable(m_IKPlayable);
        m_Graph.Play();
    }

    void OnDisable()
    {
        m_Graph.Destroy();
        Object.Destroy(m_Effector);
    }
}
