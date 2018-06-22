using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulateRagdoll : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter(Collision collision)
    {
        var physicsMixer = collision.transform.root.GetComponent<PhysicsMixer>();
        if (physicsMixer != null)
            physicsMixer.simulate = true;

    }
}
