using UnityEngine;

namespace GunJump.Core
{
    public static class GunSkin
    {
        public static void Apply(Renderer[] renderers, Material skin)
        {
            if (skin == null || renderers == null)
                return;

            foreach (Renderer target in renderers)
            {
                if (target == null)
                    continue;

                Material[] materials = target.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                    materials[i] = skin;

                target.sharedMaterials = materials;
            }
        }
    }
}
