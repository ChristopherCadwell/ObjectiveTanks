using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatController : MonoBehaviour
{
    public PowerupController powCon;
    public PowerUp cheatPowerup;

    // Start is called before the first frame update
    void Start()
    {
        if (powCon == null)
        {
            powCon = gameObject.GetComponent<PowerupController>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //key combo activate cheat
        if (Input.GetKey(KeyCode.J) && Input.GetKey(KeyCode.K) && Input.GetKey(KeyCode.L))
        {
            powCon.Add(cheatPowerup);//add cheat
        }
    }
}
