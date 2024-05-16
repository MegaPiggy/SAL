using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SALT
{
    using Utils;

    /// <summary>
    /// Change materials from your asset bundles to the in-game ones.
    /// </summary>
    public class MaterialReplacer : MonoBehaviour
    {
        public string[] materialNames;

        public void Start()
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                var materials = materialNames.Select(name => SAObjects.Get<Material>(name)).ToArray();
                if (renderer is ParticleSystemRenderer psr)
                    psr.materials = materials;
                else
                    renderer.sharedMaterials = materials;
            }
        }
    }
}
