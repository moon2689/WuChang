using UnityEngine;
using System.Collections;

namespace SakashoUISystem
{
    [AddComponentMenu("UISystem/Utility/UIDescription")]
    public class UIDescription : MonoBehaviour
    {
        [SerializeField]
        [TextAreaAttribute(3, 10)]
        private string description;
    }
}