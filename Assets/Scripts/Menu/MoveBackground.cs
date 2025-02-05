using UnityEngine;

public class MoveBackground : MonoBehaviour
{
    public float speed = 2f; // Velocidade do movimento
    public float limitLeft = 300f; // Posição mínima
    public float limitRight = 500f; // Posição máxima

    private RectTransform rectTransform;
    private int direction = 1; // 1 para direita, -1 para esquerda

    void Start()
    {
        // Pegamos o RectTransform para mover corretamente na UI
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Atualiza a posição baseada na direção
        rectTransform.anchoredPosition += new Vector2(speed * direction * Time.deltaTime, 0);

        // Verifica os limites e inverte direção
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

