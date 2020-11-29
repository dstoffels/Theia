using UnityEngine;

public class NpcTrading : NpcOffer
{
    [Header("Items for Sale")]
    public ScriptableItem[] saleItems;

    [Header("Repair")]
    public bool offersRepair = true;
    public int repairCostPerDurabilityPoint = 1;

    public override bool HasOffer(Player player) => saleItems.Length > 0;

    public override string GetOfferName() => "Buy/Sell Items";

    public override void OnSelect(Player player)
    {
        UINpcTrading.singleton.panel.SetActive(true);
        UIInventory.singleton.panel.SetActive(true); // better feedback
        UINpcDialogue.singleton.panel.SetActive(false);
    }
}
