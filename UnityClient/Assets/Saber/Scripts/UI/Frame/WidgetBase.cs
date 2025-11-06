using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Saber.UI
{
    public class WidgetBase : UIItem
    {
        private WndBase m_ParentWnd;

        public WndBase ParentWnd => m_ParentWnd ??= GetComponentInParent<WndBase>();
    }
}