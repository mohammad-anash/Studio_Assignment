using UnityEngine;
using UnityEngine.Splines;

public class ArrowPairController : MonoBehaviour
{
    [Header("Arrow Settings")]
    [SerializeField] private GameObject _arrowHeadPrefab;
    [SerializeField] private SplineLineRenderer _lineRenderer;

    private GameObject _startingArrowHead;
    private GameObject _endingArrowHead;

    private ArrowMovement _startingArrowMovement;
    private ArrowMovement _endingArrowMovement;

    private ArrowSafetyChecker _startingSafetyChecker;
    private ArrowSafetyChecker _endingSafetyChecker;

    private bool _isCompleted;
    private bool _isInitialized;

    public bool IsCompleted => _isCompleted;

    public SplineContainer SplineContainer => _lineRenderer != null ? _lineRenderer.SplineContainer : null;

    public float LineLength => _lineRenderer != null ? _lineRenderer.Length : 0f;

    private bool HasInvalidSetup => _arrowHeadPrefab == null || _lineRenderer == null || _lineRenderer.SplineContainer == null;

    private void Awake()
    {
        if (HasInvalidSetup)
        {
            Debug.LogError(
                "ArrowPairController: Required references are missing.",
                this
            );
        }
    }

    private void Start()
    {
        if (HasInvalidSetup)
            return;

        CreateArrowHeads();
    }

    private void CreateArrowHeads()
    {
        Vector3 startingPosition = SplineContainer.EvaluatePosition(0f);
        Vector3 endingPosition = SplineContainer.EvaluatePosition(1f);


        _startingArrowHead = Instantiate(_arrowHeadPrefab, startingPosition, Quaternion.identity, transform);

        _endingArrowHead = Instantiate(_arrowHeadPrefab, endingPosition, Quaternion.identity, transform);

        _startingArrowMovement = _startingArrowHead.GetComponent<ArrowMovement>();
        _endingArrowMovement = _endingArrowHead.GetComponent<ArrowMovement>();

        _startingSafetyChecker = _startingArrowHead.GetComponent<ArrowSafetyChecker>();
        _endingSafetyChecker = _endingArrowHead.GetComponent<ArrowSafetyChecker>();

        if (_startingArrowMovement == null || _endingArrowMovement == null || _startingSafetyChecker == null || _endingSafetyChecker == null)
        {
            Debug.LogError("ArrowPairController: Arrow prefab is missing required components.", this);

            Destroy(_startingArrowHead);
            Destroy(_endingArrowHead);
            return;
        }

        _startingArrowMovement.Initialize(SplineContainer, ArrowMovement.ArrowDirection.Forward, this);

        _endingArrowMovement.Initialize(SplineContainer, ArrowMovement.ArrowDirection.Backward, this);

        _isInitialized = true;
    }

    public void ResetLine()
    {
        if (_isCompleted || !_isInitialized)
            return;

        _startingArrowHead.SetActive(true);
        _endingArrowHead.SetActive(true);

        _startingArrowMovement.ResetMovement();
        _endingArrowMovement.ResetMovement();

        _lineRenderer.ShowRange(0f, 1f);
        _lineRenderer.ResetLineAppearance();
    }

    public void CompleteLine()
    {
        if (_isCompleted || !_isInitialized)
            return;

        _isCompleted = true;

        _lineRenderer.HideLine();

        _startingArrowHead.SetActive(false);
        _endingArrowHead.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.CheckLevelCompletion();
    }

    public void ShowLineRange(float fromT, float toT)
    {
        if (_isCompleted || !_isInitialized)
            return;

        _lineRenderer.ShowRange(fromT, toT);
    }

    public void HighlightLine()
    {
        if (_isCompleted || !_isInitialized)
            return;

        _lineRenderer.HighlightLine();
    }

    public void ResetLineHighlight()
    {
        if (_isCompleted || !_isInitialized)
            return;

        _lineRenderer.ResetLineAppearance();
    }

    public ArrowMovement GetSafeArrow()
    {
        if (_isCompleted || !_isInitialized)
            return null;

        bool startingIsSafe = _startingSafetyChecker.CanCompleteLine();
        bool endingIsSafe = _endingSafetyChecker.CanCompleteLine();

        if (startingIsSafe && endingIsSafe)
        {
            return Random.value < 0.5f
                ? _startingArrowMovement
                : _endingArrowMovement;
        }

        if (startingIsSafe)
            return _startingArrowMovement;

        if (endingIsSafe)
            return _endingArrowMovement;

        return null;
    }

    public void OnArrowMovementStarted(ArrowMovement movingArrow)
    {
        if (!_isInitialized || _isCompleted)
            return;

        if (movingArrow == _startingArrowMovement)
            _endingArrowMovement.SetCanMove(false);
        else if (movingArrow == _endingArrowMovement)
            _startingArrowMovement.SetCanMove(false);


        if (GameManager.Instance != null)
            GameManager.Instance.ClearHintHighlight();
    }
}