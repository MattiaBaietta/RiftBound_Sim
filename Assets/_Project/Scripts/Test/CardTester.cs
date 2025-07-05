using UnityEngine;

public class CardTester : MonoBehaviour
{
    [Header("Oggetti da Testare")]
    [Tooltip("Trascina qui l'asset ScriptableObject della carta che vuoi testare.")]
    public Card cardToTest;

    [Tooltip("Trascina qui l'oggetto CardPrefab_Visual dalla scena.")]
    public CardView cardView;

    // Start viene chiamato prima del primo frame
    void Start()
    {
        // Controlla se abbiamo collegato tutto nell'Inspector
        if (cardToTest != null && cardView != null)
        {
            Debug.Log($"Sto testando la carta: {cardToTest.cardName}");
            // Passa i dati dello ScriptableObject allo script di visualizzazione
            cardView.Setup(cardToTest);
        }
        else
        {
            Debug.LogError("Assicurati di aver assegnato 'Card To Test' e 'Card View' nell'Inspector!");
        }
    }
}