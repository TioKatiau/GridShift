using UnityEngine;

public sealed class GridShiftCityBoundary : MonoBehaviour
{
    public Bounds playable;
    public float groundHeight=-4.6f;
    PlayerMovement player;
    Vector3 lastSafe;
    float nextCheck;
    void Update()
    {
        if(Time.unscaledTime<nextCheck)return;nextCheck=Time.unscaledTime+.25f;
        if(player==null)foreach(var p in FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))if(p.gameObject.scene==gameObject.scene){player=p;lastSafe=p.transform.position;break;}
        if(player==null)return;
        var at=player.transform.position;
        bool inside=at.x>playable.min.x&&at.x<playable.max.x&&at.z>playable.min.z&&at.z<playable.max.z;
        if(inside&&at.y>groundHeight-1&&Physics.Raycast(at+Vector3.up*.2f,Vector3.down,2,~0,QueryTriggerInteraction.Ignore)){lastSafe=at;return;}
        // Emergency fallback only after a fall; ordinary movement is stopped by perimeter colliders.
        if(at.y<groundHeight-8){var c=player.GetComponent<CharacterController>();if(c!=null)c.enabled=false;player.transform.position=lastSafe;if(c!=null)c.enabled=true;}
    }
}
