using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandicappedNavigation : MonoBehaviour {

    public GameObject handicap;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Finish")
        {
            handicap.SetActive(false);
        }
    }
}
