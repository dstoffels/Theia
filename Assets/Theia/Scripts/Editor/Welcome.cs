// Shows either a welcome message, or warning about a recommended Unity version.
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DOTSNET.Editor
{
    static class Welcome
    {
        [InitializeOnLoadMethod]
        static void OnInitializeOnLoad()
        {
            // InitializeOnLoad is called on start and after each rebuild,
            // but we only want to show this once per editor session.
            if (!SessionState.GetBool("UMMORPG_WELCOME", false))
            {
                SessionState.SetBool("UMMORPG_WELCOME", true);

#if UNITY_2019_4
                Debug.Log("uMMORPG Remastered | u3d.as/1GXA | https://discord.gg/2gNKN78");
#else
                Debug.LogWarning("uMMORPG Remastered works best with Unity 2019.4 LTS: https://unity3d.com/unity/qa/lts-releases?version=2019.4");
#endif
            }
        }
    }
}
#endif