using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Saber.Frame;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Saber.UI
{
    public class Widget_Debug : WidgetBase
    {
        [SerializeField] private Button m_BtnScanScene;

        private Coroutine m_CoroutineScanScene;

        void Awake()
        {
            m_BtnScanScene.onClick.AddListener(OnClickScanScene);
        }

        private void OnClickScanScene()
        {
            if (m_CoroutineScanScene != null)
            {
                m_CoroutineScanScene.StopCoroutine();
            }

            m_CoroutineScanScene = ScanSceneItor().StartCoroutine();
            ParentWnd.Destroy();
        }

        IEnumerator ScanSceneItor()
        {
            URPFeatureSceneScan.s_IsActibe = true;
            Vector3 playerPos = GameApp.Entry.Game.Player.transform.position;
            URPFeatureSceneScan.s_Material.SetVector("_CenterPos", playerPos);
            float timer = 0;
            while (true)
            {
                URPFeatureSceneScan.s_Material.SetFloat("_ChangeAmount", timer);
                timer += Time.deltaTime * 0.2f;
                if (timer > 1)
                {
                    break;
                }

                yield return null;
            }

            URPFeatureSceneScan.s_Material.SetFloat("_ChangeAmount", 1);
            URPFeatureSceneScan.s_IsActibe = false;
            m_CoroutineScanScene = null;
        }
    }
}