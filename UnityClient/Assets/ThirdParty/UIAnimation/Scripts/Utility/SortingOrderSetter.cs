using UnityEngine;

namespace SakashoUISystem
{
    public class SortingOrderSetter : MonoBehaviour
    {
        [SerializeField]
        private GameObject target;
        
        [SerializeField]
        private int sortOrder;
        
        private void Awake()
        {
            target.GetComponent<Renderer>().sortingOrder = sortOrder;
        }
    }
}
