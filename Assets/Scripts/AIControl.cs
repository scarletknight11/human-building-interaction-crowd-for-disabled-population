using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AIControl : MonoBehaviour {

	public Transform target;
	GameObject[] goalLocations;
	public GameObject agentevac;
	UnityEngine.AI.NavMeshAgent agent;
	Animator anim;

	// Use this for initialization
	void Start () {
		goalLocations = GameObject.FindGameObjectsWithTag("Finish");
		agent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
		agent.SetDestination(goalLocations[Random.Range(0,goalLocations.Length)].transform.position);
		anim = this.GetComponent<Animator>();
		anim.SetFloat("wOffset", Random.Range(0, 1));
		anim.SetTrigger("isWalking");
		float sm = Random.Range(0.1f, 1.5f);
		anim.SetFloat("speedMult", sm);
		agent.speed *= sm;
	}
	
	// Update is called once per frame
	void Update () {
        //keep agents distance apart
        if (agent.remainingDistance < 1)
        {
            //gets random goal location of agents on set points of tagged goal
            agent.SetDestination(goalLocations[Random.Range(0, goalLocations.Length)].transform.position);
        }

    }

	void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
            //anim.runtimeAnimatorController = Resources.Load("Kevin Iglesias/Basic Motions Pack/AnimationControllers/BasicMotions@Idle.controller") as RuntimeAnimatorController;
            agentevac.SetActive(false);
            //count = count + 1;
            //EvaccountText.text = "Evacuation: " + count.ToString();
        }
    } 
}
