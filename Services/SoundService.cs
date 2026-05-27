using System.Media;
using System.Reflection;

namespace Wpf_Tomato.Services;

public class SoundService
{
    public void PlayNotificationSound()
    {
        try
        {
            // Use system sound
            SystemSounds.Asterisk.Play();
        }
        catch
        {
            // Silently fail
        }
    }

    public void PlayTickSound()
    {
        try
        {
            SystemSounds.Beep.Play();
        }
        catch
        {
            // Silently fail
        }
    }
}
