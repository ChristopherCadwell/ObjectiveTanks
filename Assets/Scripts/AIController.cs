using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TankData))]
[RequireComponent(typeof(TankMotor))]


public class AIController : MonoBehaviour
{
    public enum Personality { Full, Faker, Kamikaze, Ambusher };//FSM Personalitys
    public Personality personality = Personality.Full;//default to full
    public enum LoopType { Stop, Loop, ReverseLoop }//Types of patrol loop
    public LoopType loopType = LoopType.ReverseLoop;//default to reverse loop
    public enum AIState { Patrol, Chase, ChaseAndFire, Flee, Rest };//just to see what state we are in
    public AIState aiState = AIState.Patrol;//default to patrol


    public Transform[] waypoints;//holder for waypoints
    public float fleeDistance = 1.0f;//
    public float stateEnterTime; // time state was entered
    public float aiSenseRadius = 5; //how far can you be heard
    public float restingHealRate = 25; //hp per second
    public float closeEnough = 1.0f; // distance for waypoints
    public float avoidanceTime = 2.0f;//how long to avoide

    private int currentWaypoint = 0; //waypoint tracker
    private float exitTime;//holder for time till next stage
    private int avoidanceStage = 0;//stage tracker
    public float lastShot = 0; //variable for timer
    public float time;

    private RaycastHit vision;//variable for is something in vision
    private Transform tf;//the location of this unit
    private bool isPatrolForward = true;
    private TankMotor motor;
    private TankData data;
    public float p1Distance;
    public float p2Distance;



    void Awake()
    {
        //set components
        data = gameObject.GetComponent<TankData>();
        motor = gameObject.GetComponent<TankMotor>();
        tf = gameObject.GetComponent<Transform>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {
        time = Time.time;
        if (GameManager.instance.gameRunning == true)
        {
            Closest();//find closest player

            //Personality 1
            if (personality == Personality.Full)
            {
                if (aiState == AIState.Patrol)
                {
                    // Check for avoid
                    if (avoidanceStage != 0)
                    {

                        DoAvoidance();
                    }
                    else
                    {
                        DoPatrol();
                    }

                    // Check transition
                    if (data.tankInfo.health <= data.tankInfo.healthMax * 0.5f)
                    {
                        ChangeState(AIState.Flee);
                    }
                    else if (CanISeeYou() == true)//I see you
                    {
                        ChangeState(AIState.ChaseAndFire);
                    }
                }

                else if (aiState == AIState.ChaseAndFire)
                {
                    // Check for avoid
                    if (avoidanceStage != 0)
                    {
                        DoAvoidance();
                    }
                    else
                    {

                        DoChase();//chase player

                        // Fire, based on rate
                        if (Time.time >= data.projectiles.fireDelay + lastShot)
                        {
                            lastShot = Time.time;
                            motor.FireShell();
                        }
                    }
                    // Check transition
                    if (data.tankInfo.health <= data.tankInfo.healthMax * 0.5f)
                    {
                        ChangeState(AIState.Flee);
                    }
                    else if (CanISeeYou() == false)//not seen
                    {
                        ChangeState(AIState.Patrol);
                    }
                }
                else if (aiState == AIState.Flee)
                {
                    // Behavior
                    if (avoidanceStage != 0)
                    {
                        DoAvoidance();
                    }
                    else
                    {
                        DoFlee();
                    }
                    //Transition
                    if (CanISeeYou() == false)
                    {
                        ChangeState(AIState.Rest);
                    }
                }
                else if (aiState == AIState.Rest)
                {
                    // Behavior
                    DoRest();

                    // Transitions
                    if (CanISeeYou() == true)//can see player
                    {
                        ChangeState(AIState.Flee);
                    }
                    else if (data.tankInfo.health >= data.tankInfo.healthMax)//healed
                    {
                        ChangeState(AIState.Patrol);
                    }
                }
            }

            //Personality 2
            if (personality == Personality.Faker)
            {

                if (aiState == AIState.Chase)
                {
                    // Check for avoid
                    if (avoidanceStage != 0)
                    {
                        DoAvoidance();
                    }
                    else
                    {
                        DoChase();//chase player          
                    }
                    if (CanISeeYou() == true)//we are close to player
                    {
                        ChangeState(AIState.Flee);
                    }
                }
                else if (aiState == AIState.Flee)
                {
                    // Behavior
                    if (avoidanceStage != 0)
                    {
                        DoAvoidance();
                    }
                    else
                    {
                        DoFlee();
                    }
                    //Transition
                    if (CanISeeYou() == true)
                    {
                        ChangeState(AIState.Chase);
                    }
                }

            }

            //Personality 3
            if (personality == Personality.Kamikaze)
            {
                if (aiState == AIState.ChaseAndFire)
                {
                    // Check for avoid
                    if (avoidanceStage != 0)
                    {
                        DoAvoidance();
                    }
                    else
                    {

                        DoChase();//chase player

                        // Fire, based on rate
                        if (Time.time >= data.projectiles.fireDelay + lastShot)
                        {
                            lastShot = Time.time;
                            motor.FireShell();
                        }
                    }
                }
            }

            //Personality 4
            if (personality == Personality.Ambusher)
            {
                if (aiState == AIState.Patrol)
                {
                    // Check for avoid
                    if (avoidanceStage != 0)
                    {

                        DoAvoidance();
                    }
                    else
                    {
                        DoPatrol();
                    }

                    // Check transition
                    if (data.tankInfo.health <= data.tankInfo.healthMax * 0.5f)
                    {
                        ChangeState(AIState.Flee);
                    }
                    else if (CanISeeYou() == true)//I see you
                    {
                        ChangeState(AIState.ChaseAndFire);
                    }
                }

                else if (aiState == AIState.ChaseAndFire)
                {
                    // Check for avoid
                    if (avoidanceStage != 0)
                    {
                        DoAvoidance();
                    }
                    else
                    {

                        motor.RotateTowards(Closest().forward, data.movement.turnSpeed);//Look at player


                        //check if we can move "Speed" units away, or 1 second in the future.
                        if (CanMove(data.movement.forwardSpeed))
                        {
                            motor.Move(data.movement.forwardSpeed);
                        }
                        else
                        {
                            avoidanceStage = 1; //enter avoidance stage 1
                        }


                        // Fire, based on rate
                        if (Time.time >= data.projectiles.fireDelay + lastShot)
                        {
                            lastShot = Time.time;
                            motor.FireShell();
                            DoFlee();//get away to fire again
                        }
                    }
                    // Check transition
                    if (data.tankInfo.health <= data.tankInfo.healthMax * 0.5f)
                    {
                        ChangeState(AIState.Flee);
                    }
                    else if (CanISeeYou() == false)
                    {
                        ChangeState(AIState.Patrol);
                    }
                }
                else if (aiState == AIState.Flee)
                {
                    // Behavior
                    if (avoidanceStage != 0)
                    {
                        DoAvoidance();
                    }
                    else
                    {
                        DoFlee();
                    }
                    //Transition
                    if (CanISeeYou() == false)//Can not see player
                    {
                        ChangeState(AIState.Rest);
                    }
                }
                else if (aiState == AIState.Rest)
                {
                    // Behavior
                    DoRest();

                    // Transitions
                    if (CanISeeYou() == true)//can see player
                    {
                        ChangeState(AIState.Flee);
                    }
                    else if (data.tankInfo.health >= data.tankInfo.healthMax)//healed
                    {
                        ChangeState(AIState.Patrol);
                    }
                }
            }

        }
    }//End of Update




    //Functions


    //Flee
    public void DoFlee()
    {

        Vector3 vectorToTarget = Closest().position - tf.position; // Vector from ai to target        
        Vector3 vectorAwayFromTarget = -1 * vectorToTarget; //Reverse vector        
        vectorAwayFromTarget.Normalize(); //Normalize vector      
        vectorAwayFromTarget *= fleeDistance;//set vector length to flee distance

        Vector3 fleePosition = vectorAwayFromTarget + tf.position;//get rotation angle
        motor.RotateTowards(fleePosition, data.movement.turnSpeed);//rotate
        motor.Move(data.movement.forwardSpeed);//move
    }

    //Rest and heal
    public void DoRest()
    {
        data.tankInfo.health += restingHealRate * Time.deltaTime; //Increase health (per second)

        data.tankInfo.health = Mathf.Min(data.tankInfo.health, data.tankInfo.healthMax); //Do not increase health over max
    }

    //State management
    public void ChangeState(AIState newState)
    {
        aiState = newState; //change state

        stateEnterTime = Time.time; //set time to when we changed states
    }

    //Patrol still using looptype
    public void DoPatrol()
    {
        if (loopType == LoopType.Stop)
        {
            //advance to next WP if we have not run out
            if (currentWaypoint < waypoints.Length - 1)
            {
                if (Vector3.SqrMagnitude(waypoints[currentWaypoint].position - tf.position) > (closeEnough * closeEnough))//if not close enough to wp
                {
                    motor.RotateTowards(waypoints[currentWaypoint].position, data.movement.turnSpeed);//rotate to waypoint

                    if (CanMove(data.movement.forwardSpeed))//check if we can move "Speed" units away, or 1 second in the future.
                    {
                        //Move forward
                        motor.Move(data.movement.forwardSpeed);
                    }
                    else
                    {
                        avoidanceStage = 1; //enter avoidance stage 1
                    }
                }
                else if (Vector3.SqrMagnitude(waypoints[currentWaypoint].position - tf.position) < (closeEnough * closeEnough))//close to wp
                {
                    currentWaypoint++;
                }

            }

        }
        else if (loopType == LoopType.Loop)
        {

            //advance to next WP if we have not run out
            if (currentWaypoint < waypoints.Length - 1)
            {
                if (Vector3.SqrMagnitude(waypoints[currentWaypoint].position - tf.position) > (closeEnough * closeEnough))//if not close enough to wp
                {
                    motor.RotateTowards(waypoints[currentWaypoint].position, data.movement.turnSpeed);//rotate to waypoint

                    if (CanMove(data.movement.forwardSpeed))//check if we can move "Speed" units away, or 1 second in the future.
                    {
                        //Move forward
                        motor.Move(data.movement.forwardSpeed);
                    }
                    else
                    {
                        avoidanceStage = 1; //enter avoidance stage 1
                    }
                }
                else if (Vector3.SqrMagnitude(waypoints[currentWaypoint].position - tf.position) < (closeEnough * closeEnough))//close to wp
                {
                    currentWaypoint++;
                }

            }
            else
            {
                currentWaypoint = 0;
            }

        }
        else if (loopType == LoopType.ReverseLoop)
        {
            if (isPatrolForward)//direction of loop
            {

                //advance to next WP if we have not run out
                if (currentWaypoint < waypoints.Length - 1)
                {

                    if (Vector3.SqrMagnitude(waypoints[currentWaypoint].position - tf.position) > (closeEnough * closeEnough))//if not close enough to wp
                    {
                        motor.RotateTowards(waypoints[currentWaypoint].position, data.movement.turnSpeed);//rotate to waypoint

                        if (CanMove(data.movement.forwardSpeed))//check if we can move "Speed" units away, or 1 second in the future.
                        {
                            //Move forward
                            motor.Move(data.movement.forwardSpeed);
                        }
                        else
                        {
                            avoidanceStage = 1; //enter avoidance stage 1
                        }
                    }
                    else if (Vector3.SqrMagnitude(waypoints[currentWaypoint].position - tf.position) < (closeEnough * closeEnough))//close to wp
                    {
                        currentWaypoint++;
                    }
                }
            }
            else
            {
                isPatrolForward = false; //Reverse Direction
                currentWaypoint--;
            }

            //continue advancing if not out of wps
            if (currentWaypoint > 0)
            {
                if (Vector3.SqrMagnitude(waypoints[currentWaypoint].position - tf.position) > (closeEnough * closeEnough))//if not close enough to wp
                {
                    motor.RotateTowards(waypoints[currentWaypoint].position, data.movement.turnSpeed);//rotate to waypoint

                    if (CanMove(data.movement.forwardSpeed))//check if we can move "Speed" units away, or 1 second in the future.
                    {
                        //Move forward
                        motor.Move(data.movement.forwardSpeed);
                    }
                    else
                    {
                        avoidanceStage = 1; //enter avoidance stage 1
                    }
                }
                else if (Vector3.SqrMagnitude(waypoints[currentWaypoint].position - tf.position) < (closeEnough * closeEnough))//close to wp
                {
                    currentWaypoint--;
                }
            }
        }
        else
        {
            //no wps left, reverse direction
            isPatrolForward = true;
            currentWaypoint++;
        }
    }

    //Chase
    void DoChase()
    {
        motor.RotateTowards(Closest().position, data.movement.turnSpeed);//Look at player

        //check if we can move "Speed" units away, or 1 second in the future.
        if (CanMove(data.movement.forwardSpeed))
        {
            motor.Move(data.movement.forwardSpeed);
        }
    
        else
        {
            avoidanceStage = 1; //enter avoidance stage 1
        }

    }

    //Avoid function
    void DoAvoidance()
    {
        if (avoidanceStage == 1)
        {
            motor.Rotate(-1 * data.movement.turnSpeed);// turn left
            //If we can now move, proceed to stage 2                                  
            if (CanMove(data.movement.forwardSpeed))
            {
                avoidanceStage = 2;
                exitTime = avoidanceTime;//how long to stay in stage 2
            }
            //if not, do it again        
        }
        else if (avoidanceStage == 2)
        {   //move if we can
            if (CanMove(data.movement.forwardSpeed))
            {
                exitTime -= Time.deltaTime;//subtract time from exit time
                motor.Move(data.movement.forwardSpeed);//move

                if (exitTime <= 0)//if time is up
                {
                    motor.RotateTowards(waypoints[currentWaypoint].position, data.movement.turnSpeed);
                    avoidanceStage = 0;//resume normal operations
                }
            }
            else
            {
                //if we still can't move, back to avoidance 1
                avoidanceStage = 1;
            }
        }
    }

    // Checks if unit can move "speed" units forward, true/false
    bool CanMove(float speed)
    {
        //raycast forward, check for collisions
        RaycastHit hit;
        //if we hit something.
        if (Physics.Raycast(tf.position, tf.forward, out hit, speed))
        {
            //did not hit player
            if (!hit.collider.CompareTag("Player"))
            {
                //obstacle, can not move
                return false;
            }

        }
        //no collisions?
        return true;
    }
    //can the player be senced
    public bool CanISeeYou()
    {
        if ((Vector3.Distance(Closest().position, tf.position) <= aiSenseRadius) )
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    //see who is closer
    public Transform Closest()
    {
        if ((GameManager.instance.p1dead == false) && (GameManager.instance.p2dead == false))
        {
            p1Distance = Vector3.Distance(GameManager.instance.p1T.position, tf.position);
            p2Distance = Vector3.Distance(GameManager.instance.p2T.position, tf.position);
            if (p1Distance > p2Distance)
            {
                return GameManager.instance.p1T;
            }
            else
            {
                return GameManager.instance.p2T;
            }

        }
        else if((GameManager.instance.p1dead == true) && (GameManager.instance.p2dead == false))
        {
            return GameManager.instance.p2T;
        }
        else if ((GameManager.instance.p1dead == false) && (GameManager.instance.p2dead == true))
        {
            return GameManager.instance.p1T;
        }
        else
        {
            return null;
        }      



    }


}