using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenu : MonoBehaviour
{
    // New game method
    public void OnNewGameButton()
    {
        Debug.Log("Avvio di una nuova partita...");
        //TODO: Aggiungere log firebase ed implementarlo
        SceneManager.LoadScene("GameplayScene");
    }

    public void OnDeckbuilderButton()
    {
        Debug.Log("Apertura del Deckbuilder...");
        SceneManager.LoadScene("DeckbuilderScene");
    }

    // Close game method
    public void OnQuitButton()
    {
        Debug.Log("Uscita dal gioco...");
        Application.Quit();
    }
}
