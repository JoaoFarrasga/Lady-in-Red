using UnityEngine;

public class OptionsMenuController : MonoBehaviour
{
    public GameObject optionsMenu; // Arraste o painel de opções aqui no Inspector

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // Verifica se a tecla ESC foi pressionada
        {
            ToggleMenu(); // Chama a função para abrir/fechar o menu
        }
    }

    public void ToggleMenu()
    {
        if (optionsMenu != null)
        {
            bool isActive = optionsMenu.activeSelf; // Verifica se o menu está ativo
            optionsMenu.SetActive(!isActive); // Alterna entre ativo/inativo
        }
    }
}
