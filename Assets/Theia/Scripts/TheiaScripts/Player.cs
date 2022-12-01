using Mirror;
using UnityEngine;
using Theia.Items.deprecated;
using Sirenix.OdinInspector;
using Theia.Stats.anatomy;
using Theia.Stats.vitals;
using Theia.Stats.skills;
using Theia.Stats.attributes;


namespace Theia
{

    [RequireComponent(typeof(Attributes), typeof(Skills))]
[RequireComponent(typeof(Vitals), typeof(Anatomy))]
public class Player : NetworkBehaviour
{
    public static Player localPlayer;

    public Attributes attributes;
    public Skills skills;
    public Vitals vitals;
    public Anatomy anatomy;
    //public Gear gear;

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

            var item = hit.collider.GetComponent<iItem>();
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
        //player.gear ??= player.GetComponent<InventoryStuff.Gear>();

    }

    public static void InitializeComponents(Player player)
    {
        //player.attributes.InitializeTemplate();
        //player.skills.InitializeTemplate();
        //player.vitals.InitializeTemplate();
        //player.anatomy.InitializeTemplate();

        player.attributes.SubscribeAll(player.skills);
        player.skills.SubscribeAll(player.attributes);
        player.vitals.SubscribeAll(player.attributes);
        player.anatomy.SubscribeAll(player.attributes);

    }
}

}