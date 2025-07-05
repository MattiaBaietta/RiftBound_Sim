using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "NewDeck", menuName = "Riftbound/Deck")]
public class Deck : ScriptableObject
{
    [Header("Essential Cards")]
    public Card championLegend;
    public Card chosenChampion;

    [Header("Card Lists")]
    public List<Card> mainDeck;
    public List<Card> runeDeck;
    public List<Card> battlefields;

    public Deck()
    {
        mainDeck = new List<Card>();
        runeDeck = new List<Card>();
        battlefields = new List<Card>();
    }

    /// <summary>
    /// Checks if the deck is valid according to all deckbuilding rules.
    /// </summary>
    /// <returns>True if the deck is valid, otherwise false.</returns>
    public bool IsValid()
    {
        // --- Preliminary Checks ---
        if (championLegend == null || chosenChampion == null)
        {
            Debug.LogError("VALIDATION FAILED: Champion Legend or Chosen Champion is not set.");
            return false;
        }

        // --- Main Deck Rules ---

        // Rule 103.2: At least 40 cards
        if (mainDeck.Count < 40)
        {
            Debug.LogError($"VALIDATION FAILED: Main deck has {mainDeck.Count} cards, 40 required.");
            return false;
        }

        // Rule 103.2.b: Check for maximum copies of each card
        var cardCounts = mainDeck.GroupBy(c => c.cardName);
        foreach (var group in cardCounts)
        {
            int maxCopies = group.First().maximumCopies;
            if (group.Count() > maxCopies)
            {
                Debug.LogError($"VALIDATION FAILED: Deck contains {group.Count()} copies of '{group.Key}', but the maximum is {maxCopies}.");
                return false;
            }
        }

        // --- Special Card Rules ---

        // Rule 103.2.d: Total Signature cards check
        var signatureCards = mainDeck.Where(c => c.isSignature).ToList();
        if (signatureCards.Count > 3)
        {
            Debug.LogError($"VALIDATION FAILED: Deck contains {signatureCards.Count} Signature cards, but the limit is 3.");
            return false;
        }

        // --- Rune Deck Rules ---

        // Rule 103.3.a: Exactly 12 Rune cards
        if (runeDeck.Count != 12)
        {
            Debug.LogError($"VALIDATION FAILED: Rune deck has {runeDeck.Count} cards, but exactly 12 are required.");
            return false;
        }
        if (runeDeck.Any(card => card.type != "RUNE"))
        {
            Debug.LogError($"VALIDATION FAILED: A non-Rune card was found in the Rune Deck.");
            return false;
        }

        // --- DOMAIN IDENTITY RULE (CRUCIAL) ---

        // Rule 103.1.b.2: All cards must adhere to the Legend's Domain Identity.
        var legendDomains = new HashSet<string>(championLegend.domains);
        if (!legendDomains.Any())
        {
            Debug.LogError("VALIDATION FAILED: The Champion Legend has no domains assigned, cannot check Domain Identity.");
            return false;
        }

        foreach (var cardInDeck in mainDeck)
        {
            // A card with no domains can be in any deck.
            if (cardInDeck.domains == null || !cardInDeck.domains.Any())
            {
                continue;
            }

            // If a card has domains, all of its domains must be a subset of the legend's domains.
            var cardDomains = new HashSet<string>(cardInDeck.domains);
            if (!cardDomains.IsSubsetOf(legendDomains))
            {
                Debug.LogError($"VALIDATION FAILED: Card '{cardInDeck.cardName}' with domains [{string.Join(", ", cardInDeck.domains)}] is not allowed in a deck with Legend domains [{string.Join(", ", championLegend.domains)}].");
                return false;
            }
        }

        Debug.Log("VALIDATION PASSED: This deck is valid!");
        return true;
    }
}