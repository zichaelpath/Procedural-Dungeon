using UnityEngine;
using System;
using Random = System.Random;
using System.Collections.Generic;
using System.Collections;

[Serializable]
public class TileVariant
{
    [SerializeField] GameObject[] variants = new GameObject[0];

    public GameObject GetRandomTile()
    {
        Random random = SharedLevelData.Instance.Rand;
        int randomIndex = random.Next(0, variants.Length);
        return variants[randomIndex];
    }
}
