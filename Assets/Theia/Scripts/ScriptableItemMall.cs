// most (if not all) player prefabs use the same item mall configuration. let's
// put it all into one scriptable object.
//
// => way easier to modify without updating every single player prefab each time
using UnityEngine;

[CreateAssetMenu(menuName="uMMORPG Item Mall", order=999)]
public class ScriptableItemMall : ScriptableObject
{
    [Tooltip("The items that can be purchased in the item mall.")]
    public ItemMallCategory[] categories;
}
