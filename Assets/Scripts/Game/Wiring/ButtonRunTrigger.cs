using System;
using ShapeConnections.Simulation.GameLoop;
using UnityEngine;
using UnityEngine.UI;

namespace ShapeConnections.Game.Wiring
{
    /// <summary>
    /// IRunTrigger backed by a UI Button. The simplest implementation; alternates
    /// could include a hotkey, an auto-runner that fires N seconds after the last
    /// wire change, or an AI bot.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class ButtonRunTrigger : MonoBehaviour, IRunTrigger
    {
        private Button _button;

        public event Action RunRequested;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() => RunRequested?.Invoke());
        }

        public void SetInteractable(bool interactable)
        {
            if (_button != null) _button.interactable = interactable;
        }
    }
}
