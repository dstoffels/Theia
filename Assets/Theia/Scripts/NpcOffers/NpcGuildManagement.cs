public class NpcGuildManagement : NpcOffer
{
    public override bool HasOffer(Player player) => true;

    public override string GetOfferName() => "Guild";

    public override void OnSelect(Player player)
    {
        UINpcGuildManagement.singleton.panel.SetActive(true);
        UINpcDialogue.singleton.panel.SetActive(false);
    }
}
