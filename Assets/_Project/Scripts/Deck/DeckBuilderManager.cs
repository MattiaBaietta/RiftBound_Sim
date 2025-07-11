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
    // A single instance for easy access from other scripts (Singleton)
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

    [Header("Prefabs")]
    public GameObject cardListEntryPrefab;

    private List<Card> allCardsInGame;

    void Awake()
    {
        // Setup for the Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initial safety checks
        if (currentDeck == null) { Debug.LogError("No 'Current Deck' assigned!"); return; }

        LoadAllCardAssets();
        deckNameInput.text = currentDeck.name;

        // Add listeners to UI events
        saveButton.onClick.AddListener(SaveDeck);
        nameFilterInput.onValueChanged.AddListener(delegate { RefreshCollectionView(); });

        foreach (var toggle in domainToggles)
        {
            if (toggle != null)
            {
                // CORRECTED: The listener now calls RefreshCollectionView on ANY change (checked or unchecked).
                toggle.onValueChanged.AddListener(delegate { RefreshCollectionView(); });
            }
        }

        // Populate the UI on start
        RefreshCollectionView();
        RefreshCurrentDeckView();
    }

    void LoadAllCardAssets()
    {
        allCardsInGame = new List<Card>(Resources.LoadAll<Card>("ScriptableObjects/Cards"));
    }

    /// <summary>
    /// Updates the card collection view based on all active filters.
    /// </summary>
    void RefreshCollectionView()
    {
        // First, clear the existing list
        foreach (Transform child in cardCollectionContentArea) { Destroy(child.gameObject); }

        // --- NEW FILTERING LOGIC ---

        // 1. Get the name filter text
        string nameFilter = nameFilterInput.text.ToLower();

        // 2. Build a list of all currently selected domains
        List<string> selectedDomains = new List<string>();
        foreach (var toggle in domainToggles)
        {
            if (toggle != null && toggle.isOn)
            {
                selectedDomains.Add(toggle.GetComponentInChildren<TextMeshProUGUI>().text);
            }
        }

        // 3. Check if we should show all domains
        // This happens if "All" is selected OR if no domains are selected.
        bool filterAllDomains = selectedDomains.Contains("All") || selectedDomains.Count == 0;

        // 4. Filter the master card list
        List<Card> filteredCards = allCardsInGame.Where(card =>
        {
            bool nameMatch = string.IsNullOrEmpty(nameFilter) || card.cardName.ToLower().Contains(nameFilter);

            // The card passes if we are showing all domains, OR if any of the card's domains are in our list of selected domains.
            bool domainMatch = filterAllDomains || card.domains.Any(domain => selectedDomains.Contains(domain));

            return nameMatch && domainMatch;

        }).ToList();

        // 5. Populate the view with the filtered cards
        foreach (Card card in filteredCards.OrderBy(c => c.cardName))
        {
            GameObject entryGO = Instantiate(cardListEntryPrefab, cardCollectionContentArea);
            entryGO.GetComponent<CardListEntry>().Setup(card, AddCardToDeck);
        }

    }

    // --- The rest of the functions remain unchanged ---
    void RefreshCurrentDeckView()
    {
        // Pulisce la vista attuale
        foreach (Transform child in currentDeckContentArea) { Destroy(child.gameObject); }

        // Raggruppa le carte nel mazzo per mostrare il contatore
        var groupedDeck = currentDeck.mainDeck.GroupBy(c => c);

        foreach (var group in groupedDeck.OrderBy(g => g.Key.cardName))
        {
            Card card = group.Key;
            int count = group.Count();

            GameObject entryGO = Instantiate(cardListEntryPrefab, currentDeckContentArea);
            CardListEntry entry = entryGO.GetComponent<CardListEntry>();

            // Dice allo script del prefab: "Quando clicchi su questa carta (che è nel mazzo),
            // esegui la funzione RemoveCardFromDeck."
            entry.Setup(card, RemoveCardFromDeck);
            entry.UpdateCount(count); // Mostra il contatore (es. "x2")
        }

        // Aggiorna il testo del contatore generale
        if (deckCountText != null)
            deckCountText.text = $"Carte: {currentDeck.mainDeck.Count}/40";
    }


    void UpdateDeckCountText()
    {
        if (deckCountText != null)
            deckCountText.text = $"Cards: {currentDeck.mainDeck.Count}/40";
    }

    public void AddCardToDeck(Card cardToAdd)
    {
        if (currentDeck == null) return;

        // Controlla se si può aggiungere un'altra copia della carta
        if (currentDeck.mainDeck.Count(c => c.cardID == cardToAdd.cardID) < cardToAdd.maximumCopies)
        {
            currentDeck.mainDeck.Add(cardToAdd);
            // Dopo aver aggiunto la carta, rinfresca la vista del mazzo
            RefreshCurrentDeckView();
            Debug.Log($"Aggiunta carta: {cardToAdd.cardName}");
        }
        else
        {
            Debug.LogWarning($"Limite massimo di {cardToAdd.maximumCopies} copie raggiunto per {cardToAdd.cardName}.");
        }
    }

    public void RemoveCardFromDeck(Card cardToRemove)
    {
        if (currentDeck == null) return;

        currentDeck.mainDeck.Remove(cardToRemove);
        RefreshCurrentDeckView();
        Debug.Log($"Rimossa carta: {cardToRemove.cardName}");
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
            Debug.Log($"Deck '{currentDeck.name}' saved successfully!");
        }
    }
}