﻿using UnityEngine;

public class NpcTeleport : NpcOffer
{
    [Header("Teleportation")]
    public Transform destination;

    public override bool HasOffer(PlayerOLD player) =>
        destination != null;

    public override string GetOfferName() =>
        "Teleport: " + destination.name;

    public override void OnSelect(PlayerOLD player)
    {
        player.npcTeleport.CmdNpcTeleport();
    }
}
