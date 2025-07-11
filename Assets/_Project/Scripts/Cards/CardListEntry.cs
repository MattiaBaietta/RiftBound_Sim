using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Necessario per usare 'Action'

public class CardListEntry : MonoBehaviour
{
    // Riferimenti ai componenti UI (da collegare nell'Inspector del Prefab)
    [Header("UI References")]
    public Image cardImage;
    public TextMeshProUGUI countText;

    // Variabili private per la logica interna
    private Card cardData;
    private Action<Card> onClickAction; // L'azione da eseguire al click (es. AddCardToDeck)

    /// <summary>
    /// Configura questa voce della lista con i dati di una carta e un'azione da compiere al click.
    /// </summary>
    public void Setup(Card data, Action<Card> clickAction)
    {
        this.cardData = data;
        this.onClickAction = clickAction;

        if (cardData != null && cardImage != null)
        {
            // Mostra la thumbnail della carta. Usa cardArt se preferisci l'immagine grande.
            cardImage.sprite = cardData.cardArtThumbnail;
        }

        // Nasconde il contatore di default
        if (countText != null)
        {
            countText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Aggiorna il testo del contatore (usato nella lista del mazzo corrente).
    /// </summary>
    public void UpdateCount(int count)
    {
        if (countText != null)
        {
            // Mostra il contatore solo se è maggiore di 1
            countText.gameObject.SetActive(count > 1);
            countText.text = $"x{count}";
        }
    }

    /// <summary>
    /// Questo metodo viene chiamato dal componente Button del prefab quando viene cliccato.
    /// </summary>
    public void OnClick()
    {
        // Quando l'utente clicca, esegue l'azione che gli è stata assegnata,
        // passando i dati di questa specifica carta.
        onClickAction?.Invoke(cardData);
    }
}