using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


public class NewBehaviourScript : MonoBehaviour
{

    public float updateRate = 1.0f; // 1초마다 갱신
    public float timer = 2.0f;
    public int frameCount = 0;

    public float fps = 0.0f;
    public int batches = 0;
    public long totalVertices = 0;
    public long totalVertices_max = 0;
    public long totalVertices_min = long.MaxValue;

    void Update()
    {
        if (timer < updateRate)
        {
            timer += Time.unscaledDeltaTime;

            // GPU 렌더링 통계

            frameCount++;
            batches += UnityStats.batches;
            totalVertices += UnityStats.vertices;
            if (totalVertices_max < UnityStats.vertices)
                totalVertices_max = UnityStats.vertices;
            if (totalVertices_min > UnityStats.vertices)
                totalVertices_min = UnityStats.vertices;
        }
        else if (frameCount != 0)
        {
            print($"frame : {frameCount / timer}, batches : {batches / frameCount}, totalVertices : {totalVertices / frameCount}");
            frameCount = 0;
        }
    }

}
