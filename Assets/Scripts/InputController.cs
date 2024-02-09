using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TankData))]
[RequireComponent(typeof(TankMotor))]

public class InputController : MonoBehaviour
{
    private TankMotor motor; //Get motor from TankMotor
    private TankData data; //Get data from TankData
    public enum InputScheme { WASD, arrowKeys }; //Define types of input
    public InputScheme input = InputScheme.WASD; // Default to WASD

    private float pLastShot; //variable for timer


    // Start is called before the first frame update
    void Start()
    {
        data = gameObject.GetComponent<TankData>();
        motor = gameObject.GetComponent<TankMotor>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (input)
        {
            case InputScheme.WASD:
                if (Input.GetKey(KeyCode.W))
                {
                    motor.Move(data.movement.forwardSpeed);
                }

                if (Input.GetKey(KeyCode.S))
                {
                    motor.Move(-data.movement.reverseSpeed);
                }

                if (Input.GetKey(KeyCode.D))
                {
                    motor.Rotate(data.movement.turnSpeed);
                }

                if (Input.GetKey(KeyCode.A))
                {
                    motor.Rotate(-data.movement.turnSpeed);
                }

                if (Input.GetKey(KeyCode.Escape))
                {
                    GameManager.instance.isPaused = true;
                }

                if (Input.GetKey(KeyCode.Space))
                {
                    //timere down, and button press, call fireshell from game manager
                    if (Time.time >= data.projectiles.fireDelay + pLastShot)
                    {
                        pLastShot = Time.time;
                        motor.FireShell();
                    }

                }
                break;

            case InputScheme.arrowKeys:

                if (Input.GetKey(KeyCode.UpArrow))
                {
                    motor.Move(data.movement.forwardSpeed);
                }

                if (Input.GetKey(KeyCode.DownArrow))
                {
                    motor.Move(-data.movement.reverseSpeed);
                }

                if (Input.GetKey(KeyCode.RightArrow))
                {
                    motor.Rotate(data.movement.turnSpeed);
                }

                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    motor.Rotate(-data.movement.turnSpeed);
                }

                if (Input.GetKey(KeyCode.Escape))
                {
                    GameManager.instance.isPaused = true;
                }

                if (Input.GetKey(KeyCode.RightControl))
                {
                    //timere down, and button press, call fireshell from game manager
                    if (Time.time >= data.projectiles.fireDelay + pLastShot)
                    {
                        pLastShot = Time.time;
                        motor.FireShell();
                    }

                }
                break;

        }

    }

}
