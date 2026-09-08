using UnityEngine;

public class TrafficCar : MonoBehaviour
{
    [Header("Rota")]
    [SerializeField] private TrafficRoute rota;

    [Header("Configuração")]
    [SerializeField] private float velocidade = 8f;
    [SerializeField] private float distanciaDoPonto = 1f;
    [SerializeField] private float velocidadeRotacao = 5f;

    [Header("Semáforo")]
    [SerializeField] private float distanciaParada = 3f;
    [SerializeField] private float distanciaDesaceleracao = 18f;

    [Header("Amarelo")]
    [SerializeField] private float velocidadeMinimaAmarelo = 0.25f;

    private TrafficLight semaforo;
    private int pontoAtual;

    private void Start()
    {
        if (rota == null ||
            rota.pontos == null ||
            rota.pontos.Length < 2)
        {
            Debug.LogWarning(
                "TrafficCar sem rota: " +
                gameObject.name
            );

            return;
        }

        pontoAtual = 0;

        transform.position =
            rota.pontos[0].position;

        velocidadeRotacao =
            Mathf.Max(
                velocidadeRotacao,
                0.1f
            );

        EncontrarSemaforo();
    }

    private void Update()
    {
        if (rota == null ||
            rota.pontos == null ||
            rota.pontos.Length < 2)
            return;

        if (pontoAtual >= rota.pontos.Length)
            return;

        Transform alvo =
            rota.pontos[pontoAtual];

        if (alvo == null)
        {
            pontoAtual++;
            return;
        }

        EncontrarSemaforo();

        Vector3 direcao =
            alvo.position -
            transform.position;

        direcao.y = 0f;

        float distancia =
            direcao.magnitude;

        // ==========================================
        // CHEGOU NO PONTO
        // ==========================================

        if (distancia <= distanciaDoPonto)
        {
            // Se for o último ponto, troca de rota
            if (pontoAtual ==
                rota.pontos.Length - 1)
            {
                // Coloca exatamente no último ponto
                transform.position =
                    alvo.position;

                TrocarParaProximaRota();
                return;
            }

            pontoAtual++;
            return;
        }

        if (direcao.sqrMagnitude < 0.001f)
            return;

        direcao.Normalize();

        // ==========================================
        // SEMÁFORO
        // ==========================================

        if (semaforo != null)
        {
            int pontoSemaforo =
                rota.GetPontoDoSemaforo();

            if (pontoSemaforo >= 0 &&
                pontoSemaforo < rota.pontos.Length &&
                pontoAtual <= pontoSemaforo)
            {
                Transform pontoParada =
                    rota.pontos[pontoSemaforo];

                if (pontoParada != null)
                {
                    Vector3 direcaoAteParada =
                        pontoParada.position -
                        transform.position;

                    direcaoAteParada.y = 0f;

                    float distanciaAteParada =
                        direcaoAteParada.magnitude;

                    // ======================================
                    // VERMELHO
                    // ======================================

                    if (semaforo.EstaVermelho())
                    {
                        // PARA COMPLETAMENTE
                        // e não mexe na rotação
                        if (distanciaAteParada <=
                            distanciaParada)
                        {
                            return;
                        }

                        float velocidadeAtual =
                            velocidade;

                        if (distanciaAteParada <=
                            distanciaDesaceleracao)
                        {
                            float fator =
                                Mathf.InverseLerp(
                                    distanciaParada,
                                    distanciaDesaceleracao,
                                    distanciaAteParada
                                );

                            velocidadeAtual =
                                Mathf.Lerp(
                                    0f,
                                    velocidade,
                                    fator
                                );
                        }

                        Mover(
                            direcao,
                            velocidadeAtual
                        );

                        return;
                    }

                    // ======================================
                    // AMARELO
                    // ======================================

                    if (semaforo.EstaAmarelo())
                    {
                        float velocidadeAtual =
                            velocidade;

                        if (distanciaAteParada <=
                            distanciaDesaceleracao)
                        {
                            float fator =
                                Mathf.InverseLerp(
                                    distanciaParada,
                                    distanciaDesaceleracao,
                                    distanciaAteParada
                                );

                            velocidadeAtual =
                                Mathf.Lerp(
                                    velocidadeMinimaAmarelo,
                                    velocidade,
                                    fator
                                );
                        }

                        Mover(
                            direcao,
                            velocidadeAtual
                        );

                        return;
                    }
                }
            }
        }

        // ==========================================
        // MOVIMENTO NORMAL
        // ==========================================

        Mover(
            direcao,
            velocidade
        );
    }

    // ==============================================
    // MOVIMENTO
    // ==============================================

    private void Mover(
        Vector3 direcao,
        float velocidadeAtual)
    {
        if (direcao.sqrMagnitude < 0.001f ||
            velocidadeAtual <= 0f)
            return;

        direcao.Normalize();

        Quaternion rotacao =
            Quaternion.LookRotation(
                direcao
            );

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                rotacao,
                velocidadeRotacao *
                Time.deltaTime
            );

        float distanciaMovimento =
            velocidadeAtual *
            Time.deltaTime;

        // Não deixa o carro passar do ponto
        float distanciaAteProximoPonto =
            Vector3.Distance(
                transform.position,
                rota.pontos[pontoAtual].position
            );

        if (distanciaMovimento >=
            distanciaAteProximoPonto)
        {
            transform.position =
                rota.pontos[pontoAtual].position;
        }
        else
        {
            transform.position +=
                direcao *
                distanciaMovimento;
        }
    }

    // ==============================================
    // SEMÁFORO DA ROTA
    // ==============================================

    private void EncontrarSemaforo()
    {
        if (rota == null)
        {
            semaforo = null;
            return;
        }

        semaforo =
            rota.GetSemaforo();
    }

    // ==============================================
    // PRÓXIMA ROTA
    // ==============================================

    private void TrocarParaProximaRota()
    {
        TrafficRoute proximaRota =
            rota.EncontrarProximaRota();

        if (proximaRota == null)
        {
            Debug.LogWarning(
                "O carro chegou ao final da rota " +
                rota.name +
                " mas não encontrou outra rota."
            );

            return;
        }

        rota =
            proximaRota;

        pontoAtual = 0;

        EncontrarSemaforo();

        Debug.Log(
            gameObject.name +
            " mudou para " +
            rota.name
        );
    }
}