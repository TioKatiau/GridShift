using UnityEngine;
using System.Collections.Generic;

public class TrafficRoute : MonoBehaviour
{
    [Header("Pontos da rota")]
    public Transform[] pontos;

    [Header("Semáforo")]
    [SerializeField] private TrafficLight semaforo;
    [SerializeField] private int pontoDoSemaforo = -1;
    [SerializeField] private float distanciaMaximaSemaforo = 30f;

    [Header("Conexão entre rotas")]
    [SerializeField] private float raioConexao = 8f;

    private static TrafficRoute[] todasAsRotas;

    private void Awake()
    {
        todasAsRotas = FindObjectsOfType<TrafficRoute>();
    }

    private void Start()
    {
        EncontrarSemaforoMaisProximo();
    }

    // =========================================================
    // ENCONTRAR SEMÁFORO MAIS PRÓXIMO À FRENTE
    // =========================================================

    private void EncontrarSemaforoMaisProximo()
    {
        if (pontos == null || pontos.Length < 2)
            return;

        TrafficLight[] semaforos =
            FindObjectsOfType<TrafficLight>();

        TrafficLight melhorSemaforo = null;
        float menorDistancia = Mathf.Infinity;
        int melhorPonto = -1;

        foreach (TrafficLight candidato in semaforos)
        {
            if (candidato == null)
                continue;

            Vector3 posicaoSemaforo =
                candidato.transform.position;

            // Procura o ponto da rota mais próximo
            // do semáforo
            for (int i = 0; i < pontos.Length; i++)
            {
                if (pontos[i] == null)
                    continue;

                Vector3 direcaoAteSemaforo =
                    posicaoSemaforo -
                    pontos[i].position;

                direcaoAteSemaforo.y = 0f;

                float distancia =
                    direcaoAteSemaforo.magnitude;

                if (distancia > distanciaMaximaSemaforo)
                    continue;

                // -------------------------------------------------
                // DIREÇÃO DA ROTA NESSE PONTO
                // -------------------------------------------------

                Vector3 direcaoRota;

                if (i < pontos.Length - 1)
                {
                    direcaoRota =
                        pontos[i + 1].position -
                        pontos[i].position;
                }
                else
                {
                    direcaoRota =
                        pontos[i].position -
                        pontos[i - 1].position;
                }

                direcaoRota.y = 0f;

                if (direcaoRota.sqrMagnitude < 0.01f)
                    continue;

                direcaoRota.Normalize();

                // Direção do ponto da rota até o semáforo
                direcaoAteSemaforo.Normalize();

                // -------------------------------------------------
                // VERIFICA SE O SEMÁFORO ESTÁ À FRENTE
                // -------------------------------------------------

                float alinhamento =
                    Vector3.Dot(
                        direcaoRota,
                        direcaoAteSemaforo
                    );

                // Quanto maior, mais à frente está.
                // 0.3 = aproximadamente dentro de 72 graus.
                if (alinhamento < 0.3f)
                    continue;

                // -------------------------------------------------
                // ESCOLHE O MAIS PRÓXIMO
                // -------------------------------------------------

                if (distancia < menorDistancia)
                {
                    menorDistancia = distancia;
                    melhorSemaforo = candidato;
                    melhorPonto = i;
                }
            }
        }

        semaforo = melhorSemaforo;
        pontoDoSemaforo = melhorPonto;

        if (semaforo != null)
        {
            Debug.Log(
                gameObject.name +
                " encontrou o semáforo: " +
                semaforo.name +
                " | Ponto: " +
                pontoDoSemaforo
            );
        }
        else
        {
            Debug.Log(
                gameObject.name +
                " não encontrou semáforo à frente."
            );
        }
    }

    // =========================================================
    // GETTERS
    // =========================================================

    public TrafficLight GetSemaforo()
    {
        return semaforo;
    }

    public int GetPontoDoSemaforo()
    {
        return pontoDoSemaforo;
    }

    // =========================================================
    // ENCONTRAR PRÓXIMA ROTA
    // =========================================================

    public TrafficRoute EncontrarProximaRota()
    {
        if (pontos == null || pontos.Length < 2)
            return null;

        Transform ultimoPonto =
            pontos[pontos.Length - 1];

        Transform penultimoPonto =
            pontos[pontos.Length - 2];

        if (ultimoPonto == null ||
            penultimoPonto == null)
            return null;

        Vector3 direcaoAtual =
            ultimoPonto.position -
            penultimoPonto.position;

        direcaoAtual.y = 0f;

        if (direcaoAtual.sqrMagnitude < 0.01f)
            return null;

        direcaoAtual.Normalize();

        List<TrafficRoute> possibilidades =
            new List<TrafficRoute>();

        foreach (TrafficRoute candidata in todasAsRotas)
        {
            if (candidata == null ||
                candidata == this)
                continue;

            if (candidata.pontos == null ||
                candidata.pontos.Length < 2)
                continue;

            if (candidata.pontos[0] == null ||
                candidata.pontos[1] == null)
                continue;

            float distancia =
                Vector3.Distance(
                    ultimoPonto.position,
                    candidata.pontos[0].position
                );

            if (distancia > raioConexao)
                continue;

            Vector3 direcaoNova =
                candidata.pontos[1].position -
                candidata.pontos[0].position;

            direcaoNova.y = 0f;

            if (direcaoNova.sqrMagnitude < 0.01f)
                continue;

            direcaoNova.Normalize();

            float alinhamento =
                Vector3.Dot(
                    direcaoAtual,
                    direcaoNova
                );

            if (alinhamento < -0.3f)
                continue;

            possibilidades.Add(candidata);
        }

        if (possibilidades.Count == 0)
        {
            Debug.LogWarning(
                "Nenhuma rota encontrada para: " +
                gameObject.name
            );

            return null;
        }

        TrafficRoute escolhida =
            possibilidades[
                Random.Range(
                    0,
                    possibilidades.Count
                )
            ];

        Debug.Log(
            gameObject.name +
            " escolheu a rota: " +
            escolhida.name
        );

        return escolhida;
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    private void OnDrawGizmos()
    {
        if (pontos == null ||
            pontos.Length < 2)
            return;

        Gizmos.color = Color.yellow;

        for (int i = 0;
             i < pontos.Length - 1;
             i++)
        {
            if (pontos[i] != null &&
                pontos[i + 1] != null)
            {
                Gizmos.DrawLine(
                    pontos[i].position,
                    pontos[i + 1].position
                );

                Gizmos.DrawSphere(
                    pontos[i].position,
                    0.2f
                );
            }
        }

        // Raio de conexão
        if (pontos[pontos.Length - 1] != null)
        {
            Gizmos.color = Color.cyan;

            Gizmos.DrawWireSphere(
                pontos[pontos.Length - 1].position,
                raioConexao
            );
        }

        // Ponto encontrado do semáforo
        if (pontoDoSemaforo >= 0 &&
            pontoDoSemaforo < pontos.Length &&
            pontos[pontoDoSemaforo] != null)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(
                pontos[pontoDoSemaforo].position,
                0.6f
            );
        }
    }
}