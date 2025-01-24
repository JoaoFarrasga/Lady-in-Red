using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class FullScreenSettings : MonoBehaviour
{
    public Toggle toggle;
    
    void Start()
    {
        if (Screen.fullScreen)
        {
            toggle.isOn = true;
        }
        else
        {
            toggle.isOn = false;
        }
    }


    void Update()
    {
       
    }
    public void AtivarModoJanela(bool modoJanela)
    {   
        Screen.fullScreen = modoJanela;
    }
}
