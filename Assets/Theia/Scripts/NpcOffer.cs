// NPCs can offer various services, like teleport, guild management, trading, etc.
// => we use an abstract NpcOffer class so that the UI can build the dialogue
//    automatically.
// => this way Npc dialogues only show what they offer. We don't have to hard
//    code any possible type of offer there anymore.
using Mirror;

public abstract class NpcOffer : NetworkBehaviour
{
    // does the npc have an offer for this player right now?
    // (for example, don't show Quests button if all quests were finished)
    public abstract bool HasOffer(Player player);

    // offer name for the UI npc dialogue entry
    public abstract string GetOfferName();

    // called when the player clicks the offer button
    public abstract void OnSelect(Player player);
}
