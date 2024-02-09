using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this clss is put on all pickup prefabs

public class Pickup : MonoBehaviour
{
    public PowerUp powerup;//get stats for inspector from the powerup script
    public static Transform powerPopLocation;//store collision point for popup
    public float timout;//timer for determining what to destroy
    private Transform tf;
    

    private void Awake()
    {
        tf = gameObject.transform;

    }
   
    public void OnTriggerEnter(Collider other)
    {
        PowerupController powCon = other.GetComponent<PowerupController>();//store other objects controller if exists  
        if (other.gameObject.tag == "Powerup")
        {
            if (gameObject.GetComponent<Pickup>().timout > other.gameObject.GetComponent<Pickup>().timout)
            {
                Destroy(gameObject);
            }
        }
        //if other object does have a power up controller
        if (powCon != null)
        {
            //show popups for players only
            if(other.gameObject.tag != "Enemy")
            {
                string popName = gameObject.name;//set string for damage ammount
                powerPopLocation = other.gameObject.transform;//get location of colliding object
                PopUpController.CreatePowerupText(popName, powerPopLocation);//pass powerup name and location to popup
            }
            
            powCon.Add(powerup); //add powerup


            //seperate p1 and p2 for lives tracking.
            if (other.gameObject.name == "Player 1" && powerup.livesModifier > 0)
            {
                GameManager.instance.lives = GameManager.instance.lives + powerup.livesModifier;
            }
            if (other.gameObject.name == "Player 2" && powerup.livesModifier > 0)
            {
                GameManager.instance.p2Lives = GameManager.instance.p2Lives + powerup.livesModifier;
            }
            AudioSource.PlayClipAtPoint(AudioController.instance.powerClip, transform.position, GameManager.instance.sfxValue);//play audio 
            Destroy(gameObject);

        }

    }
    
}
