using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

public class MemoryStatsScript : MonoBehaviour
{
    string statsText;

    void Awake()
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = true;
    }

    public static string GetStats()
    {
        long num1 = UnityEngine.Profiling.Profiler.GetAllocatedMemoryForGraphicsDriver() / 1024 / 1024;
        long num2 = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024;
        long num3 = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() / 1024 / 1024;
        long num4 = UnityEngine.Profiling.Profiler.GetTotalUnusedReservedMemoryLong() / 1024 / 1024;
        long num5 = UnityEngine.Profiling.Profiler.GetTempAllocatorSize() / 1024 / 1024;
        var sb = new StringBuilder(500);
        sb.AppendLine($"Allocated Memory For GfxDriver: {num1}");
        sb.AppendLine($"Total Allocated Memory: {num2}");
        sb.AppendLine($"Total Reserved Memory: {num3}");
        sb.AppendLine($"Total Unused Reserved Memory: {num4}");
        sb.AppendLine($"Temp Allocator Size: {num5}");
        return sb.ToString();
    }
    
    void Update()
    {
        statsText = GetStats();
    }

    void OnGUI()
    {
        GUI.TextArea(new Rect(10, 30, 250, (50f/3)*5), statsText);
    }
}