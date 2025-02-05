using UnityEngine;

public class MoveBackground : MonoBehaviour
{
    public float speed = 2f; // Velocidade do movimento
    public float limitLeft = 300f; // Posi��o m�nima
    public float limitRight = 500f; // Posi��o m�xima

    private RectTransform rectTransform;
    private int direction = 1; // 1 para direita, -1 para esquerda

    void Start()
    {
        // Pegamos o RectTransform para mover corretamente na UI
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Atualiza a posi��o baseada na dire��o
        rectTransform.anchoredPosition += new Vector2(speed * direction * Time.deltaTime, 0);

        // Verifica os limites e inverte dire��o
        if (rectTransform.anchoredPosition.x >= limitRight)
        {
            direction = -1; // Vai para esquerda
        }
        else if (rectTransform.anchoredPosition.x <= limitLeft)
        {
            direction = 1; // Vai para direita
        }
    }
}

