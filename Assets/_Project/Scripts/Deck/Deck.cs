using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Necessario per funzioni come Count(), GroupBy(), Any()

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
        // Inizializza le liste per evitare errori
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
        // --- Controlli Preliminari ---
        if (championLegend == null || chosenChampion == null)
        {
            Debug.LogError("VALIDATION FAILED: Champion Legend or Chosen Champion is not set.");
            return false;
        }

        // --- Regole del Mazzo Principale ---

        // Regola 103.2: Almeno 40 carte
        if (mainDeck.Count < 40)
        {
            Debug.LogError($"VALIDATION FAILED: Main deck has {mainDeck.Count} cards, but at least 40 are required.");
            return false;
        }

        // Regola 103.2.b: Controlla le copie massime per ogni carta usando la sua proprietà
        var cardCounts = mainDeck.GroupBy(c => c.cardName);
        foreach (var group in cardCounts)
        {
            int maxCopies = group.First().maximumCopies; // Prende il limite dalla prima carta del gruppo
            if (group.Count() > maxCopies)
            {
                Debug.LogError($"VALIDATION FAILED: Deck contains {group.Count()} copies of '{group.Key}', but the maximum allowed is {maxCopies}.");
                return false;
            }
        }

        // --- Regole per Carte Speciali ---

        // Regola 103.2.d: Massimo 3 carte "Signature" in totale
        int signatureCardCount = mainDeck.Count(c => c.isSignature);
        if (signatureCardCount > 3)
        {
            Debug.LogError($"VALIDATION FAILED: Deck contains {signatureCardCount} Signature cards, but the limit is 3.");
            return false;
        }

        // Regola 103.2.d.2: Le carte Signature devono avere il tag del Campione Scelto
        var championTags = new HashSet<string>(chosenChampion.tags);
        if (signatureCardCount > 0 && !championTags.Any())
        {
            Debug.LogWarning($"VALIDATION NOTE: Chosen Champion '{chosenChampion.cardName}' has no tags to validate Signature cards against.");
        }
        else
        {
            foreach (var signatureCard in mainDeck.Where(c => c.isSignature))
            {
                if (!signatureCard.tags.Any(tag => championTags.Contains(tag)))
                {
                    Debug.LogError($"VALIDATION FAILED: Signature card '{signatureCard.cardName}' does not share a tag with the Chosen Champion '{chosenChampion.cardName}'.");
                    return false;
                }
            }
        }

        // --- Regole del Mazzo delle Rune ---

        // Regola 103.3.a: Esattamente 12 carte Runa
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

        // --- REGOLA FONDAMENTALE: IDENTITÀ DEL DOMINIO ---

        // Regola 103.1.b.2: Tutte le carte devono rispettare l'identità del dominio della Leggenda
        var legendDomains = new HashSet<string>(championLegend.domains);
        if (!legendDomains.Any())
        {
            // Se la leggenda non ha domini (es. una leggenda "incolore"), allora tutto è permesso.
            // Se invece dovesse averli per forza, questo diventerebbe un errore.
            Debug.LogWarning("VALIDATION NOTE: The Champion Legend has no domains, so any card is permitted.");
        }
        else
        {
            foreach (var cardInDeck in mainDeck)
            {
                // Una carta senza domini può essere inserita in qualsiasi mazzo.
                if (cardInDeck.domains == null || !cardInDeck.domains.Any())
                {
                    continue;
                }

                // Se una carta ha dei domini, TUTTI i suoi domini devono essere un sottoinsieme dei domini della leggenda.
                var cardDomains = new HashSet<string>(cardInDeck.domains);
                if (!cardDomains.IsSubsetOf(legendDomains))
                {
                    Debug.LogError($"VALIDATION FAILED: Card '{cardInDeck.cardName}' with domains [{string.Join(", ", cardInDeck.domains)}] is not allowed in a deck with Legend domains [{string.Join(", ", championLegend.domains)}].");
                    return false;
                }
            }
        }

        // Se tutti i controlli sono passati, il mazzo è valido
        Debug.Log("VALIDATION PASSED: This deck is valid!");
        return true;
    }
}