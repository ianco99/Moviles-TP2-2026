using UnityEngine;

namespace GunJump.Gameplay
{
    // Sigue al arma desde atrás y al costado, mirando hacia adelante. Ángulo y distancia salen de CameraTuning
    // y se leen en cada frame, así que se pueden ajustar con el juego en Play.
    [RequireComponent(typeof(Camera))]
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] CameraTuning tuning;
        [SerializeField] Transform target;

        Camera cam;
        Vector3 focus;
        Vector3 focusVelocity;

        void Awake()
        {
            cam = GetComponent<Camera>();

            if (target != null)
                focus = GoalFocus();

            Apply();
        }

        void LateUpdate()
        {
            if (target != null)
                focus = Vector3.SmoothDamp(focus, GoalFocus(), ref focusVelocity, tuning.SmoothTime);

            Apply();
        }

        Vector3 GoalFocus()
        {
            Vector3 position = target.position;
            return new Vector3(position.x + tuning.LookAhead, position.y + tuning.VerticalOffset, position.z);
        }

        void Apply()
        {
            Quaternion rotation = CameraFraming.Rotation(tuning.Yaw, tuning.Pitch);
            transform.SetPositionAndRotation(CameraFraming.Position(focus, rotation, tuning.Distance), rotation);

            if (cam.orthographic)
                cam.orthographicSize = CameraFraming.OrthographicSize(tuning.VisibleWidth, cam.aspect);
            else
                cam.fieldOfView = CameraFraming.VerticalFieldOfView(tuning.VisibleWidth, tuning.Distance, cam.aspect);
        }
    }
}
