using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(LineRenderer))]
public class SplineLineRenderer : MonoBehaviour
{
    [Header("Spline Renderer Settings")]
    [SerializeField] private SplineContainer _splineContainer;

    [Header("Line Settings")]
    [SerializeField, Range(10, 100)] private int _count = 50;
    [SerializeField, Min(0.01f)] private float _normalWidth = 0.05f;
    [SerializeField, Min(0.01f)] private float _highlightWidth = 0.09f;

    private LineRenderer _lineRenderer;
    private float _length;

    public SplineContainer SplineContainer => _splineContainer;

    public float Length
    {
        get
        {
            if (_length <= 0f && _splineContainer != null)
                _length = MeasureLength();

            return _length;
        }
    }

    private bool HasInvalidSetup =>
        _splineContainer == null ||
        _lineRenderer == null ||
        _count < 2;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();

        if (HasInvalidSetup)
        {
            Debug.LogError("SplineLineRenderer: Invalid setup.", this);
            return;
        }

        SetWidth(_normalWidth);
    }

    private void Start()
    {
        ShowRange(0f, 1f);
    }


    public void ShowRange(float fromT, float toT)
    {
        if (HasInvalidSetup)
            return;

        _lineRenderer.positionCount = _count;

        for (int i = 0; i < _count; i++)
        {
            float t = Mathf.Lerp(fromT, toT, (float)i / (_count - 1));
            _lineRenderer.SetPosition(i, _splineContainer.EvaluatePosition(t));
        }
    }

    public void HighlightLine()
    {
        if (_lineRenderer == null || !_lineRenderer.enabled)
            return;

        SetWidth(_highlightWidth);
    }

    public void ResetLineAppearance()
    {
        if (_lineRenderer == null || !_lineRenderer.enabled)
            return;

        SetWidth(_normalWidth);
    }

    public void HideLine()
    {
        if (_lineRenderer == null)
            return;

        _lineRenderer.enabled = false;
    }

    private void SetWidth(float width)
    {
        _lineRenderer.startWidth = width;
        _lineRenderer.endWidth = width;
    }

    private float MeasureLength()
    {
        const int samples = 64;

        float length = 0f;
        Vector3 previous = _splineContainer.EvaluatePosition(0f);

        for (int i = 1; i <= samples; i++)
        {
            Vector3 current = _splineContainer.EvaluatePosition((float)i / samples);
            length += Vector3.Distance(previous, current);
            previous = current;
        }

        return length;
    }
}