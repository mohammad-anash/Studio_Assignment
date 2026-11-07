using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class ArrowSafetyChecker : MonoBehaviour
{
    [Header("Safety Check Settings")]
    [SerializeField] private LayerMask _arrowHeadLayer;
    [SerializeField, Range(10, 300)] private int _checkSteps = 100;

    private const int CollisionBufferSize = 32;
    private readonly Collider2D[] _collisionResults = new Collider2D[CollisionBufferSize];

    private readonly HashSet<ArrowMovement> _previousTouching = new HashSet<ArrowMovement>();
    private readonly HashSet<ArrowMovement> _currentTouching = new HashSet<ArrowMovement>();

    private ArrowMovement _arrowMovement;
    private Collider2D _arrowCollider;

    private ContactFilter2D _contactFilter;

    private void Awake()
    {
        _arrowMovement = GetComponent<ArrowMovement>();
        _arrowCollider = GetComponent<PolygonCollider2D>();

        if (_arrowMovement == null || _arrowCollider == null)
        {
            Debug.LogError("ArrowSafetyChecker: ArrowMovement or PolygonCollider2D is missing.", this);
            return;
        }

        _contactFilter = new ContactFilter2D();
        _contactFilter.SetLayerMask(_arrowHeadLayer);
        _contactFilter.useTriggers = true;
    }

    public bool CanCompleteLine()
    {
        if (_arrowMovement == null || _arrowCollider == null)
            return false;

        if (!_arrowMovement.CanMove || _arrowMovement.IsMoving)
            return false;

        ArrowPairController pairController = _arrowMovement.PairController;

        if (pairController == null || pairController.IsCompleted)
            return false;

        SplineContainer splineContainer = pairController.SplineContainer;

        if (splineContainer == null)
            return false;

        Vector3 originalPosition = transform.position;
        Quaternion originalRotation = transform.rotation;

        float startProgress = _arrowMovement.CurrentProgress;

        bool isForward =
            _arrowMovement.Direction == ArrowMovement.ArrowDirection.Forward;

        float destination = isForward ? 1f : 0f;
        float directionMultiplier = isForward ? 1f : -1f;

        int steps = Mathf.Max(1, _checkSteps);

        try
        {
            PlaceArrow(splineContainer, startProgress, directionMultiplier);
            CollectTouching(_previousTouching, pairController);

            for (int i = 1; i <= steps; i++)
            {
                float progress = Mathf.Lerp(startProgress, destination, (float)i / steps);

                PlaceArrow(splineContainer, progress, directionMultiplier);
                CollectTouching(_currentTouching, pairController);

                foreach (ArrowMovement otherArrow in _currentTouching)
                {
                    if (!_previousTouching.Contains(otherArrow))
                        return false;
                }

                _previousTouching.Clear();
                _previousTouching.UnionWith(_currentTouching);
            }

            return true;
        }
        finally
        {
            transform.SetPositionAndRotation(originalPosition, originalRotation);
            Physics2D.SyncTransforms();
        }
    }

    private void PlaceArrow(SplineContainer splineContainer, float progress, float directionMultiplier)
    {
        Vector3 position = splineContainer.EvaluatePosition(progress);

        Vector3 tangent = splineContainer.EvaluateTangent(progress);
        tangent *= directionMultiplier;

        float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg - 90f;

        transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, angle));
        Physics2D.SyncTransforms();
    }

    private void CollectTouching(HashSet<ArrowMovement> result, ArrowPairController myPair)
    {
        result.Clear();

        int hitCount = _arrowCollider.Overlap(_contactFilter, _collisionResults);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = _collisionResults[i];

            if (hit == null || hit == _arrowCollider)
                continue;

            ArrowMovement otherArrow = hit.GetComponentInParent<ArrowMovement>();

            if (otherArrow == null || otherArrow == _arrowMovement)
                continue;

            if (otherArrow.PairController == myPair)
                continue;

            if (otherArrow.PairController != null && otherArrow.PairController.IsCompleted)
                continue;

            result.Add(otherArrow);
        }
    }
}