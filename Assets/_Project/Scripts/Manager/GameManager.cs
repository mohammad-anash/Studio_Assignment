using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("GameManager Settings")]
    [SerializeField] private GameUIController _gameUIController;

    private ArrowPairController[] _arrowPairs;
    private ArrowMovement _lastHintedArrow;
    private Coroutine _currentCoroutine;

    private int _currentLives;
    private bool _isHandlingCollision;
    private bool _isGameOver;

    public int CurrentLives => _currentLives;
    public bool IsGameOver => _isGameOver;

    private bool HasInvalidSetup => _gameUIController == null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (HasInvalidSetup)
            Debug.LogError("GameManager: Game UI Controller is missing.", this);
    }

    public void StartLevel(LevelData levelData, ArrowPairController[] arrowPairs)
    {
        if (HasInvalidSetup || levelData == null || arrowPairs == null)
            return;

        _arrowPairs = arrowPairs;
        _currentLives = levelData.StartLives;

        _isGameOver = false;
        _isHandlingCollision = false;
        _lastHintedArrow = null;

        _gameUIController.HideCompletionPanel();
        _gameUIController.UpdateLives(_currentLives);
    }

    private void LoseLife()
    {
        if (_currentLives <= 0 || _isGameOver)
            return;

        _currentLives--;

        _gameUIController.UpdateLives(_currentLives);

        if (_currentLives == 0)
            ShowLosePanel();
    }

    public void HandleArrowCollision()
    {
        if (_isHandlingCollision || _isGameOver)
            return;

        _isHandlingCollision = true;

        LoseLife();
        ClearHintHighlight();

        _currentCoroutine = StartCoroutine(ResetCollisionLock());
    }

    private IEnumerator ResetCollisionLock()
    {
        yield return new WaitForFixedUpdate();

        _isHandlingCollision = false;
        _currentCoroutine = null;
    }

    public void CheckLevelCompletion()
    {
        if (_isGameOver || _arrowPairs == null)
            return;

        foreach (ArrowPairController pair in _arrowPairs)
        {
            if (pair != null && !pair.IsCompleted)
                return;
        }

        ShowWinPanel();
    }

    private void ShowWinPanel()
    {
        if (_isGameOver)
            return;

        _isGameOver = true;

        ClearHintHighlight();
        _gameUIController.ShowWinPanel();
    }

    private void ShowLosePanel()
    {
        if (_isGameOver)
            return;

        _isGameOver = true;

        ClearHintHighlight();
        _gameUIController.ShowLosePanel();
    }

    private List<ArrowMovement> GetSafeArrows()
    {
        List<ArrowMovement> safeArrows = new List<ArrowMovement>();

        if (_arrowPairs == null)
            return safeArrows;

        foreach (ArrowPairController pair in _arrowPairs)
        {
            if (pair == null || pair.IsCompleted)
                continue;

            ArrowMovement safeArrow = pair.GetSafeArrow();

            if (safeArrow != null)
                safeArrows.Add(safeArrow);
        }

        return safeArrows;
    }

    public void ClearHintHighlight()
    {
        if (_lastHintedArrow == null)
            return;

        _lastHintedArrow.ResetArrowAppearance();

        ArrowPairController previousPair = _lastHintedArrow.PairController;

        if (previousPair != null)
            previousPair.ResetLineHighlight();

        _lastHintedArrow = null;
    }

    public void OnHintButtonClicked()
    {
        if (_isGameOver)
            return;

        ClearHintHighlight();

        List<ArrowMovement> safeArrows = GetSafeArrows();

        if (safeArrows.Count == 0)
            return;

        ArrowMovement selectedArrow = safeArrows[Random.Range(0, safeArrows.Count)];
        ArrowPairController selectedLine = selectedArrow.PairController;

        if (selectedLine == null)
            return;

        selectedLine.HighlightLine();
        selectedArrow.HighlightArrow();

        _lastHintedArrow = selectedArrow;
    }

    private void OnDestroy()
    {
        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);

        if (Instance == this)
            Instance = null;
    }
}