using ShapeConnections.Game.Loop;
using ShapeConnections.Simulation.GameLoop;
using UnityEngine;
using UnityEngine.UI;

namespace ShapeConnections.Game.UI
{
    /// <summary>
    /// Subscribes to <see cref="LevelRunner.LevelCompleted"/> and shows a
    /// pass/fail overlay. Communication via UnityEvent keeps this UI loosely
    /// coupled — a future scoring panel, sound-only feedback, or headless logger
    /// could subscribe alongside (or instead of) this one.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WinFailPanel : MonoBehaviour
    {
        [SerializeField] private LevelRunner runner;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text messageText;
        [SerializeField] private Image background;
        [SerializeField] private Color winColor  = new Color(0.2f, 0.7f, 0.3f, 0.85f);
        [SerializeField] private Color failColor = new Color(0.8f, 0.25f, 0.25f, 0.85f);

        private void Awake()
        {
            // Skip edit-mode init — scaffolder's AddComponent fires Awake before serialized fields are set.
            if (!Application.isPlaying) return;
            if (panelRoot != null) panelRoot.SetActive(false);
        }

#if UNITY_EDITOR
        /// <summary>Editor-only direct-assignment hook used by SceneScaffolder.</summary>
        public void EditorConfigure(LevelRunner runner, GameObject panelRoot, Text messageText, Image background)
        {
            this.runner = runner;
            this.panelRoot = panelRoot;
            this.messageText = messageText;
            this.background = background;
        }
#endif

        private void OnEnable()
        {
            if (runner != null) runner.LevelCompleted.AddListener(HandleResult);
        }

        private void OnDisable()
        {
            if (runner != null) runner.LevelCompleted.RemoveListener(HandleResult);
        }

        private void HandleResult(LoopResult result)
        {
            if (panelRoot == null || messageText == null) return;
            panelRoot.SetActive(true);

            if (result.HasCycle)
            {
                messageText.text = "Cycle detected — re-wire and try again.";
                if (background != null) background.color = failColor;
            }
            else if (result.Win)
            {
                messageText.text = "Solved!";
                if (background != null) background.color = winColor;
            }
            else if (result.UnwiredOutputSockets.Count > 0)
            {
                messageText.text = "Some outputs aren't wired yet.";
                if (background != null) background.color = failColor;
            }
            else
            {
                messageText.text = "Not quite — outputs don't match the target yet.";
                if (background != null) background.color = failColor;
            }
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }
    }
}
