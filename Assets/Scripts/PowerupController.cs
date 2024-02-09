using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TankData))]

public class PowerupController : MonoBehaviour
{
    public List<PowerUp> powerups;

    private TankData data;

    private void Awake()
    {
        data = gameObject.GetComponent<TankData>();
    }

    // Start is called before the first frame update
    void Start()
    {
        powerups = new List<PowerUp>();
    }

    // Update is called once per frame
    void Update()
    {
        //create a list to hold our expired powerups
        List<PowerUp> expiredPowerups = new List<PowerUp>();

        //Loop through the list
        foreach (PowerUp power in powerups)
        {
            //subtract time
            power.duration -= Time.deltaTime;

            //build a list of expired powerups
            if (power.duration <= 0)
            {
                expiredPowerups.Add(power);
            }
        }

        //after looping through powerups, remove anything from expired list
        foreach (PowerUp power in expiredPowerups)
        {
            power.OnDeactivate(data);
            powerups.Remove(power);
        }

        expiredPowerups.Clear();//clear the list

        //end of update
    }

    public void Add(PowerUp powerup)
    {
        powerup.OnActivate(data);//run power up on activate

        if (!powerup.isPermanent)//check for permanent
        {
            powerups.Add(powerup);//add to list
        }

    }

}
