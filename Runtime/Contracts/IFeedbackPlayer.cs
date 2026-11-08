namespace Astra.Pulse
{
    /// <summary>
    /// Plays one entry type (audio, shake, HUD, VFX or a pack's own entry). Registered with
    /// <see cref="IFeedbackService.RegisterPlayer{TEntry}"/>; the service only hands it entries of type
    /// <typeparamref name="TEntry"/>, together with the request that carries them for its spatial context.
    /// </summary>
    public interface IFeedbackPlayer<in TEntry> where TEntry : IFeedbackEntry
    {
        FeedbackResult Play(TEntry entry, FeedbackRequest request);
    }
}
