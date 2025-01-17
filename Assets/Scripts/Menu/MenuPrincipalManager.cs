using UnityEngine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipalManager : MonoBehaviour
{
    [SerializeField] private string JoaoScene;
    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelOpcoes;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Jogar()
    {
        SceneManager.LoadScene(JoaoScene);
    }
    public void AbrirOpcoes()
    {
        painelMenuInicial.SetActive(false);
        painelOpcoes.SetActive(true);  
    }
    public void FecharOpcoes()
    {
        painelOpcoes.SetActive(false);
        painelMenuInicial.SetActive(true);
    }
    public void SairJogo()
    {
        Debug.Log("Sair do Jogo");
        Application.Quit(); 
    }
}
