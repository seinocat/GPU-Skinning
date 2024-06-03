using System.Collections.Generic;
using Seino.GpuSkin.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GPU_Skinning
{
    [CreateAssetMenu(fileName = "GpuSkinConfig", menuName = "Tools/GpuSkin/Create GpuSkin Config", order = 0)]
    public class GpuSkinConfig : ScriptableObject
    {
        [LabelText("烘焙目标")]
        public Mesh GpuSkinMesh;
        
        [LabelText("材质")]
        public Material GpuSkinMat;
        
        [LabelText("动画信息")]
        public List<GpuAnimData> GpuAnimDatas;
    }
}