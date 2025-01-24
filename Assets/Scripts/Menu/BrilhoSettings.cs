using UnityEngine;
using UnityEngine.UI;

public class BrilhoSettings : MonoBehaviour
{
    public Slider slider;
    public float sliderValue;
    public Image panelBrilho;
    void Start()
    {
        slider.value = PlayerPrefs.GetFloat("Brilho", 0.5f);
        panelBrilho.color = new Color(panelBrilho.color.r, panelBrilho.color.g, panelBrilho.color.b, slider.value);   
    }
    public void ChangeSlider(float valor)
    {
        slider.value = valor;
        PlayerPrefs.SetFloat("Brilho",sliderValue);
        panelBrilho.color =  new Color(panelBrilho.color.r, panelBrilho.color.g, panelBrilho.color.b,sliderValue);
    }
   
}
