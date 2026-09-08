using UnityEngine;

public class TrafficLightGroup : MonoBehaviour
{
    [Header("Semáforos deste grupo")]
    [SerializeField] private TrafficLight[] semaforos;

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