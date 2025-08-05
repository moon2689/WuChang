using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Saber
{
    public class URPFeatureDemo : ScriptableRendererFeature
    {
        class URPPassDemo : ScriptableRenderPass
        {
            Settings m_Settings;
            RenderTargetIdentifier m_source;
            RenderTargetIdentifier m_dest;
            int m_TempRTName = Shader.PropertyToID($"{nameof(URPFeatureDemo)}_TempRT");

            public URPPassDemo(Settings settings)
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
                m_Settings.m_Mat.SetColor("_Color", m_Settings.m_Color);

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
            public Color m_Color = Color.white;
        }

        [SerializeField] Settings m_Settings;
        URPPassDemo m_Pass;


        public override void Create()
        {
            m_Pass = new URPPassDemo(m_Settings);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (m_Settings.m_Mat)
            {
                renderer.EnqueuePass(m_Pass);
            }
        }
    }
}