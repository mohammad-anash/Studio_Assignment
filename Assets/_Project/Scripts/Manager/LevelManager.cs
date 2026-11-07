using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Levels")]
    [SerializeField] private LevelData[] _levels;
    [SerializeField] private int _startLevelIndex;

    [Header("References")]
    [SerializeField] private Transform _levelRoot;
    [SerializeField] private GameManager _gameManager;

    private GameObject _currentLayout;
    private int _currentIndex;

    private bool HasInvalidSetup =>
        _levels == null ||
        _levels.Length == 0 ||
        _levelRoot == null ||
        _gameManager == null;

    private void Start()
    {
        if (HasInvalidSetup)
        {
            Debug.LogError("LevelManager: Assign the levels, level root, and game manager.", this);
            return;
        }

        LoadLevel(_startLevelIndex);

    }

    public void LoadLevel(int index)
    {
        if (HasInvalidSetup)
            return;

        if (index < 0 || index >= _levels.Length || _levels[index] == null)
        {
            Debug.LogError($"LevelManager: Level {index} does not exist.", this);
            return;
        }

        LevelData levelData = _levels[index];

        if (levelData.LayoutPrefab == null)
        {
            Debug.LogError($"LevelManager: {levelData.name} has no layout prefab.", this);
            return;
        }

        ClearCurrentLayout();

        _currentIndex = index;
        _currentLayout = Instantiate(levelData.LayoutPrefab, _levelRoot);


        ArrowPairController[] pairs = _currentLayout.GetComponentsInChildren<ArrowPairController>();

        _gameManager.StartLevel(levelData, pairs);
    }

    public void RetryLevel()
    {
        LoadLevel(_currentIndex);
    }
    private void ClearCurrentLayout()
    {
        if (_currentLayout == null)
            return;

        _currentLayout.SetActive(false);
        Destroy(_currentLayout);
        _currentLayout = null;
    }
}
