using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;//set instance for reference elsewhere
    [Header("Set these")]
    public TankData p1Data;//get player tankData--  Assign these in editor to prefab for preloading player information
    public TankData p2Data;//get player tankData
    public ButtonController bc;

    [Header("Map generation")]
    public int rows;
    public int cols;
    public int killsToLevel = 2;
    public int mapSeed = 0;
    public bool mapOfTheDay = false;
    public bool randomMap = true;

    [Header("Spawn Control")]
    public int maxEnemies; //The max number of enemies spawned at once
    public float pickupSpawnDelay = 1;//time between spawns
    public int pickupsToSpawn = 5;// number of pickups to spawn at once

    [Header("Prefabs")]
    public GameObject player1Pre;//Player 1, for instantiate    
    public GameObject player2Pre;//Player 2, for instantiate
    public GameObject[] gridPrefabs;//The list of prefabs to spawn grid from
    public GameObject[] pickupPrefab;//store the powerups to spawn in an array
    public GameObject[] enemyPrefab;//Holder for enemy Array  

    [Header("HuD")]
    public Text textScoreP1;
    public Text textHighScoreP1;
    public Text textHealthP1;
    public Text textLivesP1;
    public Text textScoreP2;
    public Text textHighScoreP2;
    public Text textHealthP2;
    public Text textLivesP2;

    [Header("Reference (All autofilled, should not be changed)")]
    public float score;
    public float p2Score;
    public float highScore;
    public int lives;
    public int p2Lives;
    public bool p1dead = true;//tracking if there is a player1
    public bool p2dead = true;//tracking if player 2
    public List<Transform> eSpawnPoints;//holder for enemy spawn points
    public List<Transform> p1Spawn;//player spawn point
    public List<Transform> powerupSpawnPoints;//our power up spawn locations
    public Transform p1T; //Player 1s transform
    public Transform p2T;//p2 transform
    public bool gameRunning = false;//Are we playing the game
    public bool players2 = false;//1 or 2 players
    public bool isPaused = false;//determine if we should pause
    public float musicValue;
    public float sfxValue;

    private float roomWidth = 50;
    private float roomHeight = 50;
    private int eNumber;//how many enemies are alive   
    private Room[,] grid;
    private GameObject player1;//instance to hold player1
    private GameObject player2;//instance to hold player2
    private GameObject spawnedPickup;//current spawn
    private float nextSpawnTime;//how long till next powerup spawn


    void Awake()
    {
        //Singleton  only one instance
        if (GameManager.instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        Load();//load PlayerPrefs from disk
        PopUpController.Initialize();//start damage popup script
    }


    //every frame
    void Update()
    {
        //while the game is running
        if (gameRunning == true)
        {
            //if ispaused bool is on
            if (isPaused == true)
            {
                bc.PauseGame();
            }
            //disable UI audio listener

            GameObject uia = GameObject.Find("UICanvas");
            AudioListener uial = uia.GetComponent<AudioListener>();
            uial.enabled = false;

            //Show values on screen, and keep them updated
            textScoreP1.text = "Score:" + score;
            textHighScoreP1.text = "High Score: " + highScore;
            textHealthP1.text = "Health: " + p1Data.tankInfo.health;
            textLivesP1.text = "Lives:" + lives;

            //if 2 players, hud for p2
            if (players2 == true)
            {
                textScoreP2.text = "Score:" + p2Score;
                textHighScoreP2.text = "High Score: " + highScore;
                textHealthP2.text = "Health: " + p2Data.tankInfo.health;
                textLivesP2.text = "Lives:" + p2Lives;
            }

            //spawn enemys
            EnemySpawn();

            //add score
            if (score > highScore)//if score is higher than high score
            {
                highScore = score;//update high score
                Save();//save high score
            }

            //same as for p1
            if (p2Score > highScore)
            {
                highScore = p2Score;
                Save();
            }

            //player respawn
            if (p1dead)
            {
                SpawnPlayer1();

            }
            if ((p2dead) && (players2))
            {
                SpawnPlayer2();
            }
            //out of lives,no players left, game over
            if ((lives <= 0) && (p2Lives <= 0) && (p1dead == true) && (p2dead == true))
            {
                bc.GameOver();
            }

            //p1 gone, enable p2 audio listener
            if ((lives <= 0) && (p1dead == true))
            {
                GameObject al2 = GameObject.Find("Player 2");
                AudioListener al2a = al2.GetComponent<AudioListener>();
                al2a.enabled = true;
            }

        }

        //add scores
        if (p1Data.scoretracker >= 1)
        {
            score = score + p1Data.scoretracker;//add score from player tankdata
            p1Data.scoretracker = p1Data.scoretracker - p1Data.scoretracker;//zero player tank data score
        }
        if (p2Data.scoretracker >= 1)
        {
            p2Score = p2Score + p2Data.scoretracker;//add score from player tankdata
            p2Data.scoretracker = p2Data.scoretracker - p2Data.scoretracker;//zero player tank data score
        }

        //end of Update  
    }

    //moved powerup spawn to fixed update to properly track time.
    void FixedUpdate()
    {
        if(gameRunning == true)
        {
            //spawn powerups
            if (Time.time > nextSpawnTime)
            {

                //spawn new ones
                PickupSpawner();
            }
        }
    }

    void LateUpdate()
    {
        if (players2 == true)
        {
            //keep camera at half screen (late update because update happens too fast to get the object)
            Camera p1cam = GameObject.Find("Player 1").GetComponentInChildren<Camera>();
            p1cam.rect = new Rect(0f, 0f, 1f, 0.5f);

            if (p1dead == false)
            {

                //disable p2 audio listener until it is needed.
                GameObject al2 = GameObject.Find("Player 2");
                AudioListener al2a = al2.GetComponent<AudioListener>();
                al2a.enabled = false;
            }
        }
    }
    //functions


    //start the game
    public void StartGame()
    {

        p1Spawn = new List<Transform>();//initialize spawn points
        eSpawnPoints = new List<Transform>();//initialize enemy spawn points
        powerupSpawnPoints = new List<Transform>();//initialize powerup spawn points  
        nextSpawnTime = Time.time + pickupSpawnDelay;//set timer for next spawn
        players2 = false;

        //initialize stats
        lives = p1Data.tankInfo.playerLives;//lives match tank data

        //map of the day
        if (mapOfTheDay)
        {
            mapSeed = DateToInt(DateTime.Now.Date);//set seed value for current day
        }

        //random map
        if (randomMap)
        {
            mapSeed = DateToInt(DateTime.Now);//set seed value for current time
        }

        GenerateGrid();//Generate the grid

        //Fill the lists with spawn location transforms
        foreach (GameObject a in GameObject.FindGameObjectsWithTag("PlayerSpawn"))
        {
            p1Spawn.Add(a.GetComponent<Transform>());
        }
        foreach (GameObject b in GameObject.FindGameObjectsWithTag("Enemy Spawn"))
        {
            eSpawnPoints.Add(b.GetComponent<Transform>());
        }
        foreach (GameObject c in GameObject.FindGameObjectsWithTag("PowerupSpawn"))
        {
            powerupSpawnPoints.Add(c.GetComponent<Transform>());
        }

        //spawn player
        SpawnPlayer1();

        //set our bool for is game runnign to true
        gameRunning = true;

        //set up HUD
        score = 0;
        textScoreP1.text = "Score:" + score;
        textHighScoreP1.text = "High Score: " + highScore;
        textHealthP1.text = "Health: " + p1Data.tankInfo.health;
        textLivesP1.text = "Lives:" + lives;
    }

    public void StartGame2p()
    {

        p1Spawn = new List<Transform>();//initialize spawn points
        eSpawnPoints = new List<Transform>();//initialize enemy spawn points
        powerupSpawnPoints = new List<Transform>();//initialize powerup spawn points  
        nextSpawnTime = Time.time + pickupSpawnDelay;//set timer for next spawn

        //initialize stats
        lives = p1Data.tankInfo.playerLives;//lives match tank data
        p2Lives = p2Data.tankInfo.playerLives;//same as lives

        //map of the day
        if (mapOfTheDay)
        {
            mapSeed = DateToInt(DateTime.Now.Date);//set seed value for current day
        }

        //random map
        if (randomMap)
        {
            mapSeed = DateToInt(DateTime.Now);//set seed value for current time
        }
        GenerateGrid();//Generate the grid


        //Fill the lists with spawn location transforms
        foreach (GameObject a in GameObject.FindGameObjectsWithTag("PlayerSpawn"))
        {
            p1Spawn.Add(a.GetComponent<Transform>());
        }
        foreach (GameObject b in GameObject.FindGameObjectsWithTag("Enemy Spawn"))
        {
            eSpawnPoints.Add(b.GetComponent<Transform>());
        }
        foreach (GameObject c in GameObject.FindGameObjectsWithTag("PowerupSpawn"))
        {
            powerupSpawnPoints.Add(c.GetComponent<Transform>());
        }

        //Spawn players
        SpawnPlayer1();
        SpawnPlayer2();

        //after spawning players

        gameRunning = true;//set game running bool true
        players2 = true;//bool for 2p

        //set up HUD or reassign text
        score = 0;
        p2Score = 0;

        textScoreP1.text = "Score:" + score;
        textHighScoreP1.text = "High Score: " + highScore;
        textHealthP1.text = "Health: " + p1Data.tankInfo.health;
        textLivesP1.text = "Lives:" + lives;

        textScoreP2.text = "Score:" + p2Score;
        textHighScoreP2.text = "High Score: " + highScore;
        textHealthP2.text = "Health: " + p2Data.tankInfo.health;
        textLivesP2.text = "Lives:" + p2Lives;

    }

    //player spawn function
    public void SpawnPlayer1()
    {
        //p1 camera now draws underneath of p2 camera, no script needed to seperate the viewscreens

        if (lives >= 1)
        {
            int p1SpawnLocation = UnityEngine.Random.Range(0, p1Spawn.Count);//get a random spawn location
            GameObject player1 = Instantiate(player1Pre, p1Spawn[p1SpawnLocation]) as GameObject;//spawn the player at random location
            player1.transform.parent = transform;//change parent so not a child of spawn point
            player1.name = "Player 1";//rename to Player 1
            p1Data = player1.GetComponent<TankData>();//Get player tank data    
            p1T = player1.transform;//Store players location in variable

            lives--;//remove 1 life
            p1dead = false;//tell GM p1 is alive   
            if (players2 == false)
            {
                p2T = p1T;//set both transforms to the same
            }



        }

        else if (lives <= 0)
        {
            //do nothing
        }
    }

    //spawn p2
    public void SpawnPlayer2()
    {
        if (p2Lives >= 1)
        {
            int p1SpawnLocation = UnityEngine.Random.Range(0, p1Spawn.Count);//get a random spawn location
            GameObject player2 = Instantiate(player2Pre, p1Spawn[p1SpawnLocation]) as GameObject;//spawn the player at random location
            player2.transform.parent = transform;//change parent so not a child of spawn point
            player2.name = "Player 2"; //rename to Player 2
            p2Data = player2.GetComponent<TankData>();//Get player tank data            
            p2Lives--;//remove 1 life from p2
            p2T = player2.transform;//Store players location in variable
            p2dead = false;//tell GM p2 is alive
        }
        else if (p2Lives <= 0)
        {
            //do nothing
        }
    }

    //generate the map
    public void GenerateGrid()
    {
        UnityEngine.Random.InitState(mapSeed);//set map seed

        grid = new Room[cols, rows];//empty the grid

        //for each row
        for (int i = 0; i < rows; i++)
        {
            //for each column in row "i"
            for (int j = 0; j < cols; j++)
            {
                //get location
                float xPosition = roomWidth * j;
                float zPosition = roomHeight * i;
                Vector3 newPosition = new Vector3(xPosition, 0.0f, zPosition);

                //create grid at current location
                GameObject tempRoomObj = Instantiate(RandomRoomPrefab(), newPosition, Quaternion.identity) as GameObject;

                tempRoomObj.transform.parent = this.transform;//set parent

                tempRoomObj.name = "room_" + j + "," + i;//rename

                Room tempRoom = tempRoomObj.GetComponent<Room>();//get room object

                //open doors (north/south)
                if (i == 0 && rows > 1) //if it is the bottom row
                {
                    tempRoom.doorNorth.SetActive(false);//hide north door
                }
                else if (i == rows - 1)//if it is the top row
                {
                    tempRoom.doorSouth.SetActive(false);//don't hide south door
                }
                else//all other rows
                {
                    tempRoom.doorNorth.SetActive(false);//hide north door
                    tempRoom.doorSouth.SetActive(false);//hide south door
                }

                //open doors (east/west)
                if (j == 0 && cols > 1)//if it is the first column
                {
                    tempRoom.doorEast.SetActive(false);//hide east door
                }
                else if (j == cols - 1)//if it is the last column
                {
                    tempRoom.doorWest.SetActive(false);//don't hide west door
                }
                else//all other columns
                {
                    tempRoom.doorEast.SetActive(false);//hide east door
                    tempRoom.doorWest.SetActive(false);//hide west door
                }

                grid[j, i] = tempRoom;//save section to grid array            

            }
        }
    }

    public GameObject RandomRoomPrefab()
    {
        return gridPrefabs[UnityEngine.Random.Range(0, gridPrefabs.Length)];//random room from beginning to end of prefabs array

    }

    public int DateToInt(DateTime dateToUse)
    {
        //add up values of the date and return value
        return (dateToUse.Year + dateToUse.Month + dateToUse.Day + dateToUse.Hour + dateToUse.Minute + dateToUse.Second + dateToUse.Millisecond);
    }

    //spawn powerups
    public void PickupSpawner()
    {
        if (GameObject.FindGameObjectsWithTag("Powerup").Length < pickupsToSpawn)
        {
            int powerupToSpawn = UnityEngine.Random.Range(0, pickupPrefab.Length);//Get a random number from array          
            int powerupSpawnLoc = UnityEngine.Random.Range(0, powerupSpawnPoints.Count);//get a random number from array

            GameObject spawnedPickup = Instantiate(pickupPrefab[powerupToSpawn], powerupSpawnPoints[powerupSpawnLoc]) as GameObject;//spawn object
            spawnedPickup.name = pickupPrefab[powerupToSpawn].name;//rename it (remove the (Clone) )
            spawnedPickup.transform.parent = transform;//set parent to GM
            spawnedPickup.GetComponent<Pickup>().timout = Time.time;
        }
        //pickups are at max
        if (GameObject.FindGameObjectsWithTag("Powerup").Length >= pickupsToSpawn)
        {
            nextSpawnTime = Time.time + pickupSpawnDelay;//reset timer for next spawn
        }
    }

    //spawn enemies
    public void EnemySpawn()
    {
        while (GameObject.FindGameObjectsWithTag("Enemy").Length < maxEnemies)
        {
            int enemyToSpawn = UnityEngine.Random.Range(0, enemyPrefab.Length);//Get a random number from enemy list array
            int enemySpawnLocation = UnityEngine.Random.Range(0, eSpawnPoints.Count);//get a random number from enemy spawn array
            GameObject enemy = Instantiate(enemyPrefab[enemyToSpawn], eSpawnPoints[enemySpawnLocation]) as GameObject;//spawn random enemy at random point
            enemy.transform.name = enemyPrefab[enemyToSpawn].name;
            enemy.transform.parent = transform;//change parent to GameManager
        }
    }

    //save function
    public void Save()
    {
        PlayerPrefs.SetFloat("High Score", highScore);
        PlayerPrefs.SetFloat("Sound Effects", sfxValue);
        PlayerPrefs.SetFloat("Ambient Music", musicValue);
        PlayerPrefs.SetInt("Random Map", (randomMap ? 1 : 0));//true or false
        PlayerPrefs.SetInt("Map of the Day", (mapOfTheDay ? 1 : 0));//true or false
        PlayerPrefs.SetInt("Last Seed Used", mapSeed);
        PlayerPrefs.Save();
    }

    //load function
    public void Load()
    {
        highScore = PlayerPrefs.GetFloat("High Score", highScore);
        sfxValue = PlayerPrefs.GetFloat("Sound Effects", sfxValue);
        musicValue = PlayerPrefs.GetFloat("Ambient Music", musicValue);
        randomMap = (PlayerPrefs.GetInt("Random Map") != 0);//if bool was true
        mapOfTheDay = (PlayerPrefs.GetInt("Map of the Day") != 0);//if bool was true
        mapSeed = PlayerPrefs.GetInt("Last Seed Used", mapSeed);
    }

    //autosave on exit
    public void OnApplicationQuit()
    {
        Save();
    }


    //clear objects
    public void ClearBoard()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");//gather a list of enemies
        GameObject[] boxes = GameObject.FindGameObjectsWithTag("Room");//gather a list of rooms
        GameObject[] pus = GameObject.FindGameObjectsWithTag("Powerup");//gather a list of powerups

        //cycle through enemies, and deestroy them
        foreach (GameObject current in enemies)
        {
            GameObject.Destroy(current);
        }

        //cycle through rooms and destroy them
        foreach (GameObject current in boxes)
        {
            GameObject.Destroy(current);
        }

        //cycle through power ups and destroy them
        foreach (GameObject current in pus)
        {
            GameObject.Destroy(current);
        }

    }

    
}
