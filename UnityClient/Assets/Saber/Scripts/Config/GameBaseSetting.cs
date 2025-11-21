using Saber.Frame;
using UnityEngine;
using UnityEngine.Serialization;

namespace Saber.Config
{
    [CreateAssetMenu(menuName = "Saber/GameBaseSetting", fileName = "GameBaseSetting", order = 0)]
    public class GameBaseSetting : ScriptableObject
    {
        private static GameBaseSetting s_Instance;

        public static GameBaseSetting Instance
        {
            get
            {
                if (!s_Instance )
                {
                    s_Instance = Resources.Load<GameBaseSetting>("GameBaseSetting");
                }

                return s_Instance;
            }
        }
        
        public bool EditorUseBundleAsset;
        public EGameStyle GameStyle;
    }

    public enum EGameStyle
    {
        WUCH,
        DebugAsset,
    }
}
