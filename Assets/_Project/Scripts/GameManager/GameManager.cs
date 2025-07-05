using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("Player Areas")]
    public GameObject playerOneArea;
    public GameObject playerTwoArea;

    [Header("Deck Data")]
    // **MODIFICA CHIAVE**: Ora usiamo gli asset Deck, non liste separate
    [Tooltip("Trascina qui l'asset del mazzo per il Giocatore 1")]
    public Deck playerOneDeckData;

    [Tooltip("Trascina qui l'asset del mazzo per il Giocatore 2")]
    public Deck playerTwoDeckData;

    // Variabili interne per gestire le carte durante la partita
    private List<Card> playerOneMainDeck;
    private List<Card> playerTwoMainDeck;

    void Start()
    {
        Debug.Log("GameManager started. Setting up the game...");

        // Controlla se i mazzi sono stati assegnati nell'Inspector
        if (playerOneDeckData == null || playerTwoDeckData == null)
        {
            Debug.LogError("DECK DATA NOT ASSIGNED! Please assign a Deck ScriptableObject to both player deck slots in the GameManager inspector.");
            return;
        }

        // Ora il setup usa i dati dai nostri asset Deck
        SetupGame();
    }

    void SetupGame()
    {
        // 1. Convalida i mazzi prima di iniziare
        if (!playerOneDeckData.IsValid() || !playerTwoDeckData.IsValid())
        {
            Debug.LogError("One of the decks is invalid. Halting game setup. Please check console for validation errors.");
            return;
        }

        // 2. Inizializza i mazzi di gioco creando una COPIA delle liste dall'asset
        //    Questo è importante per non modificare l'asset originale durante la partita
        playerOneMainDeck = new List<Card>(playerOneDeckData.mainDeck);
        playerTwoMainDeck = new List<Card>(playerTwoDeckData.mainDeck);

        // 3. Mischia i mazzi di gioco
        ShuffleDeck(playerOneMainDeck);
        ShuffleDeck(playerTwoMainDeck);
        Debug.Log("Player decks have been initialized and shuffled.");

        // 4. Posiziona le Leggende, i Campioni e i Campi di Battaglia
        //    (Questa logica andrà implementata qui)
        //    Esempio:
        //    playerOneLegendVisual.sprite = playerOneDeckData.championLegend.cardArt;

        // 5. Pesca la mano iniziale
        //    (Anche questa logica andrà qui)
        //    DrawInitialHands();

        Debug.Log("Game setup is complete.");
    }

    /// <summary>
    /// An algorithm to shuffle a list of cards (Fisher-Yates shuffle).
    /// </summary>
    private void ShuffleDeck(List<Card> deck)
    {
        // La logica di mescolamento è corretta e rimane invariata
        for (int i = 0; i < deck.Count; i++)
        {
            Card temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }
}