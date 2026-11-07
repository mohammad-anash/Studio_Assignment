using UnityEngine;
using UnityEngine.InputSystem;

public class ArrowInputHandler : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private LayerMask _arrowHeadLayer;

    private Camera _mainCamera;

    private bool HasInvalidSetup => _mainCamera == null;

    private void Awake()
    {
        _mainCamera = Camera.main;

        if (HasInvalidSetup)
            Debug.LogError("ArrowInputHandler: Main Camera not found.", this);
    }

    private void Update()
    {
        if (HasInvalidSetup || Pointer.current == null)
            return;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        if (!Pointer.current.press.wasPressedThisFrame)
            return;

        HandleClick();
    }

    private void HandleClick()
    {
        Vector2 screenPosition = Pointer.current.position.ReadValue();
        Vector3 worldPosition = GetWorldPosition(screenPosition);

        Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition, _arrowHeadLayer);


        foreach (Collider2D hit in hits)
        {
            ArrowMovement arrow = hit.GetComponentInParent<ArrowMovement>();

            if (arrow == null || !arrow.CanMove || arrow.IsMoving)
                continue;

            arrow.StartMovement();
            return;
        }
    }

    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        float distance = Mathf.Abs(_mainCamera.transform.position.z);

        return _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, distance));
    }
}