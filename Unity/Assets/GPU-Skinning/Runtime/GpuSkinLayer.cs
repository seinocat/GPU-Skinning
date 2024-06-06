namespace Seino.GpuSkin.Runtime
{
    public enum GpuSkinLayer
    {
        Base = 1 << 0,
        
        TopHalfBody = 2 << 1,
        
        BottomHalfBody = 2 << 2,
        
        Hand = 2 << 3,
        
        LeftHand = 2 << 4,
        
        RightHand = 2 << 5,
    }
}