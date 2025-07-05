using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Necessario per gestire i click sull'UI

public class CardView : MonoBehaviour, IPointerClickHandler
{
    // Questo è il collegamento ai dati della carta (il nostro ScriptableObject)
    private Card cardData;

    // Riferimento al componente Image di questo stesso oggetto
    private Image cardImage;

    void Awake()
    {
        // All'avvio, prendiamo il riferimento al componente Image
        cardImage = GetComponent<Image>();
    }

    // Un metodo per "caricare" i dati di una carta specifica in questa visualizzazione
    public void Setup(Card data)
    {
        cardData = data;

        // La logica di visualizzazione ora è una sola riga!
        cardImage.sprite = cardData.cardArt;
    }

    // Questo metodo viene chiamato automaticamente quando l'utente clicca sulla carta
    public void OnPointerClick(PointerEventData eventData)
    {
        if (cardData == null) return;

        // Per ora, stampiamo solo un messaggio per provare che funziona.
        // In futuro, qui potresti mettere la logica per "giocare" la carta.
        Debug.Log("Hai cliccato sulla carta: " + cardData.cardName);
    }
}