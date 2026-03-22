using UnityEngine;

[CreateAssetMenu(fileName = "New BreakableObject", menuName = "DesignerStuff/BreakableObject_SO")]
public class BreakableObject_SO : ScriptableObject
{
    [Range(0f, 10f)]
    [SerializeField] private float breakSpeed = 0.5f;
    [Range(0.2f, 10f)]
    [SerializeField] private float timeBeforeDespawn = 2f;
    [Range(0.01f, 2f)]
    [SerializeField] private float fragmentDespawnSpeed = 0.25f;
    [Range(0f, 20f)]
    [SerializeField] private float alertDetectionDistance = 8f;
    [Header("Drag and drop the original object PREFAB below:")]
    public GameObject original;
    [Header("Drag and drop the fractured object PREFAB below:")]
    public GameObject fractured;
    [Header("Transparent material goes below here:")]
    private Material TRANSPARENT_MATERIAL_SRC;
}
