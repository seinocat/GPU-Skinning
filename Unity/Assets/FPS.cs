using System;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class FPS : MonoBehaviour
{
    
    public Text fpsText;
    private float deltaTime;

    private float m_IntervalTime = 0.5f;
    private float m_CurTime;
    private float m_fps;
    private float memoryUsageMB;

    private void Awake()
    {
        fpsText = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        m_CurTime += Time.deltaTime;
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        if (m_CurTime >= m_IntervalTime)
        {
            m_CurTime = 0;
            m_fps = 1.0f / deltaTime;
            memoryUsageMB = Profiler.GetTotalAllocatedMemoryLong()/(1024f * 1024f);
        }
        fpsText.text = $"FPS: {m_fps:F1}, Memory: {memoryUsageMB:F1}MB";
    }
}
