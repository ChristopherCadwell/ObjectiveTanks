using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;



public class ButtonController : MonoBehaviour
{
    //assign all public items in inspector for reference in script
    [Header("All must be assigned")]
    public Canvas startScreen;
    public Canvas titleScreen;
    public Canvas optionsScreen;
    public Canvas gameOverScreen;
    public Canvas highScreen;
    public Canvas hudP1;
    public Canvas hudP2;
    public Canvas pauseScreen;
    public Image background;
    public Camera cam;
    public Text textOverScore;
    public Text textOverScoreP2;
    public Text textOverHigh;
    public AudioClip buttonDown;
    public AudioClip buttonUp;
    public int inputSeed;
    public GameObject musicSlider;
    public GameObject pauseSlider;
    public InputField seedInputField;
    public Toggle toggleRandom;
    public Toggle toggleSeed;
    public Toggle toggleMOD;
    public InputField pauseSeed;

    public void StartScreenPass()
    {
        startScreen.enabled = false;//hide start
        titleScreen.enabled = true;//show title
        optionsScreen.enabled = false;
        gameOverScreen.enabled = false;
        highScreen.enabled = false;
        hudP1.enabled = false;
        hudP2.enabled = false;
        pauseScreen.enabled = false;
        //start menu ambient
        AudioController.instance.menuSourceMusic.Play();
        musicSlider.GetComponent<Slider>().value = GameManager.instance.musicValue;//set slider to loaded value from prefs
    }

    public void StartOnePlayer()
    {
        titleScreen.enabled = false;//hide title
        background.enabled = false;//hide bkg image
        cam.enabled = false;//stop cam from rendering
        hudP1.enabled = true;//enable p1 hud
        GameManager.instance.StartGame();//call startgame from game manager
        AudioController.instance.menuSourceMusic.Stop();//stop menu music
        AudioController.instance.gameSourceMusic.Play();//play game music

    }

    public void StartTwoPlayer()
    {
        titleScreen.enabled = false;//hide title
        background.enabled = false;//hide image
        cam.enabled = false;//stop cam
        hudP1.enabled = true;//show p1 hud
        hudP2.enabled = true;//show p2 hud
        GameManager.instance.StartGame2p();//call gm start 2player
        AudioController.instance.menuSourceMusic.Stop();//stop menu music
        AudioController.instance.gameSourceMusic.Play();//play game music
    }
    public void Options()
    {

        titleScreen.enabled = false;//hide title
        optionsScreen.enabled = true;//show options
        seedInputField.text = GameManager.instance.mapSeed.ToString();
    }
    public void HighScores()
    {
        //TODO: make a scoreboard
    }
    public void GameOver()
    {
        AudioController.instance.gameSourceMusic.Stop();//stop game music
        cam.enabled = true;//start ui cam
        gameOverScreen.enabled = true;//show game over screen
        hudP1.enabled = false;//hide p1 hud
        hudP2.enabled = false;//hide p2 hud
        GameManager.instance.gameRunning = false;//tell gm game is no longer running
        textOverScore.text = "Score: " + GameManager.instance.score;//get score
        textOverHigh.text = "High Score: " + GameManager.instance.highScore;//get highscore

        //if 1 player
        if (!GameManager.instance.players2)
        {
            textOverScoreP2.enabled = false;//hide p2 score
        }
        //if 2 players
        else if (GameManager.instance.players2)
        {
            textOverScoreP2.enabled = true;//show p2 score
            textOverScoreP2.text = "Score: " + GameManager.instance.p2Score;//get p2 score
        }

        GameManager.instance.ClearBoard();//destroy objects
        GameManager.instance.Save();//save playerprefs

    }
    public void ToTitle()
    {
        gameOverScreen.enabled = false;//hide game over
        background.enabled = true;//show image
        titleScreen.enabled = true;//show title
        AudioController.instance.menuSourceMusic.Play();//play menu music
    }
    public void Back()
    {
        titleScreen.enabled = true;//show title
        optionsScreen.enabled = false;//hide options
    }

    public void RandomMap()
    {

        GameManager.instance.randomMap = true;//set random map to true
        GameManager.instance.mapOfTheDay = false;//random map, not mod

    }
    public void MapOfTheDay()
    {

        GameManager.instance.randomMap = false;//mod, not random map
        GameManager.instance.mapOfTheDay = true;//mod true


    }
    public void SeedMap()
    {

        GameManager.instance.randomMap = false;//seed map, no random
        GameManager.instance.mapOfTheDay = false;//no mod


    }

    public void SeedInput(int recieved)
    {
        inputSeed = int.Parse(seedInputField.text);//convert text to int
        GameManager.instance.mapSeed = inputSeed;//get seed from input
    }

    public void SlideInput(float sRecieved)
    {

        GameManager.instance.musicValue = musicSlider.GetComponent<Slider>().value;//get slider value, set it to music volume
        Debug.Log("Music volume " + GameManager.instance.musicValue);

    }
    public void PauseInput(float sRecieved)
    {
        GameManager.instance.musicValue = pauseSlider.GetComponent<Slider>().value;//get slider value, set it to music volume
        Debug.Log("Music volume " + GameManager.instance.musicValue);

    }
    public void VolUp()
    {
        GameManager.instance.sfxValue = GameManager.instance.sfxValue + 0.1f;//add .1 per + click
        GameManager.instance.sfxValue = Mathf.Clamp(GameManager.instance.sfxValue, 0.0f, 1.0f);//set min max
        Debug.Log("SFX volume " + GameManager.instance.sfxValue);
    }

    public void VolDown()
    {
        GameManager.instance.sfxValue = GameManager.instance.sfxValue - 0.1f;//subtract .1 per click
        GameManager.instance.sfxValue = Mathf.Clamp(GameManager.instance.sfxValue, 0.0f, 1.0f);//set min max
        Debug.Log("SFX volume " + GameManager.instance.sfxValue);
    }
    public void QuitGame()
    {
        //save and quit the game
        GameManager.instance.Save();
        Application.Quit();
    }
    public void PauseGame()
    {
        Time.timeScale = 0;//stop time
        pauseScreen.enabled = true;//turn on pause screen
        pauseSeed.text = GameManager.instance.mapSeed.ToString();

        //determin wich map generation mode, and set toggles on pause screen to match
        if (GameManager.instance.randomMap == true)
        {
            toggleRandom.isOn = true;
            toggleMOD.isOn = false;
            toggleSeed.isOn = false;
        }
        if (GameManager.instance.mapOfTheDay == true)
        {
            toggleRandom.isOn = false;
            toggleMOD.isOn = true;
            toggleSeed.isOn = false;
        }
        if (GameManager.instance.randomMap == false && GameManager.instance.mapOfTheDay == false)
        {
            toggleRandom.isOn = false;
            toggleMOD.isOn = false;
            toggleSeed.isOn = true;
        }
    }
    public void ResumeGame()
    {
        pauseScreen.enabled = false;//turn off pause screen
        GameManager.instance.isPaused = false;//tell gm to unpause
        Time.timeScale = 1;//start time
    }

}
