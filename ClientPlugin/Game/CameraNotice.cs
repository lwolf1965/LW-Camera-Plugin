using System;
using Keen.Game2.Client.UI.InGame;
using Keen.Game2.Client.UI.Shared.SystemNotification;
using Keen.VRage.Core.Game.Components;
using Keen.VRage.Core.Game.Systems;

namespace ClientPlugin.Game;

/// <summary>
/// Shows which dock camera is active, as the game's own small toast in the HUD ("Cam: Back", "2/6"). One toast is
/// reused: switching cameras quickly updates its text and restarts its timer instead of stacking new ones.
///
/// Game API: SessionInGameUISessionComponent (the client session's InGameUI) .DisplayNotification(NotificationViewModel),
/// the same call the game uses for safe-zone messages. It returns null while the in-game menu is open.
/// </summary>
internal static class CameraNotice
{
    private static readonly TimeSpan ShowFor = TimeSpan.FromSeconds(3);
    private static NotificationViewModel current;

    public static void Show(Session session, string cameraName, int number, int count)
    {
        var title = "Cam: " + cameraName;
        var content = $"{number}/{count}";
        try
        {
            if (current != null)
            {
                current.Title = title;
                current.Content = content;
                current.RestartTimer();
                return;
            }

            var ui = session.TryGet<SessionInGameUISessionComponent>();
            if (ui == null) return;

            var notice = new NotificationViewModel(title, content, ShowFor);
            notice.OnDisposed += _ =>
            {
                if (ReferenceEquals(current, notice)) current = null;
            };
            if (ui.DisplayNotification(notice) != null) current = notice;
        }
        catch (Exception e)
        {
            current = null;
            PluginLog.WarnOnce("notice", "Camera name toast failed: " + e.Message);
        }
    }

    /// <summary>A new world: the old toast belongs to the old UI.</summary>
    public static void Forget() => current = null;
}
