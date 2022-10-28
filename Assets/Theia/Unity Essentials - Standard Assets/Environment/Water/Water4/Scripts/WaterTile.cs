using UnityEngine;

namespace UnityStandardAssets.Water
{
    [ExecuteInEditMode]
    public class WaterTile : MonoBehaviour
    {
        public WaterBase waterBase;


        public void Start()
        {
            AcquireComponents();
        }


        void AcquireComponents()
        {
            if (!waterBase)
            {
                if (transform.parent)
                {
                    waterBase = transform.parent.GetComponent<WaterBase>();
                }
                else
                {
                    waterBase = transform.GetComponent<WaterBase>();
                }
            }
        }


#if UNITY_EDITOR
        public void Update()
        {
            AcquireComponents();
        }
#endif


        public void OnWillRenderObject()
        {
            if (waterBase)
            {
                waterBase.WaterTileBeingRendered(transform, Camera.current);
            }
        }
    }
}