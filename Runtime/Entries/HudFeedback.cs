using System;
using UnityEngine;

namespace Astra.Pulse
{
    /// <summary>HUD message. Pulse owns no HUD: the consumer registers a player that shows it.</summary>
    [Serializable]
    public sealed class HudFeedback : IFeedbackEntry
    {
        [SerializeField] private string _message;

        public HudFeedback()
        {
        }

        public HudFeedback(string message)
        {
            _message = message;
        }

        public string Message => _message;
    }
}
