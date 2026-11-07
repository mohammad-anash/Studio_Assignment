using UnityEngine;

public class ArrowCollisionHandler : MonoBehaviour
{
    private ArrowMovement _arrowMovement;

    private void Awake()
    {
        _arrowMovement = GetComponent<ArrowMovement>();

        if (_arrowMovement == null)
            Debug.LogError("ArrowCollisionHandler: ArrowMovement missing.", this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_arrowMovement == null)
            return;

        ArrowMovement otherArrow = other.GetComponentInParent<ArrowMovement>();

        if (otherArrow == null || otherArrow == _arrowMovement)
            return;

        ArrowPairController currentPair = _arrowMovement.PairController;
        ArrowPairController otherPair = otherArrow.PairController;

        if (currentPair == null || otherPair == null)
            return;

        if (currentPair == otherPair)
            return;

        if (currentPair.IsCompleted || otherPair.IsCompleted)
            return;

        if (_arrowMovement.GetEntityId() > otherArrow.GetEntityId())
            return;

        if (!_arrowMovement.IsMoving && !otherArrow.IsMoving)
            return;

        if (GameManager.Instance != null)
            GameManager.Instance.HandleArrowCollision();

        currentPair.ResetLine();
        otherPair.ResetLine();
    }
}