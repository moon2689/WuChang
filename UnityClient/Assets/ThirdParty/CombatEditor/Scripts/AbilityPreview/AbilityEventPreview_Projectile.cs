using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CombatEditor
{
# if UNITY_EDITOR
    public class AbilityEventPreview_Projectile : AbilityEventPreview_CreateObjWithHandle
    {
        GameObject m_Projectile;

        public AbilityEventObj_Projectile Obj => (AbilityEventObj_Projectile)m_EventObj;

        public AbilityEventPreview_Projectile(AbilityEventObj Obj) : base(Obj)
        {
            m_EventObj = Obj;
        }
    }

#endif
}