using UnityEngine;
public static class EscapeInput {
 static int frame=-1;
 public static bool Consumed => frame==Time.frameCount;
 public static void Consume(){frame=Time.frameCount;}
}
