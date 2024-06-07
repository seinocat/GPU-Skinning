using Sirenix.OdinInspector;

namespace Seino.GpuSkin.Runtime
{
    public enum GpuSkinLayer
    {
        [LabelText("基本层")]
        Base = 1 << 0,
        
        [LabelText("上半身")]
        UpperBody = 1 << 1,
        
        [LabelText("下半身")]
        LowerBody = 1 << 2,
        
        [LabelText("头")]
        Head = 1 << 3,
        
        [LabelText("双手")]
        Hands = 1 << 4,
        
        [LabelText("左手")]
        LeftHand = 1 << 5,
        
        [LabelText("右手")]
        RightHand = 1 << 6,
        
        [LabelText("左腿")]
        LeftLeg = 1 << 7,
        
        [LabelText("右腿")]
        RightLeg = 1 << 8,
        
        [LabelText("骨盆")]
        Pelvis = 1 << 9,
    }
}