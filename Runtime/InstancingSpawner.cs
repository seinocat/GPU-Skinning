using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GPU_Skinning.Runtime
{
    public class BatchData 
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        
        public Matrix4x4 Matrix => Matrix4x4.TRS(Position, Rotation, Scale);

        public BatchData(Vector3 pos, Quaternion rot, Vector3 scale)
        {
            this.Position = pos;
            this.Rotation = rot;
            this.Scale = scale;
        }
    }

    public class Batch
    {
        public List<Matrix4x4> Matrixs = new();
        public MaterialPropertyBlock MatPb = new();
    }
    
    public class InstancingSpawner : MonoBehaviour
    {
        public Mesh Mesh;
        public Material Material;
        private int m_MaxBatchCount = 1023;
        private int m_CurCount;
        private List<Batch> Batchs = new();
        private List<BatchData> BatchDataList = new();
        private bool m_Enable;

        public void Generate(int row, int col)
        {
            m_Enable = true;
            m_CurCount = 0;
            BatchDataList.Clear();
            Batchs.Clear();
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    var position = new Vector3(i, 0f, j);
                    Quaternion rotation = Quaternion.identity;
                    var scale = Vector3.one;
                    
                    BatchData data = new BatchData(position, rotation, scale);
                    BatchDataList.Add(data);
                    m_CurCount++;
                    if (m_CurCount >= m_MaxBatchCount)
                    {
                        Batch batch = new Batch();
                        batch.Matrixs = new List<Matrix4x4>(BatchDataList.Select(x=>x.Matrix));
                        Batchs.Add(batch);
                        BatchDataList.Clear();
                        m_CurCount = 0;
                    }
                }
            }
            
            Batch lastBatch = new Batch();
            lastBatch.Matrixs = new List<Matrix4x4>(BatchDataList.Select(x=>x.Matrix));
            Batchs.Add(lastBatch);
        }

        private void Update()
        {
            if (!m_Enable) return;

            foreach (var batch in Batchs)
            {
                Graphics.DrawMeshInstanced(Mesh, 0, Material, batch.Matrixs, batch.MatPb);
            }
        }

        public void Stop()
        {
            m_Enable = false;
        }
    }
}