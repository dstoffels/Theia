public class NpcGuildManagement : NpcOffer
{
    public override bool HasOffer(PlayerOLD player) => true;

    public override string GetOfferName() => "Guild";

    public override void OnSelect(PlayerOLD player)
    {
        UINpcGuildManagement.singleton.panel.SetActive(true);
        UINpcDialogue.singleton.panel.SetActive(false);
    }
}
