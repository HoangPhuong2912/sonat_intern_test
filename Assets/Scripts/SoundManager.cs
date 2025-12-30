using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public Slider volSlider;
    public Button toggleButton;
    public Image buttonIcon;  // The speaker icon image

    public Sprite soundOnIcon;   // Assign in inspector
    public Sprite soundOffIcon;  // Assign in inspector

    private float savedVolume = 1f;
    private bool isMuted = false;

    void Start()
    {
        // Load saved volume or use default
        savedVolume = PlayerPrefs.GetFloat("Volume", 1f);
        isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;

        if (isMuted)
        {
            AudioListener.volume = 0;
            volSlider.value = 0;
            UpdateButtonIcon();
        }
        else
        {
            AudioListener.volume = savedVolume;
            volSlider.value = savedVolume;
            UpdateButtonIcon();
        }
    }

    public void ChangeVolume()
    {
        AudioListener.volume = volSlider.value;
        savedVolume = volSlider.value;

        // Auto-unmute if user moves slider above 0
        if (volSlider.value > 0 && isMuted)
        {
            isMuted = false;
            UpdateButtonIcon();
        }
        // Auto-mute if user drags slider to 0
        else if (volSlider.value == 0 && !isMuted)
        {
            isMuted = true;
            UpdateButtonIcon();
        }

        PlayerPrefs.SetFloat("Volume", savedVolume);
        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;

        if (isMuted)
        {
            // Save current volume before muting
            if (volSlider.value > 0)
            {
                savedVolume = volSlider.value;
            }
            AudioListener.volume = 0;
            volSlider.value = 0;
        }
        else
        {
            // Restore saved volume
            AudioListener.volume = savedVolume;
            volSlider.value = savedVolume;
        }

        UpdateButtonIcon();
        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
    }

    private void UpdateButtonIcon()
    {
        if (buttonIcon != null)
        {
            buttonIcon.sprite = isMuted ? soundOffIcon : soundOnIcon;
        }
    }
}