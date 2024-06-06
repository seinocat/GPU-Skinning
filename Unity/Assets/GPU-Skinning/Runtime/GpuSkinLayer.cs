namespace Seino.GpuSkin.Runtime
{
    public enum GpuSkinLayer
    {
        Base = 1 << 0,
        
        UpperBody = 1 << 1,
        
        LowerBody = 1 << 2,
        
        Head = 1 << 3,
        
        Hands = 1 << 4,
        
        LeftHand = 1 << 5,
        
        RightHand = 1 << 6,
    }
}