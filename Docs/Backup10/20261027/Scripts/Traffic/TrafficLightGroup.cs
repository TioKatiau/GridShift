using UnityEngine;

public class TrafficLightGroup : MonoBehaviour
{
    [Header("Semáforos deste grupo")]
    [SerializeField] private TrafficLight[] semaforos;
    private void Awake() { if (semaforos != null) foreach (var s in semaforos) if (s != null) s.SetGroupControl(); }

    public void DefinirEstado(TrafficLight.Estado estado)
    {
        foreach (TrafficLight semaforo in semaforos)
        {
            if (semaforo == null)
                continue;

            semaforo.DefinirEstado(estado);
        }
    }
}
