using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Arrows/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Settings")]
    [SerializeField] private string _levelName = "Level 1";
    [SerializeField, Min(1)] private int _startLives = 3;

    [Header("Layout")]
    [SerializeField] private GameObject _layoutPrefab;

    public string LevelName => _levelName;
    public int StartLives => _startLives;
    public GameObject LayoutPrefab => _layoutPrefab;
}