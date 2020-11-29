// npc brain does nothing but stand around
using UnityEngine;

[CreateAssetMenu(menuName="uMMORPG Brain/Brains/Npc", order=999)]
public class NpcBrain : CommonBrain
{
    public override string UpdateServer(Entity entity) { return entity.state; }
    public override void UpdateClient(Entity entity) {}
}
