using System.Collections.Generic;
using ianco99.ToolBox.Services;

namespace GunJump.Core
{
    // Un ContentBundle por nombre para toda la sesión: la tienda y el arma piden acá el mismo,
    // porque Unity no deja abrir dos veces el mismo AssetBundle. Lo que ya se cargó queda listo
    // para la próxima vez; por ejemplo, al reintentar un nivel el arma no vuelve a cargar nada.
    public sealed class ContentLibrary : IService
    {
        readonly Dictionary<string, ContentBundle> bundles = new Dictionary<string, ContentBundle>();

        public bool IsPersistance => true;

        public ContentBundle Get(string bundleName)
        {
            if (!bundles.TryGetValue(bundleName, out ContentBundle bundle))
            {
                bundle = new ContentBundle(bundleName);
                bundles.Add(bundleName, bundle);
            }

            return bundle;
        }
    }
}
