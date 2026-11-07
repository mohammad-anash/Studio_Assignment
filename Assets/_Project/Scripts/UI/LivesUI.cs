using UnityEngine;
using UnityEngine.UI;

public class LivesUI : MonoBehaviour
{
    [Header("Lives UI Settings")]
    [SerializeField] private Image[] _heartImages;

    [Header("Heart Colors")]
    [SerializeField] private Color _activeColor = Color.red;
    [SerializeField] private Color _inactiveColor = Color.white;

    private int _maxLives;

    private bool HasInvalidSetup =>
        _heartImages == null || _heartImages.Length == 0;

    private void Awake()
    {
        if (HasInvalidSetup)
        {
            Debug.LogError("LivesUI: Heart Images are missing.", this);
            return;
        }

        _maxLives = _heartImages.Length;
    }

    // Shows one heart for every life the level starts with
    public void SetMaxLives(int maxLives)
    {
        if (HasInvalidSetup)
            return;

        if (maxLives > _heartImages.Length)
        {
            Debug.LogWarning(
                "LivesUI: The level has more lives than hearts. Add more hearts.",
                this
            );
        }

        _maxLives = Mathf.Clamp(maxLives, 0, _heartImages.Length);

        for (int i = 0; i < _heartImages.Length; i++)
        {
            if (_heartImages[i] != null)
                _heartImages[i].gameObject.SetActive(i < _maxLives);
        }
    }

    public void UpdateLivesUI(int currentLives)
    {
        if (HasInvalidSetup)
            return;

        currentLives = Mathf.Clamp(currentLives, 0, _maxLives);

        for (int i = 0; i < _heartImages.Length; i++)
        {
            if (_heartImages[i] == null)
                continue;

            _heartImages[i].color = i < currentLives
                ? _activeColor
                : _inactiveColor;
        }
    }
}