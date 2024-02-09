using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class TankData : MonoBehaviour
{
    public Movement movement;
    public Projectiles projectiles;
    public TankInformation tankInfo;

    [System.Serializable]
    public class Movement
    {
        public float forwardSpeed = 3;
        public float reverseSpeed = 1;
        public float turnSpeed = 180;
    }
    [System.Serializable]
    public class Projectiles
    {
        public float shellSpeed = 500;
        public float fireDelay = 1;
        public float shellLife = 5;
        public float damage = 1;
    }
    [System.Serializable]
    public class TankInformation
    {
        public float health = 100;
        public float healthMax = 100;
        public float pointValue = 10;
        public int playerLives = 3;
    }

    [Header("Reference")]
    public float scoretracker;



}
