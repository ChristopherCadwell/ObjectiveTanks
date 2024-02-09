using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class PowerUp
{
    public float duration;
    public float speedModifier;
    public float healthModifier;
    public float maxHealthModifier;
    public float fireRateModifier;
    public float damageModifier;
    public int livesModifier;
    public bool isPermanent;

    public void OnActivate(TankData target)
    {
        //modifiers to add
        target.movement.forwardSpeed += speedModifier;
        target.tankInfo.health += healthModifier;        
        target.tankInfo.healthMax += maxHealthModifier;
        target.projectiles.fireDelay -= fireRateModifier;
        target.projectiles.damage += damageModifier;
        target.tankInfo.playerLives += livesModifier;
               
    }

    public void OnDeactivate(TankData target)
    {
        //mods to disable
        target.movement.forwardSpeed -= speedModifier;
        target.tankInfo.health -= healthModifier;
        target.tankInfo.healthMax -= maxHealthModifier;
        target.projectiles.fireDelay += fireRateModifier;
        target.projectiles.damage -= damageModifier;
    }


}
