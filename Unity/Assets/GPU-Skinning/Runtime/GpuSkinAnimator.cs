using Sirenix.OdinInspector;
using UnityEngine;

namespace Seino.GpuSkin.Runtime
{
    public class GpuSkinAnimator : MonoBehaviour
    {
        public MeshRenderer Renderer;
        public Material GpuSkinMaterial;
        public int TargetAnima = 2;
        public GpuSkinLayer Layer;
        
        private static readonly int BlendParam = Shader.PropertyToID("_BlendParam");

        private void Awake()
        {
            GpuSkinMaterial = Renderer.sharedMaterial;
        }

        [Button("切换动画")]
        public void SwitchAnim()
        {
            Vector4 blendAnim = GpuSkinMaterial.GetVector(BlendParam);
            if ((int)blendAnim.x != TargetAnima)
            {
                blendAnim.z = blendAnim.x; // 上一动画
                blendAnim.x = GpuSkinUtils.CombineIndexAndLayer(TargetAnima, (int)Layer); // 当前动画
                blendAnim.w = blendAnim.y; // 上一动画的播放时间
                blendAnim.y = Time.time; // 当前动画的播放时间
                GpuSkinMaterial.SetVector(BlendParam, blendAnim);
            }
        }

        
    }
}