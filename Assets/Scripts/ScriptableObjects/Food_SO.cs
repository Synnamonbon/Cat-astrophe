using UnityEngine;

[CreateAssetMenu(fileName = "New Food", menuName = "DesignerStuff/Food_SO")]
public class Food_SO : ScriptableObject
{
    [SerializeField] private float hungerRestoreValue = 20f;

    public float HungerRestoreValue => hungerRestoreValue;
}
