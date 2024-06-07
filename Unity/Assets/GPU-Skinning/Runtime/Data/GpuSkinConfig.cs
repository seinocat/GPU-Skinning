using System.Collections.Generic;
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
        
        [LabelText("动画贴图")]
        public Object AnimTex;
        
        [LabelText("Mesh")]
        public Mesh BakeMesh;
        
        [LabelText("Material")]
        public Material BakeMaterial;
        
        [LabelText("Shader")]
        public Shader GpuSkinShader;

        [LabelText("Shader贴图参数")]
        public List<GpuSkinShaderTexData> ShaderTexDatas;

        [LabelText("切片列表")]
        public List<GpuAnimData> AnimDatas;
        
        [LabelText("层级设置")]
        public List<BoneLayerData> BoneLayers;
    }
}