using System;
using Sirenix.OdinInspector;

namespace GPU_Skinning.Runtime.Data
{
    [Serializable]
    public enum AnimFrame
    {
        [LabelText("12帧")]
        Frame12 = 12,
        
        [LabelText("15帧")]
        Frame15 = 15,
        
        [LabelText("24帧")]
        Frame24 = 24,
        
        [LabelText("30帧")]
        Frame30 = 30,
        
        [LabelText("48帧")]
        Frame48 = 48,
        
        [LabelText("60帧")]
        Frame60 = 60,
        
        [LabelText("120帧")]
        Frame120 = 120,
    }
}