using UnityEngine;
using System;
using Random = System.Random;
using Unity.VisualScripting;

[ExecuteAlways]
[DisallowMultipleComponent]
public class SharedLevelData : MonoBehaviour
{
    public static SharedLevelData Instance { get; private set; }
    [SerializeField] int scale = 1;
    [SerializeField] int seed = Environment.TickCount;
    Random random;
    public int Scale => scale;
    public int Seed => seed;
    public Random Rand => random;
    [ContextMenu("Set Random Seed")]
    public void GenerateSeed()
    {
        seed = Environment.TickCount;
        random = new Random(seed);
    }

    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            enabled = false;
            Debug.LogWarning("Duplicated SharedLevelData instance found. Disabling the new one.", this);
        }
        Debug.Log(Instance.GetInstanceID());
        random = new Random(seed);
    }

    public void ResetRandom()
    {
        random = new Random(seed);
    }
}
