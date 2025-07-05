using UnityEngine;

public class CardDataQueryResult
{
    public string card_id { get; set; }
    public string name { get; set; }
    public string type_name { get; set; }
    public string category_name { get; set; }
    public int cost_energy { get; set; }
    public int? might { get; set; }
    public bool is_champion { get; set; }
    public bool is_signature { get; set; }
    public int maximum_copies { get; set; }
    public string domains { get; set; }
    public string keywords { get; set; }
    public string tags { get; set; }
}
