using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpController : MonoBehaviour
{
    private static PopUpText damageText;
    private static PopUpText powerupText;
    private static GameObject canvas;


    public static void Initialize()
    {
        canvas = GameObject.Find("UICanvas");//find the canvas
        damageText = Resources.Load<PopUpText>("DamageTextAnchor");//get object from resource folder
        powerupText = Resources.Load<PopUpText>("PowerupTextAnchor");//get object from resource folder
    }

    public static void CreateDamageText(string text, Transform location)
    {
        PopUpText instance = Instantiate(damageText);//create a popup with an instance reference
        Vector3 screenPop = Camera.current.WorldToScreenPoint(ShellController.popLocation.transform.position);//get screen location for popup from player 1 camera
        instance.transform.SetParent(canvas.transform, false);//set position to UI Canvas position, not using worldspace
        instance.transform.position = screenPop;//set location
        instance.SetDamageText(text);//assign the text "value"
    }

    public static void CreatePowerupText(string text, Transform location)
    {
        PopUpText instance = Instantiate(powerupText);//create a popup with an instance reference
        Vector3 screenPop = Camera.current.WorldToScreenPoint(new Vector3(Pickup.powerPopLocation.transform.position.x, Pickup.powerPopLocation.transform.position.y + 0.5f, Pickup.powerPopLocation.transform.position.z));//get screen location for popup with small offset to keep it onscreen
        instance.transform.SetParent(canvas.transform, false);//set position to UI Canvas position, not using worldspace
        instance.transform.position = screenPop;//set location
        instance.SetPowerupText(text);//assign the text "value"
    }
}

