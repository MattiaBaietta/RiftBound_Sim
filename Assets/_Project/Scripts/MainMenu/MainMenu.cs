using UnityEngine;
using UnityEngine.SceneManagement; // Fondamentale per cambiare scena!

public class MainMenu : MonoBehaviour
{
    // Questa funzione verrà collegata al pulsante "Nuova Partita"
    public void OnNewGameButton()
    {
        Debug.Log("Avvio di una nuova partita...");
        // Carica la scena dove si svolge il gioco.
        // Assicurati di avere una scena con questo nome!
        SceneManager.LoadScene("GameplayScene");
    }

    // Questa funzione verrà collegata al pulsante "Deckbuilder"
    public void OnDeckbuilderButton()
    {
        Debug.Log("Apertura del Deckbuilder...");
        // Carica la scena per la costruzione dei mazzi.
        // Assicurati di avere una scena con questo nome!
        SceneManager.LoadScene("DeckbuilderScene");
    }

    // Funzione bonus per chiudere il gioco
    public void OnQuitButton()
    {
        Debug.Log("Uscita dal gioco...");
        Application.Quit();
    }
}