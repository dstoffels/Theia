// We use a custom NetworkManager that also takes care of login, character
// selection, character creation and more.
//
// We don't use the playerPrefab, instead all available player classes should be
// dragged into the spawnable objects property.
//
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Mirror;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

// we need a clearly defined state to know if we are offline/in world/in lobby
// otherwise UICharacterSelection etc. never know 100% if they should be visible
// or not.
public enum NetworkState { Offline, Handshake, Lobby, World }

[Serializable] public class UnityEventCharactersAvailableMsg : UnityEvent<CharactersAvailableMsg> {}
[Serializable] public class UnityEventCharacterCreateMsgPlayer : UnityEvent<CharacterCreateMsg, PlayerOLD> {}
[Serializable] public class UnityEventStringGameObjectNetworkConnectionCharacterSelectMsg : UnityEvent<string, GameObject, NetworkConnection, CharacterSelectMsg> {}
[Serializable] public class UnityEventCharacterDeleteMsg : UnityEvent<CharacterDeleteMsg> {}

[RequireComponent(typeof(Database))]
[DisallowMultipleComponent]
public partial class NetworkManagerMMO : NetworkManager
{
    // current network manager state on client
    public NetworkState state = NetworkState.Offline;

    // <conn, account> dict for the lobby
    // (people that are still creating or selecting characters)
    public Dictionary<NetworkConnection, string> lobby = new Dictionary<NetworkConnection, string>();

    // UI components to avoid FindObjectOfType
    [Header("UI")]
    public UIPopup uiPopup;

    // we may want to add another game server if the first one gets too crowded.
    // the server list allows people to choose a server.
    //
    // note: we use one port for all servers, so that a headless server knows
    // which port to bind to. otherwise it would have to know which one to
    // choose from the list, which is far too complicated. one port for all
    // servers will do just fine for an Indie MMORPG.
    [Serializable]
    public class ServerInfo
    {
        public string name;
        public string ip;
    }
    public List<ServerInfo> serverList = new List<ServerInfo>() {
        new ServerInfo{name="Local", ip="localhost"}
    };

    [Header("Logout")]
    [Tooltip("Players shouldn't be able to log out instantly to flee combat. There should be a delay.")]
    public float combatLogoutDelay = 5;

    [Header("Character Selection")]
    public int selection = -1;
    public Transform[] selectionLocations;
    public Transform selectionCameraLocation;
    [HideInInspector] public List<PlayerOLD> playerClasses = new List<PlayerOLD>(); // cached in Awake

    [Header("Database")]
    public int characterLimit = 4;
    public int characterNameMaxLength = 16;
    public float saveInterval = 60f; // in seconds

    // we still need OnStartClient/Server/etc. events for NetworkManager because
    // those are not regular NetworkBehaviour events that all components get.
    [Header("Events")]
    public UnityEvent onStartClient;
    public UnityEvent onStopClient;
    public UnityEvent onStartServer;
    public UnityEvent onStopServer;
    public UnityEventNetworkConnection onClientConnect;
    public UnityEventNetworkConnection onServerConnect;
    public UnityEventCharactersAvailableMsg onClientCharactersAvailable;
    public UnityEventCharacterCreateMsgPlayer onServerCharacterCreate;
    public UnityEventStringGameObjectNetworkConnectionCharacterSelectMsg onServerCharacterSelect;
    public UnityEventCharacterDeleteMsg onServerCharacterDelete;
    public UnityEventNetworkConnection onClientDisconnect;
    public UnityEventNetworkConnection onServerDisconnect;

    // store characters available message on client so that UI can access it
    [HideInInspector] public CharactersAvailableMsg charactersAvailableMsg;

    // name checks /////////////////////////////////////////////////////////////
    public bool IsAllowedCharacterName(string characterName)
    {
        // not too long?
        // only contains letters, number and underscore and not empty (+)?
        // (important for database safety etc.)
        return characterName.Length <= characterNameMaxLength &&
               Regex.IsMatch(characterName, @"^[a-zA-Z0-9_]+$");
    }

    // nearest startposition ///////////////////////////////////////////////////
    public static Transform GetNearestStartPosition(Vector3 from) =>
        Utils.GetNearestTransform(startPositions, from);

    // player classes //////////////////////////////////////////////////////////]
    public List<PlayerOLD> FindPlayerClasses()
    {
        // filter out all Player prefabs from spawnPrefabs
        // (avoid Linq for performance/gc. players are spawned a lot. it matters.)
        List<PlayerOLD> classes = new List<PlayerOLD>();
        foreach (GameObject prefab in spawnPrefabs)
        {
            PlayerOLD player = prefab.GetComponent<PlayerOLD>();
            if (player != null)
                classes.Add(player);
        }
        return classes;
    }

    // events //////////////////////////////////////////////////////////////////
    public override void Awake()
    {
        base.Awake();

        // cache list of player classes from spawn prefabs.
        // => we assume that this won't be changed at runtime (why would it?)
        // => this is way better than looping all prefabs in character
        //    select/create/delete each time!
        playerClasses = FindPlayerClasses();
    }

    void Update()
    {
        // any valid local player? then set state to world
        if (NetworkClient.localPlayer != null)
            state = NetworkState.World;
    }

    // error messages //////////////////////////////////////////////////////////
    public void ServerSendError(NetworkConnection conn, string error, bool disconnect)
    {
        conn.Send(new ErrorMsg{text=error, causesDisconnect=disconnect});
    }

    void OnClientError(ErrorMsg message)
    {
        Debug.Log("OnClientError: " + message.text);

        // show a popup
        uiPopup.Show(message.text);

        // disconnect if it was an important network error
        // (this is needed because the login failure message doesn't disconnect
        //  the client immediately (only after timeout))
        if (message.causesDisconnect)
        {
            NetworkClient.connection.Disconnect();

            // also stop the host if running as host
            // (host shouldn't start server but disconnect client for invalid
            //  login, which would be pointless)
            if (NetworkServer.active) StopHost();
        }
    }

    // start & stop ////////////////////////////////////////////////////////////
    public override void OnStartClient()
    {
        // setup handlers
        NetworkClient.RegisterHandler<ErrorMsg>(OnClientError, false); // allowed before auth!
        NetworkClient.RegisterHandler<CharactersAvailableMsg>(OnClientCharactersAvailable);

        // addon system hooks
        onStartClient.Invoke();
    }

    public override void OnStartServer()
    {
        // connect to database
        Database.singleton.Connect();

        // handshake packet handlers (in OnStartServer so that reconnecting works)
        NetworkServer.RegisterHandler<CharacterCreateMsg>(OnServerCharacterCreate);
        NetworkServer.RegisterHandler<CharacterSelectMsg>(OnServerCharacterSelect);
        NetworkServer.RegisterHandler<CharacterDeleteMsg>(OnServerCharacterDelete);

        // invoke saving
        InvokeRepeating(nameof(SavePlayers), saveInterval, saveInterval);

        // addon system hooks
        onStartServer.Invoke();
    }

    public override void OnStopClient()
    {
        // addon system hooks
        onStopClient.Invoke();
    }

    public override void OnStopServer()
    {
        CancelInvoke(nameof(SavePlayers));

        // addon system hooks
        onStopServer.Invoke();
    }

    // handshake: login ////////////////////////////////////////////////////////
    // called on the client if a client connects after successful auth
    public override void OnClientConnect(NetworkConnection conn)
    {
        // addon system hooks
        onClientConnect.Invoke(conn);

        // do NOT call base function, otherwise client becomes "ready".
        // => it should only be "ready" after selecting a character. otherwise
        //    it might receive world messages from monsters etc. already
        //base.OnClientConnect(conn);
    }

    // called on the server if a client connects after successful auth
    public override void OnServerConnect(NetworkConnection conn)
    {
        // grab the account from the lobby
        string account = lobby[conn];

        // send necessary data to client
        conn.Send(MakeCharactersAvailableMessage(account));

        // addon system hooks
        onServerConnect.Invoke(conn);
    }

    // the default OnClientSceneChanged sets the client as ready automatically,
    // which makes no sense for MMORPG situations. this was more for situations
    // where the server tells all clients to load a new scene.
    // -> setting client as ready will cause 'already set as ready' errors if
    //    we call StartClient before loading a new scene (e.g. for zones)
    // -> it's best to just overwrite this with an empty function
    public override void OnClientSceneChanged(NetworkConnection conn) {}

    // helper function to make a CharactersAvailableMsg from all characters in
    // an account
    CharactersAvailableMsg MakeCharactersAvailableMessage(string account)
    {
        // load from database
        // (avoid Linq for performance/gc. characters are loaded frequently!)
        List<PlayerOLD> characters = new List<PlayerOLD>();
        foreach (string characterName in Database.singleton.CharactersForAccount(account))
        {
            GameObject player = Database.singleton.CharacterLoad(characterName, playerClasses, true);
            characters.Add(player.GetComponent<PlayerOLD>());
        }

        // construct the message
        CharactersAvailableMsg message = new CharactersAvailableMsg();
        message.Load(characters);

        // destroy the temporary players again and return the result
        characters.ForEach(player => Destroy(player.gameObject));
        return message;
    }

    // handshake: character selection //////////////////////////////////////////
    void LoadPreview(GameObject prefab, Transform location, int selectionIndex, CharactersAvailableMsg.CharacterPreview character)
    {
        // instantiate the prefab
        GameObject preview = Instantiate(prefab.gameObject, location.position, location.rotation);
        preview.transform.parent = location;
        PlayerOLD player = preview.GetComponent<PlayerOLD>();

        // assign basic preview values like name and equipment
        player.name = character.name;
        player.isGameMaster = character.isGameMaster;
        for (int i = 0; i < character.equipment.Length; ++i)
        {
            ItemSlot slot = character.equipment[i];
            player.equipment.slots.Add(slot);
            if (slot.amount > 0)
            {
                // OnEquipmentChanged won't be called unless spawned, we
                // need to refresh manually
                ((PlayerEquipment)player.equipment).RefreshLocation(i);
            }
        }

        // add selection script
        preview.AddComponent<SelectableCharacter>();
        preview.GetComponent<SelectableCharacter>().index = selectionIndex;
    }

    public void ClearPreviews()
    {
        selection = -1;
        foreach (Transform location in selectionLocations)
            if (location.childCount > 0)
                Destroy(location.GetChild(0).gameObject);
    }

    void OnClientCharactersAvailable(CharactersAvailableMsg message)
    {
        charactersAvailableMsg = message;
        Debug.Log("characters available:" + charactersAvailableMsg.characters.Length);

        // set state
        state = NetworkState.Lobby;

        // clear previous previews in any case
        ClearPreviews();

        // load previews for 3D character selection
        for (int i = 0; i < charactersAvailableMsg.characters.Length; ++i)
        {
            CharactersAvailableMsg.CharacterPreview character = charactersAvailableMsg.characters[i];

            // find the prefab for that class
            PlayerOLD prefab = playerClasses.Find(p => p.name == character.className);
            if (prefab != null)
                LoadPreview(prefab.gameObject, selectionLocations[i], i, character);
            else
                Debug.LogWarning("Character Selection: no prefab found for class " + character.className);
        }

        // setup camera
        Camera.main.transform.position = selectionCameraLocation.position;
        Camera.main.transform.rotation = selectionCameraLocation.rotation;

        // addon system hooks
        onClientCharactersAvailable.Invoke(charactersAvailableMsg);
    }

    // handshake: character creation ///////////////////////////////////////////
    // find a NetworkStartPosition for this class, or a normal one otherwise
    // (ignore the ones with playerPrefab == null)
    public Transform GetStartPositionFor(string className)
    {
        // avoid Linq for performance/GC. players spawn frequently!
        foreach (Transform startPosition in startPositions)
        {
            NetworkStartPositionForClass spawn = startPosition.GetComponent<NetworkStartPositionForClass>();
            if (spawn != null &&
                spawn.playerPrefab != null &&
                spawn.playerPrefab.name == className)
                return spawn.transform;
        }
        // return any start position otherwise
        return GetStartPosition();
    }

    PlayerOLD CreateCharacter(GameObject classPrefab, string characterName, string account, bool gameMaster)
    {
        // create new character based on the prefab.
        // -> we also assign default items and equipment for new characters
        // -> skills are handled in Database.CharacterLoad every time. if we
        //    add new ones to a prefab, all existing players should get them
        // (instantiate temporary player)
        //Debug.Log("creating character: " + message.name + " " + message.classIndex);
        PlayerOLD player = Instantiate(classPrefab).GetComponent<PlayerOLD>();
        player.name = characterName;
        player.account = account;
        player.className = classPrefab.name;
        player.transform.position = GetStartPositionFor(player.className).position;
        for (int i = 0; i < player.inventory.size; ++i)
        {
            // add empty slot or default item if any
            player.inventory.slots.Add(i < player.inventory.defaultItems.Length ? new ItemSlot(new ItemOLD(player.inventory.defaultItems[i].item), player.inventory.defaultItems[i].amount) : new ItemSlot());
        }
        for (int i = 0; i < ((PlayerEquipment)player.equipment).slotInfo.Length; ++i)
        {
            // add empty slot or default item if any
            EquipmentInfo info = ((PlayerEquipment)player.equipment).slotInfo[i];
            player.equipment.slots.Add(info.defaultItem.item != null ? new ItemSlot(new ItemOLD(info.defaultItem.item), info.defaultItem.amount) : new ItemSlot());
        }
        player.health.current = player.health.max; // after equipment in case of boni
        player.mana.current = player.mana.max; // after equipment in case of boni
        player.isGameMaster = gameMaster;

        return player;
    }

    void OnServerCharacterCreate(NetworkConnection conn, CharacterCreateMsg message)
    {
        //Debug.Log("OnServerCharacterCreate " + conn);

        // only while in lobby (aka after handshake and not ingame)
        if (lobby.ContainsKey(conn))
        {
            // allowed character name?
            if (IsAllowedCharacterName(message.name))
            {
                // not existent yet?
                string account = lobby[conn];
                if (!Database.singleton.CharacterExists(message.name))
                {
                    // not too may characters created yet?
                    if (Database.singleton.CharactersForAccount(account).Count < characterLimit)
                    {
                        // valid class index?
                        if (0 <= message.classIndex && message.classIndex < playerClasses.Count)
                        {
                            // game master can only be requested by the host.
                            // DO NOT allow regular connections to create GMs!
                            if (message.gameMaster == false ||
                                conn == NetworkServer.localConnection)
                            {
                                // create new character based on the prefab.
                                PlayerOLD player = CreateCharacter(playerClasses[message.classIndex].gameObject, message.name, account, message.gameMaster);

                                // addon system hooks
                                onServerCharacterCreate.Invoke(message, player);

                                // save the player
                                Database.singleton.CharacterSave(player, false);
                                Destroy(player.gameObject);

                                // send available characters list again, causing
                                // the client to switch to the character
                                // selection scene again
                                conn.Send(MakeCharactersAvailableMessage(account));
                            }
                            else
                            {
                                //Debug.Log("character insufficient permissions for GM request: " + conn);  <- don't show on live server
                                ServerSendError(conn, "insufficient permissions", false);
                            }
                        }
                        else
                        {
                            //Debug.Log("character invalid class: " + message.classIndex);  <- don't show on live server
                            ServerSendError(conn, "character invalid class", false);
                        }
                    }
                    else
                    {
                        //Debug.Log("character limit reached: " + message.name); <- don't show on live server
                        ServerSendError(conn, "character limit reached", false);
                    }
                }
                else
                {
                    //Debug.Log("character name already exists: " + message.name); <- don't show on live server
                    ServerSendError(conn, "name already exists", false);
                }
            }
            else
            {
                //Debug.Log("character name not allowed: " + message.name); <- don't show on live server
                ServerSendError(conn, "character name not allowed", false);
            }
        }
        else
        {
            //Debug.Log("CharacterCreate: not in lobby"); <- don't show on live server
            ServerSendError(conn, "CharacterCreate: not in lobby", true);
        }
    }

    // overwrite the original OnServerAddPlayer function so nothing happens if
    // someone sends that message.
    public override void OnServerAddPlayer(NetworkConnection conn) { Debug.LogWarning("Use the CharacterSelectMsg instead"); }

    void OnServerCharacterSelect(NetworkConnection conn, CharacterSelectMsg message)
    {
        //Debug.Log("OnServerCharacterSelect");
        // only while in lobby (aka after handshake and not ingame)
        if (lobby.ContainsKey(conn))
        {
            // read the index and find the n-th character
            // (only if we know that he is not ingame, otherwise lobby has
            //  no netMsg.conn key)
            string account = lobby[conn];
            List<string> characters = Database.singleton.CharactersForAccount(account);

            // validate index
            if (0 <= message.index && message.index < characters.Count)
            {
                //Debug.Log(account + " selected player " + characters[index]);

                // load character data
                GameObject go = Database.singleton.CharacterLoad(characters[message.index], playerClasses, false);

                // add to client
                NetworkServer.AddPlayerForConnection(conn, go);

                // addon system hooks
                onServerCharacterSelect.Invoke(account, go, conn, message);

                // remove from lobby
                lobby.Remove(conn);
            }
            else
            {
                Debug.Log("invalid character index: " + account + " " + message.index);
                ServerSendError(conn, "invalid character index", false);
            }
        }
        else
        {
            Debug.Log("CharacterSelect: not in lobby" + conn);
            ServerSendError(conn, "CharacterSelect: not in lobby", true);
        }
    }

    void OnServerCharacterDelete(NetworkConnection conn, CharacterDeleteMsg message)
    {
        //Debug.Log("OnServerCharacterDelete " + conn);

        // only while in lobby (aka after handshake and not ingame)
        if (lobby.ContainsKey(conn))
        {
            string account = lobby[conn];
            List<string> characters = Database.singleton.CharactersForAccount(account);

            // validate index
            if (0 <= message.index && message.index < characters.Count)
            {
                // delete the character
                Debug.Log("delete character: " + characters[message.index]);
                Database.singleton.CharacterDelete(characters[message.index]);

                // addon system hooks
                onServerCharacterDelete.Invoke(message);

                // send the new character list to client
                conn.Send(MakeCharactersAvailableMessage(account));
            }
            else
            {
                Debug.Log("invalid character index: " + account + " " + message.index);
                ServerSendError(conn, "invalid character index", false);
            }
        }
        else
        {
            Debug.Log("CharacterDelete: not in lobby: " + conn);
            ServerSendError(conn, "CharacterDelete: not in lobby", true);
        }
    }

    // player saving ///////////////////////////////////////////////////////////
    // we have to save all players at once to make sure that item trading is
    // perfectly save. if we would invoke a save function every few minutes on
    // each player seperately then it could happen that two players trade items
    // and only one of them is saved before a server crash - hence causing item
    // duplicates.
    void SavePlayers()
    {
        Database.singleton.CharacterSaveMany(PlayerOLD.onlinePlayers.Values);
        if (PlayerOLD.onlinePlayers.Count > 0)
            Debug.Log("saved " + PlayerOLD.onlinePlayers.Count + " player(s)");
    }

    // stop/disconnect /////////////////////////////////////////////////////////
    // called on the server when a client disconnects
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        //Debug.Log("OnServerDisconnect " + conn);

        // players shouldn't be able to log out instantly to flee combat.
        // there should be a delay.
        float delay = 0;
        if (conn.identity != null)
        {
            PlayerOLD player = conn.identity.GetComponent<PlayerOLD>();
            delay = (float)player.remainingLogoutTime;
        }

        StartCoroutine(DoServerDisconnect(conn, delay));
    }

    IEnumerator<WaitForSeconds> DoServerDisconnect(NetworkConnection conn, float delay)
    {
        yield return new WaitForSeconds(delay);

        //Debug.Log("DoServerDisconnect " + conn);

        // save player (if any. nothing to save if disconnecting while in lobby.)
        if (conn.identity != null)
        {
            Database.singleton.CharacterSave(conn.identity.GetComponent<PlayerOLD>(), false);
            Debug.Log("saved:" + conn.identity.name);
        }

        // addon system hooks
        onServerDisconnect.Invoke(conn);

        // remove logged in account after everything else was done
        lobby.Remove(conn); // just returns false if not found

        // do base function logic (removes the player for the connection)
        base.OnServerDisconnect(conn);
    }

    // called on the client if he disconnects
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        Debug.Log("OnClientDisconnect");

        // take the camera out of the local player so it doesn't get destroyed
        // -> this is necessary for character controller movement where a camera
        //    gets parented to a player.
        Camera mainCamera = Camera.main;
        if (mainCamera.transform.parent != null)
            mainCamera.transform.SetParent(null);

        // show a popup so that users know what happened
        uiPopup.Show("Disconnected.");

        // call base function to guarantee proper functionality
        base.OnClientDisconnect(conn);

        // set state
        state = NetworkState.Offline;

        // addon system hooks
        onClientDisconnect.Invoke(conn);
    }

    // universal quit function for editor & build
    public static void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public override void OnValidate()
    {
        base.OnValidate();

        // ip has to be changed in the server list. make it obvious to users.
        if (!Application.isPlaying && networkAddress != "")
            networkAddress = "Use the Server List below!";

        // need enough character selection locations for character limit
        if (selectionLocations.Length != characterLimit)
        {
            // create new array with proper size
            Transform[] newArray = new Transform[characterLimit];

            // copy old values
            for (int i = 0; i < Mathf.Min(characterLimit, selectionLocations.Length); ++i)
                newArray[i] = selectionLocations[i];

            // use new array
            selectionLocations = newArray;
        }
    }
}
