using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Saber
{
    public class URPFeatureBlur : ScriptableRendererFeature
    {
        class URPPass_Blur : ScriptableRenderPass
        {
            Settings m_Settings;
            RenderTargetIdentifier m_Source;
            int m_TempRTID0 = Shader.PropertyToID($"{nameof(URPFeatureBlur)}_TempRT0");
            int m_TempRTID1 = Shader.PropertyToID($"{nameof(URPFeatureBlur)}_TempRT1");

            public URPPass_Blur(Settings settings)
            {
                m_Settings = settings;
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                m_Source = renderingData.cameraData.renderer.cameraColorTargetHandle;
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get($"{nameof(URPFeatureBlur)}_cmd");

                var data = renderingData.cameraData.cameraTargetDescriptor;
                int width = data.width / m_Settings.m_DownSample;
                int height = data.height / m_Settings.m_DownSample;

                cmd.GetTemporaryRT(m_TempRTID0, width, height, 0, FilterMode.Trilinear, RenderTextureFormat.ARGB32);
                cmd.Blit(m_Source, m_TempRTID0);

                for (int i = 0; i < m_Settings.m_Iterations; ++i)
                {
                    m_Settings.m_MatBlur.SetFloat("_BlurSpread", 1 + i * m_Settings.m_BlurSpread);

                    // 第一轮
                    cmd.GetTemporaryRT(m_TempRTID1, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                    cmd.Blit(m_TempRTID0, m_TempRTID1, m_Settings.m_MatBlur, 0);
                    cmd.ReleaseTemporaryRT(m_TempRTID0);

                    // 第二轮
                    cmd.GetTemporaryRT(m_TempRTID0, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                    cmd.Blit(m_TempRTID1, m_TempRTID0, m_Settings.m_MatBlur, 1);
                    cmd.ReleaseTemporaryRT(m_TempRTID1);
                }

                cmd.Blit(m_TempRTID0, m_Source);
                cmd.ReleaseTemporaryRT(m_TempRTID0);
                cmd.ReleaseTemporaryRT(m_TempRTID1);

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        [Serializable]
        public class Settings
        {
            public Material m_MatBlur;
            public int m_DownSample = 9; //降采样率
            public int m_Iterations = 3; //迭代次数
            public float m_BlurSpread = 0.2f; //模糊扩散量
        }


        [SerializeField] Settings m_Settings;
        URPPass_Blur m_Pass;


        public override void Create()
        {
            m_Pass = new URPPass_Blur(m_Settings);
            m_Pass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_Pass);
        }
    }
}