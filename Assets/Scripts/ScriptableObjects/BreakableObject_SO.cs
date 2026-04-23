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
    [SerializeField] private ObjectType objectType;
    [Header("Drag and drop the original object PREFAB below:")]
    [SerializeField] private GameObject original;
    [Header("Drag and drop the fractured object PREFAB below:")]
    [SerializeField] private GameObject fractured;
    [Header("Transparent material goes below here:")]
    [SerializeField] private Material transparent_material_src;

    public float BreakSpeed => breakSpeed;
    public float TimeBeforeDespawn => timeBeforeDespawn;
    public float FragmentDespawnSpeed => fragmentDespawnSpeed;
    public float AlertDetectionDistance => alertDetectionDistance;
    public ObjectType ObjectType => objectType;
    public GameObject Original => original;
    public GameObject Fractured => fractured;
    public Material TRANSPARENT_MATERIAL_SRC => transparent_material_src;
}
