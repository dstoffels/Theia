// a simple gather quest example
using UnityEngine;
using System.Text;

[CreateAssetMenu(menuName="uMMORPG Quest/Gather Quest", order=999)]
public class GatherQuest : ScriptableQuest
{
    [Header("Fulfillment")]
    public ScriptableItem gatherItem;
    public int gatherAmount;

    // fulfillment /////////////////////////////////////////////////////////////
    public override bool IsFulfilled(PlayerOLD player, Quest quest)
    {
        return gatherItem != null &&
               player.inventory.Count(new ItemOLD(gatherItem)) >= gatherAmount;
    }

    public override void OnCompleted(PlayerOLD player, Quest quest)
    {
        // remove gathered items from player's inventory
        if (gatherItem != null)
            player.inventory.Remove(new ItemOLD(gatherItem), gatherAmount);
    }

    // tooltip /////////////////////////////////////////////////////////////////
    public override string ToolTip(PlayerOLD player, Quest quest)
    {
        // we use a StringBuilder so that addons can modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(base.ToolTip(player, quest));
        tip.Replace("{GATHERAMOUNT}", gatherAmount.ToString());
        if (gatherItem != null)
        {
            int gathered = player.inventory.Count(new ItemOLD(gatherItem));
            tip.Replace("{GATHERITEM}", gatherItem.name);
            tip.Replace("{GATHERED}", Mathf.Min(gathered, gatherAmount).ToString());
        }
        return tip.ToString();
    }
}
