// player game master stats / actions / controls.
using UnityEditor;
using UnityEngine;
using Mirror;

public class PlayerGameMasterTool : NetworkBehaviour
{
    [Header("Components")]
    public PlayerOLD player;

    // note: isGameMaster flag is in Player.cs

    // server data via SyncVar and SyncToOwner is the easiest solution
    [HideInInspector, SyncVar] public int connections;
    [HideInInspector, SyncVar] public int maxConnections;
    [HideInInspector, SyncVar] public int onlinePlayers;
    [HideInInspector, SyncVar] public float uptime;
    [HideInInspector, SyncVar] public int tickRate;

    // tick rate helpers
    int tickRateCounter;
    double tickRateStart;

    // server data /////////////////////////////////////////////////////////////
    public override void OnStartServer()
    {
        // validate: only for GMs
        if (!player.isGameMaster) return;

        // send data to client every few seconds. use syncInterval for it.
        InvokeRepeating(nameof(RefreshData), syncInterval, syncInterval);
    }

    [ServerCallback]
    void Update()
    {
        // validate: only for GMs
        if (!player.isGameMaster) return;

        // measure tick rate to get an idea of server load
        ++tickRateCounter;
        if (NetworkTime.time >= tickRateStart + 1)
        {
            // save tick rate. will be synced to client automatically.
            tickRate = tickRateCounter;

            // start counting again
            tickRateCounter = 0;
            tickRateStart = NetworkTime.time;
        }
    }

    [Server]
    void RefreshData()
    {
        // validate: only for GMs
        if (!player.isGameMaster) return;

        // refresh sync vars. will be synced to client automatically.
        connections = NetworkServer.connections.Count;
        maxConnections = NetworkManager.singleton.maxConnections;
        onlinePlayers = PlayerOLD.onlinePlayers.Count;
        uptime = Time.realtimeSinceStartup;
    }

    [Command]
    public void CmdSendGlobalMessage(string message)
    {
        // validate: only for GMs
        if (!player.isGameMaster) return;

        player.chat.SendGlobalMessage(message);
    }

    [Command]
    public void CmdShutdown()
    {
        // validate: only for GMs
        if (!player.isGameMaster) return;

        NetworkManagerMMO.Quit();
    }

    // character ///////////////////////////////////////////////////////////////
    [Command]
    public void CmdSetCharacterInvincible(bool value)
    {
        // validate: only for GMs
        if (!player.isGameMaster) return;

        player.combat.invincible = value;
    }

    [Command]
    public void CmdSetCharacterLevel(int value)
    {
        // validate: only for GMs
        if (!player.isGameMaster) return;

        player.level.current = Mathf.Clamp(value, 1, player.level.max);
    }

    [Command]
    public void CmdSetCharacterExperience(long value)
    {
        // validate: only for GMs
        if (!player.isGameMaster) return;

        player.experience.current = Utils.Clamp(value, 0, player.experience.max);
    }

    [Command]
    public void CmdSetCharacterSkillExperience(long value)
    {
        // validate: only for GMs
        if (!player.isGameMaster) return;

        if (value > 0)
            ((PlayerSkills)player.skills).skillExperience = value;
    }

    [Command]
    public void CmdSetCharacterGold(long value)
    {
        // validate: only for GMs
        if (!player.isGameMaster) return;

        if (value > 0)
            player.gold = value;
    }

    [Command]
    public void CmdSetCharacterCoins(long value)
    {
        // validate: only for GMs
        if (!player.isGameMaster) return;

        if (value > 0)
            player.itemMall.coins = value;
    }

    // player actions //////////////////////////////////////////////////////////
    [Command]
    public void CmdWarp(string otherPlayer)
    {
        // validate: only for GMs
        if (!player.isGameMaster) return;

        // warp self to other
        if (PlayerOLD.onlinePlayers.TryGetValue(otherPlayer, out PlayerOLD other))
            player.movement.Warp(other.transform.position);
    }

    [Command]
    public void CmdSummon(string otherPlayer)
    {
        // validate: only for GMs
        if (!player.isGameMaster) return;

        // summon other to self and add chat message so the player knows why
        // it happened
        if (PlayerOLD.onlinePlayers.TryGetValue(otherPlayer, out PlayerOLD other))
        {
            other.movement.Warp(player.transform.position);
            other.chat.TargetMsgInfo("A GM summoned you.");
        }
    }

    [Command]
    public void CmdKill(string otherPlayer)
    {
        // validate: only for GMs
        if (!player.isGameMaster) return;

        // kill other and add chat message so the player knows why it happened
        if (PlayerOLD.onlinePlayers.TryGetValue(otherPlayer, out PlayerOLD other))
        {
            other.health.current = 0;
            other.chat.TargetMsgInfo("A GM killed you.");
        }
    }

    [Command]
    public void CmdKick(string otherPlayer)
    {
        // validate: only for GMs
        if (!player.isGameMaster) return;

        // kick other
        if (PlayerOLD.onlinePlayers.TryGetValue(otherPlayer, out PlayerOLD other))
            // TODO add a reason for kick so people don't think they were disconnected
            other.connectionToClient.Disconnect();
    }

    // validation //////////////////////////////////////////////////////////////
    void OnValidate()
    {
        // gm tool data should only ever be synced to owner!
        // observers should not know about it!
        if (syncMode != SyncMode.Owner)
        {
            syncMode = SyncMode.Owner;
#if UNITY_EDITOR
            Undo.RecordObject(this, name + " " + GetType() + " component syncMode changed to Owner.");
#endif
        }
    }
}
