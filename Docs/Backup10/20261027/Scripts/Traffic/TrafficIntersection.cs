using UnityEngine;

public class TrafficIntersection : MonoBehaviour
{
    [Header("Grupos")]
    [SerializeField] private TrafficLightGroup grupoHorizontal;
    [SerializeField] private TrafficLightGroup grupoVertical;

    [Header("Tempos")]
    [SerializeField] private float tempoVerde = 10f;
    [SerializeField] private float tempoAmarelo = 3f;

    private float contador;
    private int etapa = 0;
    private TrafficJunction[] junctions;
    private static TrafficIntersection instance;
    public static bool PedestrianAdmissionOpen {
        get {
            if(instance==null)return true;
            if((instance.etapa!=2&&instance.etapa!=5)||instance.contador>=4f||instance.junctions==null)return false;
            return true;
        }
    }
    private void Awake(){instance=this;}
    private void OnDestroy(){if(instance==this)instance=null;}

    private void Start()
    {
        junctions=FindObjectsByType<TrafficJunction>(FindObjectsSortMode.None);
        AplicarEstado();
    }

    private void Update()
    {
        contador += Time.deltaTime;

        float tempoAtual;

        if (etapa == 0)
            tempoAtual = tempoVerde;
        else if (etapa == 1)
            tempoAtual = tempoAmarelo;
        else if (etapa == 2)
            tempoAtual = 4f;
        else if (etapa == 3)
            tempoAtual = tempoVerde;
        else if (etapa == 4)
            tempoAtual = tempoAmarelo;
        else tempoAtual = 4f;

        if (contador >= tempoAtual)
        {
            // Finish the pedestrian clearance phase before starting a full vehicle green.
            if(etapa==2||etapa==5)foreach(var junction in junctions)if(junction!=null&&junction.PedestrianOccupied)return;
            contador = 0f;
            etapa++;

            if (etapa > 5)
                etapa = 0;

            AplicarEstado();
        }
    }

    private void AplicarEstado()
    {
        switch (etapa)
        {
            // Horizontal verde
            case 0:
                grupoHorizontal.DefinirEstado(TrafficLight.Estado.Verde);
                grupoVertical.DefinirEstado(TrafficLight.Estado.Vermelho);
                break;

            // Horizontal amarelo
            case 1:
                grupoHorizontal.DefinirEstado(TrafficLight.Estado.Amarelo);
                grupoVertical.DefinirEstado(TrafficLight.Estado.Vermelho);
                break;

            // Ambos vermelhos: janela de quatro segundos, estendida para concluir a travessia.
            case 2:
            case 5:
                grupoHorizontal.DefinirEstado(TrafficLight.Estado.Vermelho);
                grupoVertical.DefinirEstado(TrafficLight.Estado.Vermelho);
                break;

            // Vertical verde
            case 3:
                grupoHorizontal.DefinirEstado(TrafficLight.Estado.Vermelho);
                grupoVertical.DefinirEstado(TrafficLight.Estado.Verde);
                break;

            // Vertical amarelo
            case 4:
                grupoHorizontal.DefinirEstado(TrafficLight.Estado.Vermelho);
                grupoVertical.DefinirEstado(TrafficLight.Estado.Amarelo);
                break;
        }
    }
}
