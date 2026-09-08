using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    public enum Estado
    {
        Vermelho,
        Amarelo,
        Verde
    }

    [Header("Estado atual")]
    [SerializeField] private Estado estadoAtual = Estado.Vermelho;

    [Header("Tempo")]
    [SerializeField] private float tempoVerde = 10f;
    [SerializeField] private float tempoAmarelo = 3f;
    [SerializeField] private float tempoVermelho = 10f;

    [Header("Luzes")]
    [SerializeField] private GameObject luzVermelha;
    [SerializeField] private GameObject luzAmarela;
    [SerializeField] private GameObject luzVerde;

    private float contador;

    private void Start()
    {
        AtualizarLuzes();
    }

    private void Update()
    {
        contador += Time.deltaTime;

        switch (estadoAtual)
        {
            case Estado.Vermelho:

                if (contador >= tempoVermelho)
                {
                    estadoAtual = Estado.Verde;
                    contador = 0f;
                    AtualizarLuzes();
                }

                break;

            case Estado.Verde:

                if (contador >= tempoVerde)
                {
                    estadoAtual = Estado.Amarelo;
                    contador = 0f;
                    AtualizarLuzes();
                }

                break;

            case Estado.Amarelo:

                if (contador >= tempoAmarelo)
                {
                    estadoAtual = Estado.Vermelho;
                    contador = 0f;
                    AtualizarLuzes();
                }

                break;
        }
    }

    private void AtualizarLuzes()
    {
        if (luzVermelha != null)
            luzVermelha.SetActive(
                estadoAtual == Estado.Vermelho
            );

        if (luzAmarela != null)
            luzAmarela.SetActive(
                estadoAtual == Estado.Amarelo
            );

        if (luzVerde != null)
            luzVerde.SetActive(
                estadoAtual == Estado.Verde
            );
    }

    public bool PodePassar()
    {
        return estadoAtual == Estado.Verde;
    }

    public bool EstaAmarelo()
    {
        return estadoAtual == Estado.Amarelo;
    }

    public bool EstaVermelho()
    {
        return estadoAtual == Estado.Vermelho;
    }
    public void DefinirEstado(Estado novoEstado)
    {
        estadoAtual = novoEstado;
        contador = 0f;
        AtualizarLuzes();
    }
}