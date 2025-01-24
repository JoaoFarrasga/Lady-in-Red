using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public void setHealth(int Health)
    {
        slider.value = Health;
    }
}
