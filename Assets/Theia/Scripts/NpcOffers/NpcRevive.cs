// revives summonable pets etc.
public class NpcRevive : NpcOffer
{
    public override bool HasOffer(PlayerOLD player) => true;

    public override string GetOfferName() => "Revive";

    public override void OnSelect(PlayerOLD player)
    {
        UINpcRevive.singleton.panel.SetActive(true);
        UIInventory.singleton.panel.SetActive(true); // better feedback
        UINpcDialogue.singleton.panel.SetActive(false);
    }
}
