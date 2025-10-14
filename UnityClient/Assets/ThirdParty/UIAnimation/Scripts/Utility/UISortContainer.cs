using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SakashoUISystem
{
    [AddComponentMenu("UISystem/Utility/UISortContainer")]
    public class UISortContainer : MonoBehaviour
    {
        public enum SortAxisType
        {
            X,
            Y,
            Z
        }
        
        public enum OrderType
        {
            Ascending,
            Descending
        }
        
        [SerializeField]
        private SortAxisType sortAxis = SortAxisType.Z;
        public SortAxisType SortAxis
        {
            get { return sortAxis; }
            set { sortAxis = value; }
        }
        
        [SerializeField]
        private OrderType order = OrderType.Ascending;
        public OrderType Order
        {
            get { return order; }
            set { order = value; }
        }
    
        [SerializeField]
        private List<RectTransform> rectTransformList = new List<RectTransform>();
        public List<RectTransform> RectTransformList { get { return rectTransformList; } }
        
        public void Sort()
        {
            RectTransform[] rectTransforms = null;
            
            if (sortAxis == SortAxisType.X) {
                if (Order == OrderType.Ascending) {
                    rectTransforms = rectTransformList.OrderBy(ui => ui.gameObject.transform.localPosition.x).ToArray();
                } else {
                    rectTransforms = rectTransformList.OrderByDescending(ui => ui.gameObject.transform.localPosition.x).ToArray();
                }
            } else if (sortAxis == SortAxisType.Y) {
                if (Order == OrderType.Ascending) {
                    rectTransforms = rectTransformList.OrderBy(ui => ui.gameObject.transform.localPosition.y).ToArray();
                } else {
                    rectTransforms = rectTransformList.OrderByDescending(ui => ui.gameObject.transform.localPosition.y).ToArray();
                }
            } else {
                if (Order == OrderType.Ascending) {
                    rectTransforms = rectTransformList.OrderBy(ui => ui.gameObject.transform.localPosition.z).ToArray();
                } else {
                    rectTransforms = rectTransformList.OrderByDescending(ui => ui.gameObject.transform.localPosition.z).ToArray();
                }
            }
            
            if (rectTransforms == null) {
                return;
            }
            
            // ヒエラルキー上の親子関係をソート
            for (int i = 0; i < rectTransforms.Count (); i++) {
                rectTransforms.ElementAt(i).transform.SetSiblingIndex(i);
            }
        }    
    }
}