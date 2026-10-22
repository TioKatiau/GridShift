using UnityEngine;
[System.Serializable] public class PedestrianNode {
 public Vector3 position;
 public int[] neighbours;
 public bool entrance;
 public bool entrancePath;
}
[System.Serializable] public class PedestrianCrossing {
 public int a,b;
 public TrafficJunction junction;
 public TrafficLight[] signals;
 public bool CanStart {
  get { if(signals!=null&&signals.Length>0){foreach(var s in signals)if(s!=null&&!s.EstaVermelho())return false;return true;}
   foreach(var c in TrafficCar.Active)if(c!=null&&Vector3.Distance(c.transform.position,junction.transform.position)<22)return false;
   return true;
  }
 }
}
public class PedestrianGraph:MonoBehaviour {
 public PedestrianNode[] nodes=new PedestrianNode[0];
 public PedestrianCrossing[] crossings=new PedestrianCrossing[0];
 public int Crossing(int a,int b){for(int i=0;i<crossings.Length;i++){var c=crossings[i];if((c.a==a&&c.b==b)||(c.a==b&&c.b==a))return i;}return -1;}
 void OnDrawGizmosSelected(){if(nodes==null)return;Gizmos.color=Color.cyan;for(int i=0;i<nodes.Length;i++){var n=nodes[i];foreach(int j in n.neighbours)if(j>=0&&j<nodes.Length)Gizmos.DrawLine(n.position,nodes[j].position);if(n.entrance){Gizmos.color=Color.yellow;Gizmos.DrawSphere(n.position,.25f);Gizmos.color=Color.cyan;}}Gizmos.color=Color.magenta;foreach(var c in crossings)Gizmos.DrawLine(nodes[c.a].position+Vector3.up*.1f,nodes[c.b].position+Vector3.up*.1f);}
}
