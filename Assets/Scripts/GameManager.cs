using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public int count;
    public int Evaccountcount;
    public Text EvaccountText;



    // Start is called before the first frame update
    void Start()
    {
        count = 0;
        Evaccountcount = 0;
        EvaccountText.text = "Evacuation: ";
    }

 

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            //count = count + 1;
            //CollisionCountText();
            Evaccountcount = Evaccountcount + 1;
            EvaccountText.text = "Evacuation: " + Evaccountcount.ToString();
        }
    }
    void OnTriggerEnter(Collider other)
    {
        count = count + 1;
        CollisionCountText();
    }

    void CollisionCountText()
    {
        Evaccountcount = Evaccountcount + 1;
        EvaccountText.text = "Evacuation: " + Evaccountcount.ToString();
    }
}
