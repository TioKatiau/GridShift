using UnityEngine;

// Uma reserva por cruzamento; liberada somente quando toda a traseira já saiu.
public class TrafficJunction : MonoBehaviour
{
    [SerializeField] private TrafficCar owner;
    [SerializeField] private PedestrianAgent pedestrian;
    public bool PedestrianOccupied => pedestrian != null && pedestrian.isActiveAndEnabled;
    public bool TryCross(PedestrianAgent agent)
    {
        if (PedestrianOccupied) return pedestrian == agent;
        if (owner != null && owner.isActiveAndEnabled) return false;
        pedestrian = agent; return true;
    }
    public void LeaveCrossing(PedestrianAgent agent) { if (pedestrian == agent) pedestrian = null; }
    public bool TryEnter(TrafficCar vehicle)
    {
        if (PedestrianOccupied) return false;
        if (owner != null && owner.isActiveAndEnabled && owner != vehicle) return false;
        owner = vehicle; return true;
    }
    public bool Available(TrafficCar vehicle) => !PedestrianOccupied && (owner == null || !owner.isActiveAndEnabled || owner == vehicle);
    public void Release(TrafficCar vehicle) { if (owner == vehicle) owner = null; }
}
