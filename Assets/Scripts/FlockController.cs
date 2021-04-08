using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockController : MonoBehaviour {

    //private int flockSize = 20;
    ////meant to control the speed
    //private float speedModifer = 5;

    //[SerializeField]
    ////alignment weight of objects
    //private float alignmentWeight = 1;

    //[SerializeField]
    ////cohesion weight of objects
    //private float cohesionWeight = 1;

    //[SerializeField]
    ////seperation weight of objects
    //private float seperationWeight = 1;

    //[SerializeField]
    ////follow weight
    //private float followWeight = 5;

    //[SerializeField]
    //private Boid prefab;

    //[SerializeField]
    ////spawn radius of object
    //private float spawnRadius = 3.0f;
    ////spawn location of objects
    //private Vector3 spawnLocation = Vector3.zero;

    //[SerializeField]
    //public Transform target;

    //// Start is called before the first frame update
    //private void Awake()
    //{
    //    //use List element in order to organize flocksize through Boid script which calculates directiom, target diretcion & speed for agents
    //    flockList = new List<Boid>(flocksize);
    //    //repeat iteration of generating spawing of prefabs and position of movement in random areas
    //    for (int i = 0; i < flockSize; i++)
    //    {
    //        //random spawn location with in sphere & radius of of all agents with awareness
    //        spawnLocation = Random.insideUnitSphere * spawnRadius + transform.position;
    //        //render spawn material
    //        Boid boid = Instantiate(prefab, spawnLocation, transform.rotation) as Boid;
    //        //move the transformed object
    //        boid.transform.parent = transform;
    //        //boid uses flockcontroller data
    //        boid.FlockController = this;
    //        //add characteristics of boid to flocklist variable
    //        flockList.Add(boid);
    //    }
    //}

    ////implement position and direction of boid objects
    //public Vector3 Flock(Boid boid, Vector3 boidPosition, Vector3 boidDirection)
    //{
    //    //direction set to 0
    //    flockDirection = Vector3.zero;
    //    //center start set to 0
    //    flockCenter = Vector3.zero;
    //    //target direction set to 0
    //    targetDirection = Vector3.zero;
    //    //seperation weight set to 0
    //    seperationWeight = Vector3.zero;

    //    for (int i = 0; i < flockList.Count; i++)
    //    {
    //        boid neighbor = flockList[i];
    //        //Check only against neighbor
    //        if (neighbor != boid)
    //        {
    //            //Aggregate the direction of all the boids.
    //            flockDirection += neighbor.Direction;
    //            //Aggregate the position of all the boids.
    //            flockCenter += neighbor.transform.localPosition;
    //            //Aggregate the delta to all the boids.
    //            separation += neighbor.transform.localPosition - boidPosition;
    //            separation *= -1;
    //        }
    //    }
    //    //Alignment. The average direction of all boids
    //    flockDirection /= flockSize;
    //    flockDirection = flockDirection.normalized * alignmentWeight;

    //    //Cohesion. The centroid of the flock.
    //    flockCenter /= flockSize;
    //    flockCenter = flockCenter.normalized * cohesionWeight;

    //    //Seperation.
    //    seperationWeight /= flockSize;
    //    seperation = seperationWeight.normalized * seperationWeight;

    //    //Direction vector to the target of the flock.
    //    targetDirection = target.localPosition - boidPosition;
    //    targetDirection = targetDirection * followWeight;

    //    return flockDirection + flockCenter + separation + targetDirection;
    //}

}
