using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShellController : MonoBehaviour
{
    private TankData data;
    private float sDamage;
    private TankData cData;
    private string shotFrom;
    public static Transform popLocation;

    private void Start()
    {

        data = gameObject.GetComponentInParent<TankData>();//set tank data to parent objects info
        shotFrom = transform.parent.name;
        Destroy(gameObject, data.projectiles.shellLife);//destroy shell after lifespan ends
        sDamage = data.projectiles.damage;//set shell damage to parents damage value   
        transform.parent = null;//after getting values, orphan so that turning does not effect trajectory
    }


    void OnTriggerEnter(Collider col) //collisions
    {
        //stop shooting yourself
        if (col.gameObject.name != shotFrom)
        {
            AudioSource.PlayClipAtPoint(AudioController.instance.shellExplode, transform.position, GameManager.instance.sfxValue); //play sound from here with sfx volume

            if (col.gameObject.tag == "Enemy")//If a bullet hits an enemy 
            {
                cData = col.gameObject.GetComponent<TankData>();//get tank data from colliding entity
                cData.tankInfo.health = cData.tankInfo.health - sDamage;//apply damage to object
                                                                        //Only show popups from player shots
                if (shotFrom == "Player 1" || shotFrom == "Player 2")
                {
                    string popAmount = data.projectiles.damage.ToString();//set string for damage ammount
                    popLocation = col.gameObject.transform;//get location of colliding object
                    PopUpController.CreateDamageText(popAmount, popLocation);//pass damage amount and location to popup
                }

                if (cData.tankInfo.health <= sDamage)//if this kills our enemy
                {
                    data.scoretracker = data.scoretracker + cData.tankInfo.pointValue;//add the point value to score
                    Destroy(col.gameObject);//destroy the object we hit
                    AudioSource.PlayClipAtPoint(AudioController.instance.tankDeath, transform.position, GameManager.instance.sfxValue);//play sound from here with sfx volume
                }
            }
            else if (col.gameObject.tag == "Player")//If a bullet hits a player
            {
                cData = col.gameObject.GetComponent<TankData>();//get tank data from colliding entity
                cData.tankInfo.health = cData.tankInfo.health - sDamage;//apply damage to object
                string popAmount = data.projectiles.damage.ToString();//set string for damage ammount
                popLocation = col.gameObject.transform;//get location of colliding object
                PopUpController.CreateDamageText(popAmount, popLocation);//pass damage amount and location to popup

                if (cData.tankInfo.health <= sDamage)//if this kills the player
                {
                    //find what player, and tell GM they are dead
                    if (col.gameObject.name == "Player 1")
                    {
                        GameManager.instance.p1dead = true;
                    }
                    else if (col.gameObject.name == "Player 2")
                    {
                        GameManager.instance.p2dead = true;
                    }
                    Destroy(col.gameObject);//destroy the object we hit
                    AudioSource.PlayClipAtPoint(AudioController.instance.tankDeath, transform.position, GameManager.instance.sfxValue);//play sound from here with sfx volume
                }
            }
        }
        //destroy on non bullet collisions

        Destroy(gameObject);//collision -> destroy bullet
        AudioSource.PlayClipAtPoint(AudioController.instance.shellExplode, transform.position, GameManager.instance.sfxValue);//play sound from here with sfx volume


    }
}
