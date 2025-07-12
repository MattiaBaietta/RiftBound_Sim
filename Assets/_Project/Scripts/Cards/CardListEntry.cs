using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems; // Necessario per gli eventi del mouse

// Aggiungiamo le interfacce per rilevare l'entrata e l'uscita del mouse
public class CardListEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Riferimenti ai componenti UI (da collegare nel prefab)
    public Image cardImage;
    public TextMeshProUGUI countText;

    private Card cardData;
    private Action<Card> onClickAction;

    public void Setup(Card data, Action<Card> clickAction)
    {
        this.cardData = data;
        this.onClickAction = clickAction;

        if (cardData != null && cardImage != null)
        {
            cardImage.sprite = cardData.cardArtThumbnail;
        }

        if (countText != null)
        {
            countText.gameObject.SetActive(false);
        }
    }

    public void UpdateCount(int count)
    {
        if (countText != null)
        {
            countText.gameObject.SetActive(count > 1);
            countText.text = $"x{count}";
        }
    }

    public void OnClick()
    {
        onClickAction?.Invoke(cardData);
    }

    // --- METODI PER L'ANTEPRIMA ---

    /// <summary>
    /// Chiamato da Unity quando il mouse entra nell'area di questo oggetto.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (cardData != null && DeckbuilderManager.instance != null)
        {
            // Dice al manager di mostrare l'anteprima di questa carta
            DeckbuilderManager.instance.ShowCardPreview(cardData);
        }
    }

    /// <summary>
    /// Chiamato da Unity quando il mouse esce dall'area.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (DeckbuilderManager.instance != null)
        {
            // Dice al manager di nascondere l'anteprima
            DeckbuilderManager.instance.HideCardPreview();
        }
    }
}