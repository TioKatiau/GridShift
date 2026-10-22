using System.Collections.Generic;
using UnityEngine;

public class TrafficRoute : MonoBehaviour
{
    [Header("Pontos ordenados no centro da faixa; curvas devem ser amostradas")]
    public Transform[] pontos;
    [Tooltip("Limite desta via em m/s; zero usa o limite do veículo.")]
    [SerializeField, Min(0f)] private float velocidadeMaxima;
    public float VelocidadeMaxima => velocidadeMaxima > 0f ? velocidadeMaxima : float.PositiveInfinity;
    [SerializeField] private TrafficJunction cruzamento;
    public TrafficJunction Cruzamento => cruzamento;
    [Header("Semáforo da direção desta rota")]
    [SerializeField] private TrafficLight semaforo;
    [SerializeField] private int pontoDoSemaforo = -1;
    [Tooltip("Linha de retenção, antes da faixa. Forward aponta no sentido do trânsito.")]
    [SerializeField] private Transform linhaParada;
    [Tooltip("Ponto depois da faixa de saída; deve caber o caminhão inteiro após este ponto.")]
    [SerializeField] private Transform saidaCruzamento;
    [Header("Conexões dirigidas (inclua rotas específicas para cada conversão)")]
    [SerializeField] private TrafficRoute[] proximasRotas;
    [SerializeField] private float raioConexao = 8f;
    [Tooltip("Legado: somente pontas praticamente coincidentes são conectadas automaticamente.")]
    [SerializeField] private bool conectarAutomaticamente = true;
    private static readonly List<TrafficRoute> ativas = new List<TrafficRoute>();
    private float validationAt=-1;
    private bool validCache;
    public void Invalidate(){validationAt=-1;}
    public static bool Continuation(TrafficRoute broken,Vector3 position,Vector3 direction,out TrafficRoute route,out int point){
        route=null;point=1;float best=.35f*.35f;
        foreach(var candidate in ativas){if(candidate==broken||!candidate.Valida||candidate.Cruzamento!=broken.Cruzamento)continue;
            for(int i=1;i<candidate.pontos.Length;i++){Vector3 a=candidate.pontos[i-1].position,b=candidate.pontos[i].position;var ab=Vector3.ProjectOnPlane(b-a,Vector3.up);if(Vector3.Dot(ab.normalized,direction)<.95f)continue;
                float t=Vector3.Dot(Vector3.ProjectOnPlane(position-a,Vector3.up),ab)/ab.sqrMagnitude;if(t<0||t>1)continue;
                float distance=Vector3.ProjectOnPlane(position-(a+ab*t),Vector3.up).sqrMagnitude;
                if(distance<best&&Mathf.Abs(position.y-a.y)<.2f){best=distance;route=candidate;point=i;}
            }
        }return route!=null;
    }
    public bool Valida
    {
        get
        {
            if(Application.isPlaying&&Time.time<validationAt)return validCache;
            validationAt=Time.time+1;validCache=false;
            if (pontos == null || pontos.Length < 2) return false;
            foreach (Transform ponto in pontos) if (ponto == null) return false;
            for (int i = 1; i < pontos.Length; i++)
                if ((pontos[i].position - pontos[i - 1].position).sqrMagnitude < 0.0001f) return false;
            validCache=true;return true;
        }
    }
    private void OnEnable() { Invalidate();if (!ativas.Contains(this)) ativas.Add(this); }
    private void OnValidate(){Invalidate();}
    private void OnDisable() { ativas.Remove(this); }
    public TrafficLight GetSemaforo() { return semaforo; }
    public int GetPontoDoSemaforo() { return pontoDoSemaforo; }
    public bool TemSaidaCruzamento => saidaCruzamento != null;
    public Vector3 Saida => saidaCruzamento.position;
    public bool GetParada(out Vector3 posicao, out Vector3 direcao)
    {
        posicao = Vector3.zero; direcao = Vector3.forward;
        if (!Valida) return false;
        if (linhaParada != null)
        {
            posicao = linhaParada.position;
            direcao = Vector3.ProjectOnPlane(linhaParada.forward, Vector3.up).normalized;
            return direcao.sqrMagnitude > 0.5f;
        }
        if (pontoDoSemaforo < 0 || pontoDoSemaforo >= pontos.Length) return false;
        int i = pontoDoSemaforo;
        posicao = pontos[i].position;
        direcao = Vector3.ProjectOnPlane(i == 0 ? pontos[1].position - posicao : posicao - pontos[i - 1].position, Vector3.up).normalized;
        return true;
    }
    // Escolha única por travessia. Não usa proximidade de postes para adivinhar a direção do sinal.
    public TrafficRoute EncontrarProximaRota()
    {
        if (!Valida) return null;
        bool explicitas = proximasRotas != null && proximasRotas.Length > 0;
        IEnumerable<TrafficRoute> candidatas = explicitas ? proximasRotas :
            (Application.isPlaying ? (IEnumerable<TrafficRoute>)ativas : FindObjectsByType<TrafficRoute>(FindObjectsSortMode.None));
        if (!explicitas && !conectarAutomaticamente) return null;
        TrafficRoute escolhida = null;
        int quantidade = 0;
        Vector3 fim = pontos[pontos.Length - 1].position;
        Vector3 entrada = (fim - pontos[pontos.Length - 2].position).normalized;
        foreach (TrafficRoute candidata in candidatas)
        {
            if (candidata == null || candidata == this || !candidata.isActiveAndEnabled || !candidata.Valida) continue;
            // Mesmo uma conexão explícita precisa de uma trajetória contínua, nunca um salto de 30 m.
            if (Vector3.Distance(fim, candidata.pontos[0].position) > Mathf.Min(1f, raioConexao)) continue;
            Vector3 saida = (candidata.pontos[1].position - candidata.pontos[0].position).normalized;
            if (Vector3.Dot(entrada, saida) < -0.3f) continue;
            if (Random.Range(0, ++quantidade) == 0) escolhida = candidata;
        }
        return escolhida;
    }
    private void OnDrawGizmos()
    {
        if (!Valida) return;
        Gizmos.color = Color.yellow;
        for (int i = 1; i < pontos.Length; i++) Gizmos.DrawLine(pontos[i - 1].position, pontos[i].position);
        if (GetParada(out Vector3 p, out Vector3 d))
        {
            Gizmos.color = Color.red;
            Vector3 lateral = Vector3.Cross(Vector3.up, d) * 2f;
            Gizmos.DrawLine(p - lateral, p + lateral);
        }
        if (saidaCruzamento != null) { Gizmos.color = Color.green; Gizmos.DrawWireSphere(Saida, 1f); }
    }
}
