using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class Navigation_Script : MonoBehaviour
{
    public Transform target;
    public GameObject agentevac;
    NavMeshAgent agent;
    Animator animator;

 

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>(); 
        animator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(target.position);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
            animator.runtimeAnimatorController = Resources.Load("Kevin Iglesias/Basic Motions Pack/AnimationControllers/BasicMotions@Idle.controller") as RuntimeAnimatorController;
            agentevac.SetActive(false);
            //count = count + 1;
            //EvaccountText.text = "Evacuation: " + count.ToString();
        }
    } 
    

}
