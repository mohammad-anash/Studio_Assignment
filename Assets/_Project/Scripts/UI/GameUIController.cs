using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIController : MonoBehaviour
{
    [Header("Lives UI")]
    [SerializeField] private LivesUI _livesUI;

    [Header("Game Completion UI")]
    [SerializeField] private Image _gameCompletionPanel;
    [SerializeField] private TMP_Text _resultText;
    [SerializeField] private Button _retryButton;

    [Header("Completion Colors")]
    [SerializeField] private Color _winColor = new Color(0.09f, 0.41f, 0.67f);
    [SerializeField] private Color _loseColor = new Color(0.72f, 0.13f, 0.13f);

    [Header("Level")]
    [SerializeField] private LevelManager _levelManager;

    private bool HasInvalidCompletionSetup =>
        _gameCompletionPanel == null || _resultText == null;

    private void Awake()
    {
        HideCompletionPanel();
    }

    private void Start()
    {
        if (_retryButton == null)
        {
            Debug.LogError("GameUIController: Retry button is missing.", this);
            return;
        }

        _retryButton.onClick.AddListener(RetryLevel);
    }

    public void SetMaxLives(int maxLives)
    {
        if (_livesUI != null)
            _livesUI.SetMaxLives(maxLives);
    }

    public void UpdateLives(int currentLives)
    {
        if (_livesUI != null)
            _livesUI.UpdateLivesUI(currentLives);
    }

    public void ShowWinPanel()
    {
        ShowCompletionPanel(_winColor, "YOU WON");
    }

    public void ShowLosePanel()
    {
        ShowCompletionPanel(_loseColor, "YOU LOST");
    }

    public void HideCompletionPanel()
    {
        if (_gameCompletionPanel != null)
            _gameCompletionPanel.gameObject.SetActive(false);
    }

    private void ShowCompletionPanel(Color panelColor, string message)
    {
        if (HasInvalidCompletionSetup)
        {
            Debug.LogError("GameUIController: Completion UI references are missing.", this);
            return;
        }

        _gameCompletionPanel.color = panelColor;
        _resultText.text = message;

        _gameCompletionPanel.gameObject.SetActive(true);
    }

    private void RetryLevel()
    {
        if (_levelManager == null)
        {
            Debug.LogError("GameUIController: Level Manager is missing.", this);
            return;
        }

        _levelManager.RetryLevel();
    }

    private void OnDestroy()
    {
        if (_retryButton != null)
            _retryButton.onClick.RemoveListener(RetryLevel);
    }
}