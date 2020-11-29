// Understanding the Addon System:
//
// 1. Unity Components:
//    Unlike uMMORPG Classic, uMMORPG Components Edition supports addons that
//    regular Unity components. Simply inherit from NetworkBehaviour or
//    MonoBehaviour, add it to the Player/Monster/Npc prefabs and hook into the
//    events provided by the base components. For example, if you open the
//    Player prefab in the Inspector, then you can see that the Health component
//    has an OnEmpty event. You can plug into this with as many addon components
//    as you want. Check out the AddonExample component below.
//
// 2. partial classes:
//    The C# compiler will include all 'partial' classes of the same type into
//    one big class. In other words, it's a way to split a big class into many
//    smaller files.
//
//    We use partial classes for the remaining classes that can't have extra
//    components on them. For example, ScriptableItem or Item.
//    The compiler includes all partial classes into one class when compiling.
//
// Why:
//    There are two main benefits of using this addon system:
//        1. Updates to the core files won't overwrite your modifications.
//        2. Sharing addons is way easier. All it takes is one addon script.
//
// Final Note:
//    No addon system allows 100% modifications. There might be cases where you
//    still have to modify the core scripts. If so, it's recommended to write
//    down the necessary modifications for your addons in the comment section.
//
//    If your addon needs to know about a specific event in one of uMMORPG's
//    core scripts, I can also add it to the code. Just contact me in Discord.
//
// IMPORTANT:
//    The addons folder must be inside the Scripts folder (=using the same
//    assembly definition), otherwise extending partial classes won't work.
//
////////////////////////////////////////////////////////////////////////////////
// Example Addon
//    Author: ...
//
//    Description: ...
//
//    Required Core modifications: ...
//
//    Usage: ...
//
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

// a regular addon /////////////////////////////////////////////////////////////
public class AddonExample : NetworkBehaviour
{
    // hook this into the Health component's OnEmpty method in the Inspector
    public void OnDeath()
    {
        Debug.LogWarning("ExampleAddon: OnDeath!");
    }

    // we can also provide our own hooks for additional addons/components to use
    // add the AddonExample to a prefab, then you can see the TestEvent in the
    // Inspector and hook other component's functions into it.
    public UnityEvent TestEvent;
    void Start()
    {
        TestEvent.Invoke();
    }

    // we can also use UnityEvents with parameters, for example:
    public UnityEventInt TestEventInt;
    void Update()
    {
        TestEventInt.Invoke(42);
    }
}

// items ///////////////////////////////////////////////////////////////////////
public partial class ItemTemplate
{
    //[Header("My Addon")]
    //public int addonVariable = 0;
}

// note: can't add variables yet without modifying original constructor
public partial struct Item
{
    //public int addonVariable {
    //    get { return template.addonVariable; }
    //}

    void ToolTip_Example(StringBuilder tip)
    {
        //tip.Append("");
    }
}

// skills //////////////////////////////////////////////////////////////////////
public partial class SkillTemplate
{
    //[Header("My Addon")]
    //public int addonVariable = 0;

    public partial struct SkillLevel
    {
        // note: adding variables here will give lots of warnings, but it works.
        //public int addonVariable;
    }
}

// note: can't add variables yet without modifying original constructor
public partial struct Skill
{
    //public int addonVariable
    //{
    //    get { return template.addonVariable; }
    //}

    void ToolTip_Example(StringBuilder tip)
    {
        //tip.Append("");
    }
}

// quests //////////////////////////////////////////////////////////////////////
public partial class QuestTemplate
{
    //[Header("My Addon")]
    //public int addonVariable = 0;
}

// note: can't add variables yet without modifying original constructor
public partial struct Quest
{
    //public int addonVariable
    //{
    //    get { return template.addonVariable; }
    //}

    void ToolTip_Example(StringBuilder tip)
    {
        //tip.Append("");
    }
}

// network messages ////////////////////////////////////////////////////////////
// all network messages can be extended
public partial class LoginMsg
{
}

// here is how to pass more data to the available message to show health, mana
// etc. in the character selection UI if necessary
public partial class CharactersAvailableMsg
{
    public partial struct CharacterPreview
    {
        //public int health;
    }
    void Load_Example(List<Player> players)
    {
        //for (int i = 0; i < players.Count; ++i)
        //    characters[i].health = players[i].health;
    }
}
