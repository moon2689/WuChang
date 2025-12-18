using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Saber
{
    public class URPFeatureSceneScan : ScriptableRendererFeature
    {
        class URPPassSceneScan : ScriptableRenderPass
        {
            Settings m_Settings;
            RenderTargetIdentifier m_source;
            RenderTargetIdentifier m_dest;
            int m_TempRTName = Shader.PropertyToID($"{nameof(URPFeatureDemo)}_TempRT");

            public URPPassSceneScan(Settings settings)
            {
                m_Settings = settings;
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                m_source = renderingData.cameraData.renderer.cameraColorTarget;

                cmd.GetTemporaryRT(m_TempRTName, renderingData.cameraData.cameraTargetDescriptor);
                m_dest = new RenderTargetIdentifier(m_TempRTName);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                //m_Settings.m_Mat.SetVector("_CenterPos", m_Settings.m_CenterPos);
                //m_Settings.m_Mat.SetFloat("_ChangeAmount", m_Settings.m_ChangeAmount);

                CommandBuffer cmd = CommandBufferPool.Get($"{nameof(URPFeatureDemo)}_cmd");
                Blit(cmd, m_source, m_dest, m_Settings.m_Mat, 0);
                Blit(cmd, m_dest, m_source);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        [Serializable]
        public class Settings
        {
            public Material m_Mat;
        }

        public static bool s_IsActibe;
        public static Material s_Material;

        [SerializeField] Settings m_Settings;
        URPPassSceneScan m_Pass;


        public override void Create()
        {
            m_Pass = new URPPassSceneScan(m_Settings);
            s_Material = m_Settings.m_Mat;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (s_IsActibe && m_Settings.m_Mat)
            {
                renderer.EnqueuePass(m_Pass);
            }
        }
    }
}