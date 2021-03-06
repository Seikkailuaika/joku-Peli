﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HeadPhoneFade : MonoBehaviour
{
    RawImage HeadPhoneImage;


    // Use this for initialization

    private IEnumerator WakingUpScreen()
    {
        //max alpha value
        float alpha = 255;

        //decrease alpha by a little rapidly so it looks like a smooth fade in
        for (int i = 0; i < 100; i++)
        {
            //if this line gives you errors make sure to enable SleepOverlay before you start the game!!
            HeadPhoneImage.color = new Color32(0, 0, 0, (byte)alpha);
            alpha -= 2.55f;
            yield return new WaitForSeconds(0.02f);

        }

        yield return new WaitForSeconds(5f);

        if(Application.platform == RuntimePlatform.Android)
        {
            SceneManager.LoadSceneAsync("Startmenu");
        }
        else
        {
            SceneManager.LoadSceneAsync("ComputerMenu");
        }
        
        
    }
    void Start()
    {
        HeadPhoneImage = GameObject.Find("Overlay").GetComponent<RawImage>();
        StartCoroutine(WakingUpScreen());
        


    }


    // Update is called once per frame
    void Update()
    {

    }
}
