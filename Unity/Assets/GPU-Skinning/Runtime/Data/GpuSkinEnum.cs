using System;
using Sirenix.OdinInspector;

namespace Seino.GpuSkin.Runtime
{
    [Serializable]
    public enum AnimFrame
    {
        [LabelText("自动检测")]
        FrameAuto = 0,
        
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

    [Serializable]
    public enum TextureWidth
    {
        [LabelText("自动计算")]
        Auto = 0,
        
        [LabelText("128")]
        Width128 = 128,
        
        [LabelText("256")]
        Width256 = 256,
        
        [LabelText("512")]
        Width512 = 512,
        
        [LabelText("1024")]
        Width1024 = 1024,
        
        [LabelText("2048")]
        Width2048 = 2048,
    }
}