using System;
using Sirenix.OdinInspector;

namespace GPU_Skinning.Runtime.Data
{
    [Serializable]
    public enum AnimFrame
    {
        [LabelText("15帧")]
        Frame15 = 15,
        [LabelText("30帧")]
        Frame30 = 30,
        [LabelText("60帧")]
        Frame60 = 60,
    }
}