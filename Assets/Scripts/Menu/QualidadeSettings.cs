using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class QualidadeSettings : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public int qualidade;
    void Start()
    {
        qualidade = PlayerPrefs.GetInt("N�meroDeQualidade", 3);
        dropdown.value = qualidade;
        AjustarQualidade();


    }

    
    void Update()
    {
        
    }
    public void AjustarQualidade()
    {
        QualitySettings.SetQualityLevel(dropdown.value);
        PlayerPrefs.SetInt("N�meroDeQualidade", dropdown.value);
        qualidade = dropdown.value;
    }
}
