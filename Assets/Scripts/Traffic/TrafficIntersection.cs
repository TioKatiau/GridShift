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

    private void Start()
    {
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
            tempoAtual = 1f;
        else if (etapa == 3)
            tempoAtual = tempoVerde;
        else
            tempoAtual = tempoAmarelo;

        if (contador >= tempoAtual)
        {
            contador = 0f;
            etapa++;

            if (etapa > 4)
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

            // Ambos vermelhos por 1 segundo
            case 2:
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