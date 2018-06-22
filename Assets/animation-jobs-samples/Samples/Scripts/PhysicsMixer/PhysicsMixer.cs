using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;
using Unity.Collections;

public class PhysicsMixer : MonoBehaviour {

    public RuntimeAnimatorController controller;
    public bool             simulate;
    public bool             debugShowRigidBodyVelocity;
    public float            simulationTime = 1.0f;
    public float            physicsToAnimationBlendTime = 0.4f;

    bool                    m_PreviousSimulate;

    Transform[]             m_Transforms;
    Rigidbody[]             m_Rigidbodies;

    Animator                m_Animator;

    PlayableGraph                   m_Graph;
    AnimationMixerPlayable          m_Mixer;
    AnimationScriptPlayable         m_ReadRigScriptPlayable;
    AnimationScriptPlayable         m_ComputeVelocitiesScriptPlayable;
    
    // We need to keep these job data in a monobehaviour otherwise the system 
    // think that allocated memory from job is not reference and leak.
    ComputeTransformVelocitiesJob   m_ComputeVelocitiesJobData; 
    ReadRigJob                      m_ReadRigJobData;
    
    float                   m_BlendTime;
    float                   m_Time;

    const int               kAnimationSource = 0;
    const int               kReadRig = 1;

	public void SetRagdollActive(bool active)
    {
        for (int i = 1; i < m_Rigidbodies.Length; i++)
        {
            if (m_Rigidbodies[i] != null)
            {
                m_Rigidbodies[i].isKinematic = !active;
            }
        }
    }

    protected void SetupPlayableGraph()
    {
        m_Graph = PlayableGraph.Create();

        var output = AnimationPlayableOutput.Create(m_Graph, "PhysicsMixer", m_Animator);
        var controllerPlayable = AnimatorControllerPlayable.Create(m_Graph, controller);

        m_Mixer = AnimationMixerPlayable.Create(m_Graph, 2);

        m_ReadRigJobData = new ReadRigJob();
        m_ReadRigJobData.Setup(m_Animator, m_Transforms);
        m_ReadRigScriptPlayable = AnimationScriptPlayable.Create(m_Graph, m_ReadRigJobData);

        m_Mixer.ConnectInput(kAnimationSource, controllerPlayable, 0, 1.0f);
        m_Mixer.ConnectInput(kReadRig, m_ReadRigScriptPlayable, 0, 0.0f);

        m_ComputeVelocitiesJobData = new ComputeTransformVelocitiesJob();
        m_ComputeVelocitiesJobData.Setup(m_Animator, m_Transforms);
        m_ComputeVelocitiesScriptPlayable = AnimationScriptPlayable.Create(m_Graph, m_ComputeVelocitiesJobData, 1);
        m_ComputeVelocitiesScriptPlayable.ConnectInput(0, m_Mixer, 0, 1.0f);
        
        output.SetSourcePlayable(m_ComputeVelocitiesScriptPlayable);
    }

    protected void OnEnable()
    {
        if (controller == null)
            throw new System.ArgumentNullException("controller");

        debugShowRigidBodyVelocity = false;
        simulate = false;
        m_PreviousSimulate = false;
        m_BlendTime = 0.0f;
        m_Time = 0.0f;
        m_Animator = GetComponent<Animator>();
        m_Transforms = GetComponentsInChildren<Transform>();
        m_Rigidbodies = new Rigidbody[m_Transforms.Length];
        for (int i = 0; i < m_Transforms.Length; i++)
        {
            m_Rigidbodies[i] = m_Transforms[i].GetComponent<Rigidbody>();
        }

        SetRagdollActive(false);

        SetupPlayableGraph();

        m_Animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
        m_Animator.applyRootMotion = true;
        m_Animator.enabled = false;
        m_Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        m_Graph.Play();
    }

    protected void OnDisable()
    {
        if (m_Graph.IsValid())
        {
            var computeVelocitiesJobData = m_ComputeVelocitiesScriptPlayable.GetJobData<ComputeTransformVelocitiesJob>();
            computeVelocitiesJobData.Dipose();

            var readRigJobData = m_ReadRigScriptPlayable.GetJobData<ReadRigJob>();
            readRigJobData.Dipose();

            m_Graph.Destroy();
        }
    }

    IEnumerator WaitAndStopSimulation(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        simulate = false;
    }

    void FixedUpdate ()
    {
        // Here we start a blend from the physics pose to animation
        if(!simulate && m_PreviousSimulate)
        {
            m_PreviousSimulate = simulate;
            SetRagdollActive(false);

            var jobData = m_ReadRigScriptPlayable.GetJobData<ReadRigJob>();
            jobData.syncPose = true;
            m_ReadRigScriptPlayable.SetJobData(jobData);
            m_Mixer.SetInputWeight(kAnimationSource, 0.0f);
            m_Mixer.SetInputWeight(kReadRig, 1.0f);

            m_Animator.enabled = true;
            m_Graph.Evaluate(0.0f);
            m_Animator.enabled = false;

            jobData = m_ReadRigScriptPlayable.GetJobData<ReadRigJob>();
            jobData.syncPose = false;
            m_ReadRigScriptPlayable.SetJobData(jobData);

            m_PreviousSimulate = simulate;
            m_Time = 0.0f;
            m_BlendTime = physicsToAnimationBlendTime;
        }

        // graph is ticked manually to be sure that both ProcessRootMotion and ProcessAnimation is called before the physics fixed update
        if(!simulate || !m_PreviousSimulate)
        {
            m_Animator.enabled = true;
            m_Graph.Evaluate(Time.fixedDeltaTime);
            m_Animator.enabled = false;
        }

        // Here we start the physics simulation
        if(simulate && !m_PreviousSimulate)
        {
            m_PreviousSimulate = simulate;
            SetRagdollActive(true);

            var jobData = m_ComputeVelocitiesScriptPlayable.GetJobData<ComputeTransformVelocitiesJob>();
            jobData.WriteTransformVelocities(m_Rigidbodies);

            StartCoroutine(WaitAndStopSimulation(simulationTime));
        }

        // Blending is finish, reset time and weight accordingly
        if(m_Time > m_BlendTime)
        {
            m_Time = 0.0f;
            m_BlendTime = 0.0f;
            m_Mixer.SetInputWeight(kAnimationSource, 1.0f);
            m_Mixer.SetInputWeight(kReadRig, 0.0f);
        }
        // Updating the blend weight between physics pose and animation
        else if(m_Time < m_BlendTime)
        {
            float weight = Mathf.Clamp01(m_Time / m_BlendTime);
            m_Mixer.SetInputWeight(kAnimationSource, weight);
            m_Mixer.SetInputWeight(kReadRig, 1.0f - weight);

            m_Time += Time.fixedDeltaTime;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (m_Rigidbodies == null)
            return;

        if(debugShowRigidBodyVelocity)
        {
            Gizmos.color = Color.green;
            for(int i = 0; i < m_Rigidbodies.Length;i++)
            {
                if(m_Rigidbodies[i] != null)
                {
                    Gizmos.DrawLine(m_Rigidbodies[i].position, m_Rigidbodies[i].position +  m_Rigidbodies[i].velocity);
                }
            }
        }
    }
}
