using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SakashoUISystem
{
    [AddComponentMenu("UISystem/Utility/NotEditable")]
    public class NotEditable : MonoBehaviour
    {
        [SerializeField]
        public List<Component> ComponentList = new List<Component>();
        
        [SerializeField]
        public List<bool> EditableFlagList = new List<bool>();
        
        public void Clear()
        {
            foreach (var component in ComponentList) {
                component.hideFlags = HideFlags.None;
            }
            ComponentList.Clear();
            EditableFlagList.Clear();
        }
    }
}