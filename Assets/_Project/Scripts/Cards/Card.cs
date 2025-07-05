using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Card", menuName = "Riftbound/Card Definition")]
public class Card : ScriptableObject
{
    [Header("Card Identity")]
    public string cardID;
    public string cardName;
    public string type;
    public string category;

    [Tooltip("L'immagine principale della carta.")]
    public Sprite cardArt;

    [Tooltip("L'immagine thumbnail/icona della carta.")]
    public Sprite cardArtThumbnail;

    [Header("Stats & Costs")]
    public int energyCost;
    public int might;
    public int maximumCopies;

    [Header("Gameplay Properties")]
    [TextArea(5, 10)]
    public string rulesText;
    public List<string> domains;
    public List<string> keywords;
    public List<string> tags;
    public bool isChampion;
    public bool isSignature;
}