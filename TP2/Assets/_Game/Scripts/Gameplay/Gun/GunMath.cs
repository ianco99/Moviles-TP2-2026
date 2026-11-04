using UnityEngine;

namespace GunJump.Gameplay
{
    public static class GunMath
    {
        // Componente X del cañón por debajo de la cual se lo considera vertical (unos 6 grados).
        const float VerticalDeadZone = 0.1f;

        // El retroceso empuja en sentido contrario al cañón.
        public static Vector2 ApplyRecoil(Vector2 velocity, Vector2 barrelDirection, float recoil, float carryOver, float maxSpeed)
        {
            Vector2 result = velocity * carryOver - barrelDirection.normalized * recoil;
            return Vector2.ClampMagnitude(result, maxSpeed);
        }

        // Apoyada en el piso el arma no puede girar: el cañón choca contra el suelo.
        // Por eso un disparo desde el piso siempre la deja subiendo al menos a esta velocidad.
        public static Vector2 EnsureLift(Vector2 velocity, float lift, float maxSpeed)
        {
            velocity.y = Mathf.Max(velocity.y, lift);
            return Vector2.ClampMagnitude(velocity, maxSpeed);
        }

        // El arma no gira sola: el giro sale del retroceso, que siempre patea el cañón hacia arriba.
        // Con el arma derecha (cañón adelante, mango abajo) eso es un sentido; dada vuelta
        // (cañón atrás, mango arriba) es el contrario, y el cañón vuelve hacia adelante.
        // Con el cañón casi vertical no hay un "arriba" claro: sigue girando para donde venía.
        public static float RecoilSpinDirection(float barrelX, float currentSpin)
        {
            if (Mathf.Abs(barrelX) > VerticalDeadZone)
                return Mathf.Sign(barrelX);

            return currentSpin < 0f ? -1f : 1f;
        }

        public static float DampSpin(float spin, float damping, float deltaTime)
        {
            return Mathf.MoveTowards(spin, 0f, damping * deltaTime);
        }

        // Escala y posición que dejan un modelo con estos límites (medidos con el modelo en el origen y escala 1)
        // midiendo length sobre X y centrado en el origen. Sin largo sobre X no hay qué escalar.
        public static void FitToLength(Bounds bounds, float length, out float scale, out Vector3 position)
        {
            float size = bounds.size.x;
            scale = size > 0f ? length / size : 1f;
            position = -bounds.center * scale;
        }

        // Escala local que hace que un hijo se vea a su tamaño propio debajo de un padre escalado.
        public static Vector3 UndoParentScale(Vector3 childScale, Vector3 parentScale)
        {
            return new Vector3(
                parentScale.x != 0f ? childScale.x / parentScale.x : childScale.x,
                parentScale.y != 0f ? childScale.y / parentScale.y : childScale.y,
                parentScale.z != 0f ? childScale.z / parentScale.z : childScale.z);
        }
    }
}
