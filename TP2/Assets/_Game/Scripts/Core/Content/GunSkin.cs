using UnityEngine;

namespace GunJump.Core
{
    // Viste un arma con una skin: el material de la skin reemplaza todos los materiales de cada renderer.
    // Lo usan el arma en los niveles (GunCustomization) y la vista previa del menú (GunPreviewController).
    public static class GunSkin
    {
        // Sin skin no toca nada: el arma queda con sus propios materiales.
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
