using System.Collections.Generic;
using UnityEngine;

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

        /// <summary>
        /// 查找Transform
        /// </summary>
        /// <param name="root"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Transform GetTransformByName(Transform root, string name)
        {
            if (root.name == name)
                return root;

            for (int i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name == name)
                    return child;

                if (child.childCount > 0)
                {
                    var finder = GetTransformByName(child, name);
                    if (finder != null)
                    {
                        return finder;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 收集所有子物体
        /// </summary>
        /// <param name="root"></param>
        /// <param name="childs"></param>
        public static void GetTransformChilds(Transform root, ref Dictionary<string, Transform> childs)
        {
            if (!childs.ContainsKey(root.name))
            {
                childs.Add(root.name, root);
            }
            
            for (int i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (!childs.ContainsKey(child.name))
                {
                    childs.Add(child.name, child);
                }
                
                if (child.childCount > 0)
                {
                    GetTransformChilds(child, ref childs);
                }
            }
        }
    }
}