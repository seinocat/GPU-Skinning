using Sirenix.OdinInspector;
using UnityEngine;

namespace Seino.GpuSkin.Runtime
{
    public class GpuSkinAnimator : MonoBehaviour
    {
        public MeshRenderer Renderer;
        public Material GpuSkinMaterial;
        public int TargetAnima = 1;
        public GpuSkinLayer Layer = GpuSkinLayer.FullBody;
        
        private static readonly int TimeParam = Shader.PropertyToID("_TimeParam");
        private static readonly int LayerParam = Shader.PropertyToID("_LayerParam");

        private void Awake()
        {
            GpuSkinMaterial = Renderer.sharedMaterial;
        }

        [Button("切换动画")]
        public void SwitchAnim()
        {
            Vector4 timeParam = GpuSkinMaterial.GetVector(TimeParam);
            Vector4 layerParam = GpuSkinMaterial.GetVector(TimeParam);

            if (Layer == GpuSkinLayer.FullBody)
            {
                timeParam.y = timeParam.x;
                timeParam.x = Time.time;
                layerParam.y = layerParam.x;
                layerParam.x = TargetAnima;
            }
            else
            {
                timeParam.w = timeParam.z;
                timeParam.z = Time.time;
                layerParam.w = layerParam.z;
                layerParam.z = TargetAnima;
            }
            
            GpuSkinMaterial.SetVector(LayerParam, layerParam);
            GpuSkinMaterial.SetVector(TimeParam, timeParam);
        }
    }
}