using UnityEngine;
using UnityEditor;
using SQLite4Unity3d; // O il namespace corretto del tuo pacchetto
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class CardImporter
{
    // --- PERCORSI DA CONFIGURARE ---
    private static string dbPath = Path.Combine(Application.streamingAssetsPath, "riftbound_final.db");
    private static string cardImagesFolderPath = "Assets/Art/CardImages";
    private static string outputPath = "Assets/_Project/ScriptableObjects/Cards";

    /// <summary>
    /// Funzione helper per caricare uno Sprite in modo robusto, gestendo i sotto-asset di Unity.
    /// </summary>
    private static Sprite LoadSpriteAtPath(string path)
    {
        // Questo metodo è più affidabile di LoadAssetAtPath<Sprite> perché cerca lo Sprite
        // all'interno di tutti gli asset presenti nel file (l'immagine è una Texture2D, lo Sprite è un suo figlio).
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
    }

    [MenuItem("Riftbound/Importa/Importa Carte dal Database")]
    public static void ImportCardsFromDatabase()
    {
        // Forza un refresh dell'AssetDatabase per assicurarsi che tutte le impostazioni siano state lette
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        if (!File.Exists(dbPath))
        {
            Debug.LogError($"DATABASE NON TROVATO! Assicurati che il file '{Path.GetFileName(dbPath)}' esista nel percorso: {dbPath}");
            return;
        }
        Debug.Log("Database trovato. Inizio importazione...");

        // Pulisce la cartella di output per un'importazione pulita
        if (Directory.Exists(outputPath))
        {
            Directory.Delete(outputPath, true);
        }
        Directory.CreateDirectory(outputPath);

        var db = new SQLiteConnection(dbPath);

        try
        {
            // 1. Query principale semplice per leggere i dati base di tutte le carte
            string mainQuery = @"
                SELECT 
                    C.*, 
                    T.name as type_name, 
                    CAT.name as category_name
                FROM Cards C
                LEFT JOIN Types T ON C.type_id = T.type_id
                LEFT JOIN Categories CAT ON C.category_id = CAT.category_id;
            ";
            List<CardBaseData> allCardsData = db.Query<CardBaseData>(mainQuery);
            Debug.Log($"Trovate {allCardsData.Count} carte da importare nel database.");

            int count = 0;
            // 2. Ciclo su ogni carta trovata
            foreach (var cardData in allCardsData)
            {
                Card newCard = ScriptableObject.CreateInstance<Card>();

                // Popola i campi semplici leggendo dalla classe di supporto
                newCard.cardID = cardData.card_id;
                newCard.cardName = cardData.name;
                newCard.type = cardData.type_name;
                newCard.category = cardData.category_name;
                newCard.energyCost = cardData.cost_energy;
                newCard.might = cardData.might ?? 0; // Se might è NULL nel DB, usa 0
                newCard.isChampion = cardData.is_champion;
                newCard.isSignature = cardData.is_signature;
                newCard.maximumCopies = cardData.maximum_copies;
                newCard.rulesText = cardData.rules_text;

                // 3. Esegui query separate e mirate per i dati collegati (molti-a-molti)
                newCard.domains = db.Query<StringQueryResult>("SELECT D.name FROM CardDomains CD JOIN Domains D ON CD.domain_id = D.domain_id WHERE CD.card_id = ?", cardData.card_id).Select(r => r.name).ToList();
                newCard.keywords = db.Query<StringQueryResult>("SELECT K.name FROM CardKeywords CK JOIN Keywords K ON CK.keyword_id = K.keyword_id WHERE CK.card_id = ?", cardData.card_id).Select(r => r.name).ToList();
                newCard.tags = db.Query<StringQueryResult>("SELECT T.name FROM CardTags CT JOIN Tags T ON CT.tag_id = T.tag_id WHERE CT.card_id = ?", cardData.card_id).Select(r => r.name).ToList();

                // 4. Salva l'asset ScriptableObject nel progetto
                string assetPath = Path.Combine(outputPath, $"{newCard.cardID}.asset");
                AssetDatabase.CreateAsset(newCard, assetPath);

                // 5. Cerca e collega gli Sprite
                // Assicurati che l'estensione qui (.jpg, .png, .webp) sia quella dei tuoi file!
                string fullImagePath = Path.Combine(cardImagesFolderPath, $"{newCard.cardID}.jpg");
                string thumbImagePath = Path.Combine(cardImagesFolderPath, $"{newCard.cardID}_small.jpg");

                newCard.cardArt = LoadSpriteAtPath(fullImagePath);
                newCard.cardArtThumbnail = LoadSpriteAtPath(thumbImagePath);

                // Aggiungo un log di avviso se non trova un'immagine
                if (newCard.cardArt == null)
                    Debug.LogWarning($"Immagine grande non trovata per {newCard.cardID}. Percorso cercato: {fullImagePath}");
                if (newCard.cardArtThumbnail == null)
                    Debug.LogWarning($"Thumbnail non trovata per {newCard.cardID}. Percorso cercato: {thumbImagePath}");

                // Marca l'asset come "modificato" per salvare i riferimenti agli Sprite
                EditorUtility.SetDirty(newCard);
                count++;
            }

            Debug.Log($"Importazione completata! Creati e collegati {count} asset di carte.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ERRORE DURANTE L'IMPORTAZIONE: {e}");
        }
        finally
        {
            db.Close();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

// --- CLASSI DI SUPPORTO PER MAPPARE I RISULTATI DELLE QUERY ---

/// <summary>
/// Mappa i risultati della query principale sulla tabella Cards.
/// </summary>
public class CardBaseData
{
    public string card_id { get; set; }
    public string name { get; set; }
    public string type_name { get; set; }
    public string category_name { get; set; }
    public int cost_energy { get; set; }
    public int? might { get; set; } // int? permette di ricevere valori NULL dal DB per le non-unità
    public bool is_champion { get; set; }
    public bool is_signature { get; set; }
    public int maximum_copies { get; set; }
    public string rules_text { get; set; }
}

/// <summary>
/// Classe generica per le query che restituiscono una singola colonna di testo chiamata 'name'.
/// </summary>
public class StringQueryResult
{
    public string name { get; set; }
}