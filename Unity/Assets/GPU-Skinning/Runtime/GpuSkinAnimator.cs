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
            GpuSkinMaterial.SetVector(LayerParam, new Vector4(1, 1, 1, 1));
            GpuSkinMaterial.SetVector(TimeParam, new Vector4(0, 0, 0, 0));
        }

        [Button("切换动画")]
        public void SwitchAnim()
        {
            Vector4 timeParam = GpuSkinMaterial.GetVector(TimeParam);
            Vector4 layerParam = GpuSkinMaterial.GetVector(LayerParam);

            if (Layer == GpuSkinLayer.FullBody)
            {
                int curIndex = GpuSkinUtils.GetIndex0((int)layerParam.y);
                
                layerParam.x = (int)Layer;
                layerParam.y = GpuSkinUtils.CombineIndex(TargetAnima, curIndex);
                timeParam.y = timeParam.x;
                timeParam.x = Time.time;
            }
            else
            {
                int curIndex = GpuSkinUtils.GetIndex0((int)layerParam.w);

                layerParam.z = (int)Layer;
                layerParam.w = GpuSkinUtils.CombineIndex(TargetAnima, curIndex);
                timeParam.w = timeParam.z;
                timeParam.z = Time.time;
            }
            
            GpuSkinMaterial.SetVector(LayerParam, layerParam);
            GpuSkinMaterial.SetVector(TimeParam, timeParam);
        }
    }
}