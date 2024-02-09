using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TankData))]

public class TankMotor : MonoBehaviour
{

   [Header("Game Objects, must be set")]
    public Rigidbody shell; //Shell prefab for instantiation
    public Transform cannon; //get location to fire shells from
    [Header("Reference")]
    public Transform tf; //variable for storing the transform data  

    private CharacterController characterController; //variable for character controller component
    private TankData data; //holder for tank data
    private Rigidbody firedShell; //instance variable to hold shells


    public void Awake()
    {
        tf = gameObject.GetComponent<Transform>(); //store transform in variable
        characterController = gameObject.GetComponent<CharacterController>(); //store teh character controller in variable
        data = gameObject.GetComponent<TankData>();//get tank data
    }

    // Move() - This function moves our tank forward.
    public void Move( float speed)
    {
    Vector3 speedVector; //create vector to hold speed data
    speedVector = tf.forward; //point vector the samd direction as gameobject.
    speedVector *= speed; //apply speed value
    characterController.SimpleMove (speedVector); //call SimpleMove() and send the vector
    }

    //Rotation function
    public void Rotate( float speed)
    {
        Vector3 rotateVector; //variable for ratation data
        rotateVector = Vector3.up; //assign rotation axis
        rotateVector *= speed; //set rotation speed to current speed
        rotateVector *= Time.deltaTime; //match speed to time instead of frame
        tf.Rotate(rotateVector, Space.Self); // apply rotation in local space
    }

    //RotateTowards (target, speed) - rotates towars the target if possible.
    //if we can rotate, return true, if not return false
    public bool RotateTowards (Vector3 target, float speed )
    {
        Vector3 vectorToTarget;
        //The vector to our target is the difference between target.position and our position.
        vectorToTarget = target - tf.position;

        //find quaternion looking down vector
        Quaternion targetRotation = Quaternion.LookRotation(vectorToTarget);

        //If we are already looking at target, don't rotate
        if ( targetRotation == tf.rotation)
        {
            return false;
        }

        //if not, rotater towards target *delta time to rotate in degrees per second
        tf.rotation = Quaternion.RotateTowards(tf.rotation, targetRotation, data.movement.turnSpeed * Time.deltaTime);

        //rotation complete, return true
        return true;

    }

    //fire command
    public void FireShell()
    {
        //Instantiate bullet from tip of cannon
        firedShell = Instantiate(shell, cannon.position, cannon.rotation, transform.parent) as Rigidbody;
        firedShell.transform.parent = transform;//set parent to instantiating object
        firedShell.AddForce(cannon.forward * data.projectiles.shellSpeed);//add firespeed force

        AudioSource.PlayClipAtPoint(AudioController.instance.cannonFire, transform.position, GameManager.instance.sfxValue);//play sound from here with sfx volume

    }

}
