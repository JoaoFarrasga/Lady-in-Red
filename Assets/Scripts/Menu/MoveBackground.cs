using UnityEngine;

public class MoveBackground : MonoBehaviour
{
    public float speed = 2.0f; // Velocidade do movimento
    public float limitLeft = -5.0f; // Limite esquerdo
    public float limitRight = 5.0f; // Limite direito

    private int direction = 1; // Dire��o inicial (1 para direita, -1 para esquerda)

    void Update()
    {
        // Move o objeto na dire��o atual
        transform.position += Vector3.right * speed * direction * Time.deltaTime;

        // Verifica se atingiu o limite esquerdo ou direito
        if (transform.position.x >= limitRight)
        {
            direction = -1; // Muda a dire��o para a esquerda
        }
        else if (transform.position.x <= limitLeft)
        {
            direction = 1; // Muda a dire��o para a direita
        }
    }
}
