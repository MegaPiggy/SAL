using UnityEngine;

public class ModdedTokenScript : TokenScript
{
    public new void SpawnVFX()
    {
        if (vfxPrefab != null)
            Object.Destroy(Object.Instantiate(vfxPrefab, tokenTransform.position, transform.rotation), 2);
        else
            SALT.Console.Console.LogWarning("No VFX Prefab found for Modded Token!");
    }
}