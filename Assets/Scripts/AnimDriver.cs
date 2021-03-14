using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimDriver : MonoBehaviour
{
    private Animator anim;
    private float walkRunParam = 0;

    void Start() {
        anim = GetComponent<Animator>(); 
    }

    // Update is called once per frame
    void Update() {
        float x_vel = Input.GetAxis("Horizontal");
        float y_vel = Input.GetAxis("Vertical");

        if (Input.GetKey("left shift") || Input.GetKey("right shift")) {
            walkRunParam = Mathf.Clamp01(walkRunParam + 1 * Time.deltaTime);
        }
        else {
            walkRunParam = Mathf.Clamp01(walkRunParam - 1 * Time.deltaTime);
        }

        if (Input.GetKeyDown("space"))
        {
            anim.SetTrigger("Jump");
        }

        anim.SetFloat("x_vel", x_vel);
        anim.SetFloat("y_vel", y_vel);
        anim.SetFloat("Run", walkRunParam);
    }
}
