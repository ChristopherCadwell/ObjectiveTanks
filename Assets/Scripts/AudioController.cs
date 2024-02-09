using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController instance;
    public AudioSource menuSourceMusic;
    public AudioSource gameSourceMusic;
    public AudioSource menuSourceFX;
    public AudioClip click;
    public AudioClip click2;
    public AudioClip cannonFire;
    public AudioClip tankDeath;
    public AudioClip shellExplode;
    public AudioClip gameMusic;
    public AudioClip menuMusic;
    public AudioClip powerClip;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

    }


    // Update is called once per frame
    void Update()
    {
        menuSourceMusic.volume = GameManager.instance.musicValue;//set volume level
        gameSourceMusic.volume = GameManager.instance.musicValue;//set volume level
        menuSourceFX.volume = GameManager.instance.sfxValue;//set volume level
    }
}
