using System.Collections.Generic;
using GPU_Skinning.Runtime.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Seino.GpuSkin.Runtime
{
    [CreateAssetMenu(fileName = "GpuSkinConfig", menuName = "Tools/GpuSkin/Create GpuSkin Config")]
    public class GpuSkinConfig : ScriptableObject
    {
        [LabelText("动画帧率")]
        public AnimFrame FrameRate = AnimFrame.Frame30;
        
        [LabelText("烘焙模型")]
        public GameObject BakeTarget;
        
        [LabelText("Material")]
        public Material BakeMaterial;
        
        [LabelText("Shader")]
        public Shader GpuSkinShader;
        
        [LabelText("漫反射参数")]
        public string MainTexProperty = "_MainTex";
        
        [LabelText("漫反射贴图")]
        public Texture MainTex;
        
        [LabelText("切片列表")]
        public List<GpuAnimData> AnimDatas;
    }
}