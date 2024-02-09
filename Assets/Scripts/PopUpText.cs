using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PopUpText : MonoBehaviour
{
    public Animator textAnimation;//tell the text what to do

    private Text damageText;
    private Text powerupText;

    private void OnEnable()
    {
        AnimatorClipInfo[] animInfo = textAnimation.GetCurrentAnimatorClipInfo(0);//store animation in an array because....animator
        Destroy(gameObject, animInfo[0].clip.length);//destroy object after animation completets
        damageText = textAnimation.GetComponent<Text>();//get text object from anchor window
        powerupText = textAnimation.GetComponent<Text>();//get text
    }

    public void SetDamageText(string text)
    {
        damageText.text = text;//set displayed value for text
    }

    public void SetPowerupText(string text)
    {
        powerupText.text = text;//set displayed value for text
    }

}
