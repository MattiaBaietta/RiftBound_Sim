using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DeckbuilderManager : MonoBehaviour
{
    public static DeckbuilderManager instance;

    [Header("Deck Data")]
    public Deck currentDeck;

    [Header("UI References")]
    public TMP_InputField deckNameInput;
    public Button saveButton;
    public Transform currentDeckContentArea;
    public Transform cardCollectionContentArea;
    public TMP_InputField nameFilterInput;
    public List<Toggle> domainToggles;
    public TextMeshProUGUI deckCountText;

    // Aggiungi questo campo per l'anteprima
    [Header("UI Preview")]
    public Image cardPreviewImage;

    [Header("Prefabs")]
    public GameObject cardListEntryPrefab;

    private List<Card> allCardsInGame;
    private string selectedDomainFilter = "All";

    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        // Nascondi l'anteprima all'inizio
        if (cardPreviewImage != null)
        {
            cardPreviewImage.gameObject.SetActive(false);
        }

        if (currentDeck == null) { Debug.LogError("ERRORE: Nessun 'Current Deck' assegnato!"); return; }

        LoadAllCardAssets();
        deckNameInput.text = currentDeck.name;

        if (saveButton != null) saveButton.onClick.AddListener(SaveDeck);
        if (nameFilterInput != null) nameFilterInput.onValueChanged.AddListener((s) => RefreshCollectionView());

        if (domainToggles != null)
        {
            foreach (var toggle in domainToggles)
            {
                if (toggle != null)
                {
                    toggle.onValueChanged.AddListener((isOn) => { if (isOn) UpdateSelectedDomainFilter(toggle); });
                }
            }
        }

        RefreshCollectionView();
        RefreshCurrentDeckView();
    }

    void LoadAllCardAssets()
    {
        allCardsInGame = new List<Card>(Resources.LoadAll<Card>("ScriptableObjects/Cards"));
    }

    private void UpdateSelectedDomainFilter(Toggle activeToggle)
    {
        if (activeToggle == null) return;
        TextMeshProUGUI toggleLabel = activeToggle.GetComponentInChildren<TextMeshProUGUI>();
        if (toggleLabel != null)
        {
            selectedDomainFilter = toggleLabel.text;
        }
        RefreshCollectionView();
    }

    void RefreshCollectionView()
    {
        foreach (Transform child in cardCollectionContentArea) { Destroy(child.gameObject); }

        string nameFilter = nameFilterInput.text.ToLower();
        List<Card> filteredCards = allCardsInGame.Where(card =>
            (string.IsNullOrEmpty(nameFilter) || card.cardName.ToLower().Contains(nameFilter)) &&
            (selectedDomainFilter == "All" || card.domains.Contains(selectedDomainFilter))
        ).ToList();

        foreach (Card card in filteredCards.OrderBy(c => c.cardName))
        {
            GameObject entryGO = Instantiate(cardListEntryPrefab, cardCollectionContentArea);
            entryGO.GetComponent<CardListEntry>().Setup(card, AddCardToDeck);
        }
    }



    /// <summary>
    /// Aggiorna la vista del mazzo corrente con un ordinamento personalizzato e robusto.
    /// </summary>
    /// <summary>
    /// Aggiorna la vista del mazzo corrente con un ordinamento personalizzato e robusto.
    /// </summary>
    void RefreshCurrentDeckView()
    {
        // Pulisce la vista attuale per evitare duplicati
        foreach (Transform child in currentDeckContentArea) { Destroy(child.gameObject); }

        if (currentDeck == null) return;

        // --- NUOVA LOGICA DI ORDINAMENTO ROBUSTA ---

        var sortedDeck = currentDeck.mainDeck
            .OrderBy(card => {
                // Assegna una priorità basandosi sul tipo della carta.
                // Numeri più bassi vengono prima.
                if (card.type == "LEGEND") return 0;   // Priorità massima per le Leggende
                if (card.isChampion) return 1;          // Seconda priorità per i Campioni
                return 2;                               // Priorità normale per tutte le altre
            })
            .ThenBy(card => card.cardName) // Ordina alfabeticamente le carte con la stessa priorità
            .ToList();

        // Raggruppa le carte per mostrare il contatore
        var groupedDeck = sortedDeck.GroupBy(c => c);

        // Popola la vista con la lista ordinata e raggruppata
        foreach (var group in groupedDeck)
        {
            Card card = group.Key;
            int count = group.Count();

            GameObject entryGO = Instantiate(cardListEntryPrefab, currentDeckContentArea);
            CardListEntry entry = entryGO.GetComponent<CardListEntry>();

            entry.Setup(card, RemoveCardFromDeck);
            entry.UpdateCount(count);
        }

        UpdateDeckCountText();
    }

    void UpdateDeckCountText()
    {
        if (deckCountText != null)
            deckCountText.text = $"Carte: {currentDeck.mainDeck.Count}/40";
    }

    public void AddCardToDeck(Card cardToAdd)
    {
        if (currentDeck == null || cardToAdd == null) return;

        // --- NUOVO CONTROLLO PER LA LEGGENDA ---
        // Controlla se la carta che stai aggiungendo è una Leggenda
        if (cardToAdd.type == "LEGEND")
        {
            // Controlla se nel mazzo esiste già una carta di tipo Leggenda
            if (currentDeck.mainDeck.Any(card => card.type == "LEGEND"))
            {
                Debug.LogWarning("Non puoi aggiungere una seconda Leggenda al mazzo!");
                return; // Interrompe la funzione e non aggiunge la carta
            }
        }

        // Controlla il limite di copie (la logica di prima rimane)
        if (currentDeck.mainDeck.Count(c => c.cardID == cardToAdd.cardID) < cardToAdd.maximumCopies)
        {
            currentDeck.mainDeck.Add(cardToAdd);
            RefreshCurrentDeckView();
        }
        else
        {
            Debug.LogWarning($"Limite massimo di {cardToAdd.maximumCopies} copie raggiunto per {cardToAdd.cardName}.");
        }
    }

    public void RemoveCardFromDeck(Card cardToRemove)
    {
        currentDeck.mainDeck.Remove(cardToRemove);
        RefreshCurrentDeckView();
    }

    public void SaveDeck()
    {
        if (currentDeck.IsValid())
        {
            currentDeck.name = deckNameInput.text;
#if UNITY_EDITOR
            EditorUtility.SetDirty(currentDeck);
            AssetDatabase.SaveAssets();
#endif
            Debug.Log($"Mazzo '{currentDeck.name}' salvato!");
        }
    }


    /// <summary>
    /// Mostra l'anteprima della carta specificata.
    /// </summary>
    public void ShowCardPreview(Card cardToShow)
    {
        if (cardPreviewImage != null && cardToShow != null)
        {
            // Usa l'immagine grande per l'anteprima
            cardPreviewImage.sprite = cardToShow.cardArt;
            cardPreviewImage.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Nasconde l'area di anteprima.
    /// </summary>
    public void HideCardPreview()
    {
        if (cardPreviewImage != null)
        {
            cardPreviewImage.gameObject.SetActive(false);
        }
    }
}