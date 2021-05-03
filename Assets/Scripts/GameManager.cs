using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public int Evaccountcount;
    public Text EvaccountText;
    public Text HandicapText;

    public GameObject finish;
    private Handicappedmanager Handicapped;

    // Start is called before the first frame update
    void Start()
    {
        Evaccountcount = 0;
        EvaccountText.text = "Evacuation: ";

        Handicapped = finish.GetComponent<Handicappedmanager>();
    }

    void OnTriggerEnter(Collider other)
    {
        Evaccountcount++;
        EvaccountText.text = "Evacuation: " + Evaccountcount.ToString();

        if (other.gameObject.tag == "Handicapped") {
            Handicapped.Handicapcount++;
            HandicapText.text = "Handicapped: " + Handicapped.Handicapcount.ToString();
        }
    }
}
