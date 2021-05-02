using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Handicappedmanager : MonoBehaviour {

    public int count;
    public int Handicapcount;
    public Text HandicapText;

    // Start is called before the first frame update
    void Start()
    {
        count = 0;
        Handicapcount = 0;
        HandicapText.text = "Handicapped: ";
    }

    //void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.tag == "Handicapped")
    //    {
    //        Handicapcount = Handicapcount + 1;
    //        HandicapText.text = "Handicapped: " + HandicapText.ToString();
    //    }
    //}
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Handicapped")
        {
            Handicapcount = Handicapcount + 1;
            HandicapText.text = "Handicapped: " + HandicapText.ToString();
            count = count + 1;
            CollisionCountText();
        }
    }

    void CollisionCountText()
    {
        Handicapcount = Handicapcount + 1;
        HandicapText.text = "Handicapped: " + HandicapText.ToString();
    }
}
