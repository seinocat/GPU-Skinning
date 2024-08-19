using System;
using System.Collections.Generic;
using UnityEngine;

namespace Seino.GpuSkin.Runtime
{
    public class GpuInstancer : MonoBehaviour
    {
        public Mesh InstanceMesh;
        public Material InstanceMaterial;
        Matrix4x4[] matrixs = new Matrix4x4[100];
        private ComputeBuffer argsBuff;
        private ComputeBuffer m_TRSBuffer;
        private int instanceCount = 100;
        private static readonly int TRSBufferProperty = Shader.PropertyToID("_TRSBuffer");
        
        private void Awake()
        {
            Application.targetFrameRate = 500;
            
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    matrixs[i * 10 + j] = Matrix4x4.TRS(new Vector3(j, 0, i), Quaternion.identity, Vector3.one);
                }
            }
            
            List<Matrix4x4> trsMatrixs = new List<Matrix4x4>();
            //设置坐标
            for (int i = 0; i < 100; i++)
            {
                var matrix = matrixs[i];
                trsMatrixs.Add(matrix);
            }
            
            argsBuff = new ComputeBuffer(100, sizeof(uint) * 5, ComputeBufferType.IndirectArguments);
            uint[] args = new uint[5];
            args[0] = InstanceMesh.GetIndexCount(0);
            args[1] = (uint)instanceCount;
            args[2] = InstanceMesh.GetIndexStart(0);
            args[3] = InstanceMesh.GetBaseVertex(0);
            args[4] = 0;
            argsBuff.SetData(args);
            
            m_TRSBuffer?.Release();
            m_TRSBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 16);
            m_TRSBuffer.SetData(trsMatrixs);
            
            InstanceMaterial.SetBuffer(TRSBufferProperty, m_TRSBuffer);
        }

        private void Update()
        {
            Graphics.DrawMeshInstancedIndirect(InstanceMesh, 0, InstanceMaterial, new Bounds(Vector3.zero, new Vector3(100, 50, 100)), argsBuff);
        }
    }
}