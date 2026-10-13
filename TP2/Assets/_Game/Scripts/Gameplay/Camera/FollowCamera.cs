using UnityEngine;

namespace GunJump.Gameplay
{
    // Cámara lateral: mira hacia +Z desde z = -distance y sigue al arma en X e Y.
    [RequireComponent(typeof(Camera))]
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] CameraTuning tuning;
        [SerializeField] Transform target;

        Camera cam;
        Vector3 velocity;
        float framedAspect;

        void Awake()
        {
            cam = GetComponent<Camera>();
            ApplyFraming();

            if (target != null)
                transform.position = GoalPosition();
        }

        void LateUpdate()
        {
            if (!Mathf.Approximately(cam.aspect, framedAspect))
                ApplyFraming();

            if (target == null)
                return;

            transform.position = Vector3.SmoothDamp(transform.position, GoalPosition(), ref velocity, tuning.SmoothTime);
        }

        Vector3 GoalPosition()
        {
            Vector3 position = target.position;
            return new Vector3(position.x + tuning.LookAhead, position.y + tuning.VerticalOffset, -tuning.Distance);
        }

        void ApplyFraming()
        {
            framedAspect = cam.aspect;

            if (cam.orthographic)
                cam.orthographicSize = CameraFraming.OrthographicSize(tuning.VisibleWidth, framedAspect);
            else
                cam.fieldOfView = CameraFraming.VerticalFieldOfView(tuning.VisibleWidth, tuning.Distance, framedAspect);
        }
    }
}
