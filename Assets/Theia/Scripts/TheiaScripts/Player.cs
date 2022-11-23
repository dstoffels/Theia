using System;
using Mirror;
using Stats;
using UnityEngine;
using InventoryStuff.Armor;
using InventoryStuff;
using Items;
using Sirenix.OdinInspector;
using Stats.Anatomy;

[RequireComponent(typeof(Attributes), typeof(Skills))]
//[RequireComponent(typeof(Stamina), typeof(Mana), typeof(Blood))]
//[RequireComponent(typeof(Temperature), typeof(Celerity))]
[RequireComponent(typeof(Gear), typeof(Armor))]
public class Player : NetworkBehaviour
{
    public static Player localPlayer;

    public Attributes attributes;
    public Skills skills;
    public Vitals vitals;
    public Anatomy anatomy;
    //public Stamina stamina;
    //public Mana mana;
    //public Blood blood;
    //public Temperature temperature;
    //public Celerity celerity;
    public Gear gear;
    public Armor armor;

    public override void OnStartLocalPlayer()
    {
        // set singleton
        localPlayer = this;
    }

    private void OnValidate()
    {
        Init();
    }

    private void Reset()
    {
        Init();
    }

    [Button]
    public void Init()
    {
        PlayerHelpers.AssignComponents(this);
        PlayerHelpers.InitializeComponents(this);
    }


    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);

            var item = hit.collider.GetComponent<IItem>();
            if (item != null)
                item.PickUp(this);

        }
    }
}

public struct PlayerHelpers
{
    public static void AssignComponents(Player player)
    {
        player.attributes ??= player.GetComponent<Attributes>();
        player.skills ??= player.GetComponent<Skills>();
        player.anatomy ??= player.GetComponent<Anatomy>();
        player.vitals ??= player.GetComponent<Vitals>();
        //player.stamina ??= player.GetComponent<Stamina>();
        //player.mana ??= player.GetComponent<Mana>();
        //player.blood ??= player.GetComponent<Blood>();
        //player.temperature ??= player.GetComponent<Temperature>();
        //player.celerity ??= player.GetComponent<Celerity>();
        player.gear ??= player.GetComponent<InventoryStuff.Gear>();
        player.armor ??= player.GetComponent<Armor>();

    }

    public static void InitializeComponents(Player player)
    {
        player.attributes.InitializeTemplate();
        player.skills.InitializeTemplate();
        player.vitals.InitializeTemplate();
        player.anatomy.InitializeTemplate();

        player.attributes.SubscribeAll(player.skills);
        player.skills.SubscribeAll(player.attributes);
        player.vitals.SubscribeAll(player.attributes);
        player.anatomy.SubscribeAll(player.attributes);

    }
}
