using UnityEngine;

namespace GunJump.UI
{
    public sealed class PauseView : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] CommandButton resumeButton;
        [SerializeField] CommandButton retryButton;
        [SerializeField] CommandButton menuButton;

        public CommandButton ResumeButton => resumeButton;
        public CommandButton RetryButton => retryButton;
        public CommandButton MenuButton => menuButton;

        public void Show(bool visible)
        {
            panel.SetActive(visible);
        }
    }
}
