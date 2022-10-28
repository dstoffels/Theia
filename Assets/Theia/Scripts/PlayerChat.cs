// We implemented a chat system that works directly with UNET. The chat supports
// different channels that can be used to communicate with other players:
//
// - **Local Chat:** by default, all messages that don't start with a **/** are
// addressed to the local chat. If one player writes a local message, then all
// players around him _(all observers)_ will be able to see the message.
// - **Whisper Chat:** a player can write a private message to another player by
// using the **/ name message** format.
// - **Guild Chat:** we implemented guild chat support with the **/g message**
// - **Info Chat:** the info chat can be used by the server to notify all
// players about important news. The clients won't be able to write any info
// messages.
//
// _Note: the channel names, colors and commands can be edited in the Inspector_
using System;
using UnityEngine;
using Mirror;

[Serializable]
public class ChannelInfo
{
    public string command; // /w etc.
    public string identifierOut; // for sending
    public string identifierIn; // for receiving
    public GameObject textPrefab;

    public ChannelInfo(string command, string identifierOut, string identifierIn, GameObject textPrefab)
    {
        this.command = command;
        this.identifierOut = identifierOut;
        this.identifierIn = identifierIn;
        this.textPrefab = textPrefab;
    }
}

[Serializable]
public struct ChatMessage
{
    public string sender;
    public string identifier;
    public string message;
    public string replyPrefix; // copied to input when clicking the message
    public GameObject textPrefab;

    public ChatMessage(string sender, string identifier, string message, string replyPrefix, GameObject textPrefab)
    {
        this.sender = sender;
        this.identifier = identifier;
        this.message = message;
        this.replyPrefix = replyPrefix;
        this.textPrefab = textPrefab;
    }

    // construct the message
    public string Construct()
    {
        return "<b>" + sender + identifier + ":</b> " + message;
    }
}

[RequireComponent(typeof(PlayerGuild))]
[RequireComponent(typeof(PlayerParty))]
[DisallowMultipleComponent]
public class PlayerChat : NetworkBehaviour
{
    [Header("Components")] // to be assigned in inspector
    public PlayerGuild guild;
    public PlayerParty party;

    [Header("Channels")]
    public ChannelInfo whisperChannel = new ChannelInfo("/w", "(TO)", "(FROM)", null);
    public ChannelInfo localChannel = new ChannelInfo("", "", "", null);
    public ChannelInfo partyChannel = new ChannelInfo("/p", "(Party)", "(Party)", null);
    public ChannelInfo guildChannel = new ChannelInfo("/g", "(Guild)", "(Guild)", null);
    public ChannelInfo infoChannel = new ChannelInfo("", "(Info)", "(Info)", null);

    [Header("Other")]
    public int maxLength = 70;

    [Header("Events")]
    public UnityEventString onSubmit;

    public override void OnStartLocalPlayer()
    {
        // test messages
        UIChat.singleton.AddMessage(new ChatMessage("", infoChannel.identifierIn, "Use /w NAME to whisper", "",  infoChannel.textPrefab));
        UIChat.singleton.AddMessage(new ChatMessage("", infoChannel.identifierIn, "Use /p for party chat", "",  infoChannel.textPrefab));
        UIChat.singleton.AddMessage(new ChatMessage("", infoChannel.identifierIn, "Use /g for guild chat", "",  infoChannel.textPrefab));
        UIChat.singleton.AddMessage(new ChatMessage("", infoChannel.identifierIn, "Or click on a message to reply", "",  infoChannel.textPrefab));
        UIChat.singleton.AddMessage(new ChatMessage("Someone", guildChannel.identifierIn, "Anyone here?", "/g ",  guildChannel.textPrefab));
        UIChat.singleton.AddMessage(new ChatMessage("Someone", partyChannel.identifierIn, "Let's hunt!", "/p ",  partyChannel.textPrefab));
        UIChat.singleton.AddMessage(new ChatMessage("Someone", whisperChannel.identifierIn, "Are you there?", "/w Someone ",  whisperChannel.textPrefab));
        UIChat.singleton.AddMessage(new ChatMessage("Someone", localChannel.identifierIn, "Hello!", "/w Someone ",  localChannel.textPrefab));
    }

    // submit tries to send the string and then returns the new input text
    [Client]
    public string OnSubmit(string text)
    {
        // not empty and not only spaces?
        if (!string.IsNullOrWhiteSpace(text))
        {
            // command in the commands list?
            // note: we don't do 'break' so that one message could potentially
            //       be sent to multiple channels (see mmorpg local chat)
            string lastCommand = "";
            if (text.StartsWith(whisperChannel.command))
            {
                // whisper
                (string user, string message) = ParsePM(whisperChannel.command, text);
                if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(message))
                {
                    if (user != name)
                    {
                        lastCommand = whisperChannel.command + " " + user + " ";
                        CmdMsgWhisper(user, message);
                    }
                    else Debug.Log("cant whisper to self");
                }
                else Debug.Log("invalid whisper format: " + user + "/" + message);
            }
            else if (!text.StartsWith("/"))
            {
                // local chat is special: it has no command
                lastCommand = "";
                CmdMsgLocal(text);
            }
            else if (text.StartsWith(partyChannel.command))
            {
                // party
                string msg = ParseGeneral(partyChannel.command, text);
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    lastCommand = partyChannel.command + " ";
                    CmdMsgParty(msg);
                }
            }
            else if (text.StartsWith(guildChannel.command))
            {
                // guild
                string msg = ParseGeneral(guildChannel.command, text);
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    lastCommand = guildChannel.command + " ";
                    CmdMsgGuild(msg);
                }
            }

            // addon system hooks
            onSubmit.Invoke(text);

            // input text should be set to lastcommand
            return lastCommand;
        }

        // input text should be cleared
        return "";
    }

    // parse a message of form "/command message"
    internal static string ParseGeneral(string command, string msg)
    {
        // return message without command prefix (if any)
        return msg.StartsWith(command + " ") ? msg.Substring(command.Length + 1) : "";
    }

    // parse a private message
    internal static (string user, string message) ParsePM(string command, string pm)
    {
        // parse to /w content
        string content = ParseGeneral(command, pm);

        // now split the content in "user msg"
        if (content != "")
        {
            // find the first space that separates the name and the message
            int i = content.IndexOf(" ");
            if (i >= 0)
            {
                string user = content.Substring(0, i);
                string msg = content.Substring(i+1);
                return (user, msg);
            }
        }
        return ("", "");
    }

    // networking //////////////////////////////////////////////////////////////
    [Command]
    void CmdMsgLocal(string message)
    {
        if (message.Length > maxLength) return;

        // it's local chat, so let's send it to all observers via ClientRpc
        RpcMsgLocal(name, message);
    }

    [Command]
    void CmdMsgParty(string message)
    {
        if (message.Length > maxLength) return;

        // send message to all online party members
        if (party.InParty())
        {
            foreach (string member in party.party.members)
            {
                if (PlayerOLD.onlinePlayers.TryGetValue(member, out PlayerOLD onlinePlayer))
                {
                    // call TargetRpc on that GameObject for that connection
                    onlinePlayer.chat.TargetMsgParty(name, message);
                }
            }
        }
    }

    [Command]
    void CmdMsgGuild(string message)
    {
        if (message.Length > maxLength) return;

        // send message to all online guild members
        if (guild.InGuild())
        {
            foreach (GuildMember member in guild.guild.members)
            {
                if (PlayerOLD.onlinePlayers.TryGetValue(member.name, out PlayerOLD onlinePlayer))
                {
                    // call TargetRpc on that GameObject for that connection
                    onlinePlayer.chat.TargetMsgGuild(name, message);
                }
            }
        }
    }

    [Command]
    void CmdMsgWhisper(string playerName, string message)
    {
        if (message.Length > maxLength) return;

        // find the player with that name
        if (PlayerOLD.onlinePlayers.TryGetValue(playerName, out PlayerOLD onlinePlayer))
        {
            // receiver gets a 'from' message, sender gets a 'to' message
            // (call TargetRpc on that GameObject for that connection)
            onlinePlayer.chat.TargetMsgWhisperFrom(name, message);
            TargetMsgWhisperTo(playerName, message);
        }
    }

    // send a global info message to everyone
    [Server]
    public void SendGlobalMessage(string message)
    {
        foreach (PlayerOLD player in PlayerOLD.onlinePlayers.Values)
            player.chat.TargetMsgInfo(message);
    }

    // message handlers ////////////////////////////////////////////////////////
    [TargetRpc]
    public void TargetMsgWhisperFrom(string sender, string message)
    {
        // add message with identifierIn
        string identifier = whisperChannel.identifierIn;
        string reply = whisperChannel.command + " " + sender + " "; // whisper
        UIChat.singleton.AddMessage(new ChatMessage(sender, identifier, message, reply, whisperChannel.textPrefab));
    }

    [TargetRpc]
    public void TargetMsgWhisperTo(string receiver, string message)
    {
        // add message with identifierOut
        string identifier = whisperChannel.identifierOut;
        string reply = whisperChannel.command + " " + receiver + " "; // whisper
        UIChat.singleton.AddMessage(new ChatMessage(receiver, identifier, message, reply, whisperChannel.textPrefab));
    }

    [ClientRpc]
    public void RpcMsgLocal(string sender, string message)
    {
        // add message with identifierIn or Out depending on who sent it
        string identifier = sender != name ? localChannel.identifierIn : localChannel.identifierOut;
        string reply = whisperChannel.command + " " + sender + " "; // whisper
        UIChat.singleton.AddMessage(new ChatMessage(sender, identifier, message, reply, localChannel.textPrefab));
    }

    [TargetRpc]
    public void TargetMsgGuild(string sender, string message)
    {
        string reply = whisperChannel.command + " " + sender + " "; // whisper
        UIChat.singleton.AddMessage(new ChatMessage(sender, guildChannel.identifierIn, message, reply, guildChannel.textPrefab));
    }

    [TargetRpc]
    public void TargetMsgParty(string sender, string message)
    {
        string reply = whisperChannel.command + " " + sender + " "; // whisper
        UIChat.singleton.AddMessage(new ChatMessage(sender, partyChannel.identifierIn, message, reply, partyChannel.textPrefab));
    }

    [TargetRpc]
    public void TargetMsgInfo(string message)
    {
        AddMsgInfo(message);
    }

    // info message can be added from client too
    public void AddMsgInfo(string message)
    {
        UIChat.singleton.AddMessage(new ChatMessage("", infoChannel.identifierIn, message, "", infoChannel.textPrefab));
    }
}
