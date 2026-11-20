using UnityEngine;

namespace GunJump.Gameplay
{
    [RequireComponent(typeof(Camera))]
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] CameraTuning tuning;
        [SerializeField] Transform target;

        Camera attachedCamera;
        Vector3 focus;
        Vector3 focusVelocity;

        void Awake()
        {
            attachedCamera = GetComponent<Camera>();

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

            if (attachedCamera.orthographic)
                attachedCamera.orthographicSize = CameraFraming.OrthographicSize(tuning.VisibleWidth, attachedCamera.aspect);
            else
                attachedCamera.fieldOfView = CameraFraming.VerticalFieldOfView(tuning.VisibleWidth, tuning.Distance, attachedCamera.aspect);
        }
    }
}
