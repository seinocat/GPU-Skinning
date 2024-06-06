using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Seino.GpuSkin.Runtime
{
    [Serializable]
    public class GpuSkinShaderTexData
    {
        [LabelText("贴图名称", SdfIconType.BodyText)]
        public string PropertyName;
        
        [LabelText("贴图", SdfIconType.Image)]
        public Texture Texture;
    }
}