using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Handicappedmanager : MonoBehaviour {

    public int Handicapcount;
    public Text HandicapText;
    public Text EvaccountText;

    public GameObject plane;
    private GameManager Evaccount;

    // Start is called before the first frame update
    void Start()
    {
        Handicapcount = 0;
        HandicapText.text = "Handicapped: ";

        Evaccount = plane.GetComponent<GameManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Handicapped")
        {
            Handicapcount++;
            Evaccount.Evaccountcount++;

            HandicapText.text = "Handicapped: " + Handicapcount.ToString();
            EvaccountText.text = "Evacuation: " + Evaccount.Evaccountcount.ToString();
        }
    }
}
