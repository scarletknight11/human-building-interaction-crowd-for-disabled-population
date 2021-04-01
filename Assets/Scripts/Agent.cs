using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour {

    public float radius;
    public float mass;
    public float perceptionRadius;
    public bool isLeader = false;

    public AgentManager agents;

    private List<Vector3> path;
    private NavMeshAgent nma;
    private Rigidbody rb;

    private HashSet<GameObject> perceivedNeighbors = new HashSet<GameObject>();
    private HashSet<GameObject> adjacentWalls = new HashSet<GameObject>();
    private float DesiredSpeed = 1;

    void Start()
    {
        path = new List<Vector3>();
        nma = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        gameObject.transform.localScale = new Vector3(2 * radius, 1, 2 * radius);
        nma.radius = radius;
        rb.mass = mass;
        GetComponent<SphereCollider>().radius = perceptionRadius / 2;
    }

    private void Update()
    {
        if (path.Count > 1 && Vector3.Distance(transform.position, path[0]) < 1.1f)
        {
            path.RemoveAt(0);
        } else if (path.Count == 1 && Vector3.Distance(transform.position, path[0]) < 2f)
        {
            path.RemoveAt(0);

            if (path.Count == 0)
            {
                gameObject.SetActive(false);
                AgentManager.RemoveAgent(gameObject);
		if (isLeader)
		{
		    agents.ElectLeader();
		}
            }
        }

        #region Visualization

        if (false)
        {
            if (path.Count > 0)
            {
                Debug.DrawLine(transform.position, path[0], Color.green);
            }
            for (int i = 0; i < path.Count - 1; i++)
            {
                Debug.DrawLine(path[i], path[i + 1], Color.yellow);
            }
        }

        if (false)
        {
            foreach (var neighbor in perceivedNeighbors)
            {
                Debug.DrawLine(transform.position, neighbor.transform.position, Color.yellow);
            }
        }

        #endregion
    }

    #region Public Functions

    public void ComputePath(Vector3 destination)
    {
        nma.enabled = true;
        var nmPath = new NavMeshPath();
        nma.CalculatePath(destination, nmPath);
        path = nmPath.corners.Skip(1).ToList();
        //path = new List<Vector3>() { destination };
        //nma.SetDestination(destination);
        nma.enabled = false;
    }

    public Vector3 GetVelocity()
    {
        return rb.velocity;
    }

    #endregion

    #region Incomplete Functions

    private Vector3 ComputeForce()
    {
        Vector3 force;
        if (agents.simulationType == SimulationType.GrowingSpiral)
        {
            force = CalculateSpiralForce() + CalculateWallForce() + CalculateGoalForce();
        }
	else if (agents.simulationType == SimulationType.LeaderFollowing)
	{
	    force = CalculateLeaderFollowingForce() + CalculateWallForce() + CalculateAgentForce();
	}
        else
        {
            force = CalculateGoalForce() + CalculateWallForce() + CalculateAgentForce();
        }


        if (force != Vector3.zero)
        {
            return force.normalized * Mathf.Min(force.magnitude, Parameters.maxSpeed);
        } else
        {
            return Vector3.zero;
        }
    }
    
    private Vector3 CalculateGoalForce()
    {
        if (path.Count == 0)
        {
            return Vector3.zero;
        }

        var temp = path[0] - transform.position;
        var desiredVel = temp.normalized * DesiredSpeed;
        var actVel = rb.velocity;
        return mass * (desiredVel - actVel) / Parameters.T;
    }

    private Vector3 CalculateAgentForce()
    {
        //position to 0
         var agentForce = Vector3.zero;
        
        //for number of agents that spawn
         foreach (var n in perceivedNeighbors)
        {
            //agentmanager generates agents
            if (!AgentManager.IsAgent(n))
            {
                continue;
            }
            //agentmanager get agentobjects and render them
            var neighbor = AgentManager.agentsObjs[n];
            //calculate direction of position objects are moving
            var dir = (transform.position - neighbor.transform.position).normalized;
            //calculate collision of radius between objects and distance of position and transformation
            var overlap = (radius + neighbor.radius) - Vector3.Distance(transform.position, n.transform.position);

            //updates position of agent force distances
            agentForce += Parameters.A * Mathf.Exp(overlap / Parameters.B) * dir;
            //agentForce += Parameters.k * (overlap > 0f ? 1 : 0) * dir;

            //var tangent = Vector3.Cross(Vector3.up, dir);
        }
        return agentForce;
    }

    private Vector3 CalculateWallForce()
    {
        var wallForce = Vector3.zero;
        foreach (var w in adjacentWalls)
        {
            if (!WallManager.IsWall(w))
            {
                continue;
            }
            var dir = (transform.position - w.transform.position).normalized;
            var overlap = (radius + 0.5f) - Vector3.Distance(transform.position, w.transform.position);

            wallForce += Parameters.A * Mathf.Exp(overlap / Parameters.B) * dir;
            wallForce += Parameters.k * Mathf.Max(overlap, 0) * dir;

            var tangent = Vector3.Cross(Vector3.up, dir);
            wallForce += Parameters.Kappa * (overlap > 0f ? overlap : 0) * Vector3.Dot(rb.velocity, tangent) * tangent;
            
        }
        return wallForce;
    }
    private Vector3 CalculateSpiralForce()
    {
        var radius = (path[0] - transform.position);
        Vector3 perpVect = Vector3.zero;
        perpVect.x = radius.z;
        perpVect.z= -radius.x;
        var centripForceMagnitude = (1/radius.sqrMagnitude) * mass;
        Vector3 centripForce = radius.normalized * centripForceMagnitude;
        return perpVect.normalized * 2f;
    }

    private Vector3 CalculateLeaderFollowingForce()
    {
	if (isLeader) return CalculateGoalForce();
	var leader = agents.leader;
	var leaderAgent = leader.GetComponent<Agent>();
	var leaderVel = leaderAgent.GetVelocity();
	var diff = leader.transform.position - transform.position;
	var angularDeviation = Vector3.SignedAngle(
	    diff,
	    leaderVel,
	    Vector3.up
	);
	
	// apply 90 degree rotation matrix to get a perp for the right
	// if it should be on the left Sign(angularDeviation) will be negative
	var perp = new Vector3(-leaderVel.z, 0, leaderVel.x).normalized *
	    Mathf.Sign(angularDeviation);
	// move towards behind the leader
	var fallInForce = perp * Mathf.Exp(Mathf.Abs(angularDeviation/90))/1000;
	// balances out fallInForce to keep it behind the leader
	// var stayInForce = perp * Mathf.Exp(-Mathf.Abs(angularDeviation) + 4);
	// move away from the path if in front
	var makeWayForce = -perp * Mathf.Exp(-Mathf.Abs(angularDeviation)/90) * 1.2f;
	var followForce = leaderVel.normalized * 0.01f;
	var minLeaderDistance = 3.0f;
	// check if in front of the leader or too close
	if (Mathf.Abs(angularDeviation) > 90 || Mathf.Abs(diff.magnitude) < minLeaderDistance)
	{
	    // only make way if within 2m
	    if (Mathf.Abs(diff.magnitude) < minLeaderDistance)
	    {
		return makeWayForce;
	    }
	    else
	    {
		return -GetVelocity();
	    }
	}


	// if were close to perfectly behind, just follow
	if (Mathf.Abs(diff.magnitude * Mathf.Sin(angularDeviation)) < 2
	    && Mathf.Abs(angularDeviation) < 10)
	{
	    fallInForce = Vector3.zero;
	}
	var direction = followForce + fallInForce;
	var desiredVel = direction.normalized * DesiredSpeed;
        var actVel = rb.velocity;
        return mass * (desiredVel - actVel) / Parameters.T;

    }

    public void ApplyForce()
    {
        var force = ComputeForce();
        force.y = 0;

        rb.AddForce(force * 10, ForceMode.Force);
    }

    public void OnTriggerEnter(Collider other)
    {
       if (AgentManager.IsAgent(other.gameObject))
        {
            perceivedNeighbors.Add(other.gameObject);
        }
       if (WallManager.IsWall(other.gameObject))
        {
            adjacentWalls.Add(other.gameObject);
        }
    }
    
    public void OnTriggerExit(Collider other)
    {
        if (perceivedNeighbors.Contains(other.gameObject))
        {
            perceivedNeighbors.Remove(other.gameObject);
        }
        if (adjacentWalls.Contains(other.gameObject))
        {
            adjacentWalls.Remove(other.gameObject);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {

    }

    public void OnCollisionExit(Collision collision)
    {
        
    }

    #endregion
}
