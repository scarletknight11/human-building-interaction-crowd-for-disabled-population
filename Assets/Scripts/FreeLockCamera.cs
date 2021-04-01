using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FreeLockCamera : MonoBehaviour {

    public GameObject agentmanagerscript;
    public float inputX;
    public float inputZ;
    private Vector3 Destination = Vector3.zero;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                var objectsposition = hit.transform.gameObject;
                Destination = hit.point;
                agentmanagerscript.GetComponent<AgentManager>().SetAgentDestinations(hit.point);
            }
        }
        if (Destination != Vector3.zero)
        {
            agentmanagerscript.GetComponent<AgentManager>().SetAgentDestinations(Destination);
        }

        inputX = Input.GetAxis("Horizontal");
        inputZ = Input.GetAxis("Vertical");
        if (inputX != 0)
            rotate();
        if (inputZ != 0)
            Move();
    }

    private void Move()
    {
        transform.position += transform.forward * inputZ * Time.deltaTime;
    }

    private void rotate()
    {
        transform.Rotate(new Vector3(0f, inputX * Time.deltaTime, 0f));
    }
}