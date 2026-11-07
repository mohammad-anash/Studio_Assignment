using UnityEngine;
using UnityEngine.Splines;

public class ArrowMovement : MonoBehaviour
{
    public enum ArrowDirection
    {
        Forward,
        Backward
    }

    [Header("Arrow Movement Settings")]
    [SerializeField] private ArrowDirection _direction;
    [Tooltip("Units per second along the line.")]
    [SerializeField, Min(0.1f)] private float _speed = 4f;

    [Header("Arrow Highlight Settings")]
    [SerializeField] private Color _highlightColor = Color.yellow;

    private SplineContainer _splineContainer;
    private ArrowPairController _pairController;
    private SpriteRenderer _spriteRenderer;

    private Color _normalColor;

    private float _currentProgress;

    private bool _isMoving;
    private bool _canMove = true;

    public ArrowDirection Direction => _direction;
    public float CurrentProgress => _currentProgress;
    public ArrowPairController PairController => _pairController;

    public bool IsMoving => _isMoving;
    public bool CanMove => _canMove;

    private bool HasInvalidSetup => _splineContainer == null || _pairController == null;
    private float Destination => _direction == ArrowDirection.Forward ? 1f : 0f;
    private float DirectionMultiplier => _direction == ArrowDirection.Forward ? 1f : -1f;

    private bool HasReachedDestination =>
        Mathf.Approximately(_currentProgress, Destination);

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteRenderer == null)
        {
            Debug.LogError("ArrowMovement: SpriteRenderer is missing.", this);
            return;
        }

        _normalColor = _spriteRenderer.color;
    }

    public void Initialize(
        SplineContainer splineContainer,
        ArrowDirection direction,
        ArrowPairController pairController)
    {
        if (splineContainer == null || pairController == null)
        {
            Debug.LogError("ArrowMovement: Required references are missing.", this);
            return;
        }

        _splineContainer = splineContainer;
        _direction = direction;
        _pairController = pairController;

        ResetMovement();
    }

    public void StartMovement()
    {
        if (HasInvalidSetup || !_canMove || _isMoving)
            return;

        if (_pairController.IsCompleted)
            return;

        ResetArrowAppearance();

        _isMoving = true;

        _pairController.OnArrowMovementStarted(this);
    }

    public void SetCanMove(bool value)
    {
        _canMove = value;
    }

    public void HighlightArrow()
    {
        if (_spriteRenderer == null || !_canMove || _isMoving)
            return;

        _spriteRenderer.color = _highlightColor;
    }

    public void ResetArrowAppearance()
    {
        if (_spriteRenderer == null)
            return;

        _spriteRenderer.color = _normalColor;
    }

    private void Update()
    {
        if (!_isMoving || HasInvalidSetup)
            return;


        float lineLength = Mathf.Max(0.01f, _pairController.LineLength);
        float progressStep = _speed / lineLength * Time.deltaTime;

        _currentProgress = Mathf.Clamp01(_currentProgress + DirectionMultiplier * progressStep);

        UpdateArrowPosition();
        UpdateLineVisual();

        if (HasReachedDestination)
        {
            _isMoving = false;
            _pairController.CompleteLine();
        }
    }

    private void UpdateArrowPosition()
    {
        if (_splineContainer == null)
            return;

        transform.position = _splineContainer.EvaluatePosition(_currentProgress);

        Vector3 tangent = _splineContainer.EvaluateTangent(_currentProgress);

        tangent *= DirectionMultiplier;

        float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg - 90f;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void UpdateLineVisual()
    {
        if (_direction == ArrowDirection.Forward)
            _pairController.ShowLineRange(_currentProgress, 1f);
        else
            _pairController.ShowLineRange(0f, _currentProgress);
    }

    public void ResetMovement()
    {
        _isMoving = false;
        _canMove = true;

        ResetArrowAppearance();

        _currentProgress = _direction == ArrowDirection.Forward ? 0f : 1f;

        UpdateArrowPosition();
    }
}