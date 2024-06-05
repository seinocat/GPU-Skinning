namespace Seino.GpuSkin.Runtime
{
    public static class GpuSkinUtils
    {
        /// <summary>
        /// 组合，低8位index, 高8位layer
        /// </summary>
        /// <param name="index"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static int CombineIndexAndLayer(int index, int layer)
        {
            index &= 0xff;
            layer &= 0xff;
            
            int combine = (layer << 8) | index;
            return combine;
        }

        public static int GetIndex(int combine)
        {
            int index = combine & 0xff;
            return index;
        }

        public static int GetLayer(int combine)
        {
            int layer = (combine >> 8) & 0xff;
            return layer;
        }
    }
}