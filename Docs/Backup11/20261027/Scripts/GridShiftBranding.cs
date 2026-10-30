using UnityEngine;
[CreateAssetMenu(menuName="GridShift/Identidade visual")]
public sealed class GridShiftBranding:ScriptableObject {
 public Sprite gs,urbanFlow;
 static GridShiftBranding cached;
 public static GridShiftBranding Current=>cached!=null?cached:cached=Resources.Load<GridShiftBranding>("GridShiftBranding");
}
