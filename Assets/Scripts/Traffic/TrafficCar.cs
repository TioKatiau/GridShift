using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TrafficCar : MonoBehaviour
{
    public static readonly System.Collections.Generic.List<TrafficCar> Active = new System.Collections.Generic.List<TrafficCar>();
    private void OnEnable() { if (!Active.Contains(this)) Active.Add(this); }
    [Header("Rota")]
    [SerializeField] private TrafficRoute rota;
    [Header("Velocidade (m/s), aceleração e frenagem (m/s²)")]
    [SerializeField, Min(0.1f)] private float velocidade = 8f;
    [SerializeField, Min(0.1f)] private float aceleracao = 1.5f;
    [SerializeField, Min(0.1f)] private float frenagem = 3f;
    [SerializeField, Min(0.1f)] private float velocidadeCurva = 3f;
    [SerializeField, Min(1f)] private float raioGiro = 7f;
    [SerializeField, Min(0.1f)] private float distanciaDoPonto = 0.5f;
    [SerializeField, Min(0.1f)] private float velocidadeRotacao = 5f;
    [Header("Segurança")]
    [SerializeField, Min(0.1f)] private float distanciaParada = 1f;
    [SerializeField, Min(1f)] private float distanciaDesaceleracao = 18f;
    [SerializeField, Min(0.1f)] private float distanciaSeguranca = 2f;
    [SerializeField, Min(0.1f)] private float tempoReacao = 0.8f;
    [SerializeField] private LayerMask obstaculos = ~0;
    [Tooltip("BoxCollider sólido envolvendo o veículo inteiro, alinhado ao eixo +Z.")]
    [SerializeField] private BoxCollider carroceria;
    [Tooltip("Desvio máximo do centro da trajetória; ajustar conforme largura útil da faixa.")]
    [SerializeField, Min(0.1f)] private float toleranciaFaixa = 0.75f;
    [SerializeField, Range(0f, 1f)] private float atrasoRetomada = .25f;
    private float contadorRetomada;
    private bool aguardava;
    private TrafficCar veiculoFrente;
    private TrafficJunction reserva;
    private bool reservaAtravessada;
    public enum StopReason { Nenhum, Semaforo, Veiculo, Pedestre, CruzamentoBloqueado, Obstaculo, SemRota }
    [SerializeField] private StopReason motivoParada;
    [SerializeField] private Collider bloqueador;
    public enum DiagnosticState { MOVING,WAITING_TRAFFIC_LIGHT,WAITING_PEDESTRIAN,WAITING_VEHICLE,BLOCKED_INTERSECTION,WAITING_OBSTACLE,INVALID_ROUTE,STUCK,MANEUVERING,REVERSING,RECOVERING_ROUTE,PARKED }
    [SerializeField] private DiagnosticState diagnostic;
    [SerializeField] private string diagnosticDetail;
    TrafficRoute detailRoute; Collider detailBlocker; int detailPoint=-1;StopReason detailReason;string detailGate;
    [SerializeField] bool logStopChanges;
    [SerializeField] private float stationaryTime;
    private float retryAt;
    [Header("Recuperação limitada pela trajetória já percorrida")]
    [SerializeField] bool safeManeuvers=true;
    [SerializeField, Min(.2f)] float reverseSpeed=.65f;
    [SerializeField, Min(.5f)] float maximumReverseDistance=2;
    struct Pose { public Vector3 position; public Quaternion rotation; public int point; }
    readonly System.Collections.Generic.List<Pose> trail=new System.Collections.Generic.List<Pose>();
    bool reversing,correctionNeeded,cooperativeReverse;float reverseDistance,maneuverUntil,maneuverCooldown;int maneuverAttempts;
    public float StationarySeconds=>stationaryTime;
    public Collider Obstacle=>bloqueador;
    public string DiagnosticDetail=>diagnosticDetail+" / "+motivoParada+" / "+gateDetail;
    string gateDetail="";
    public Vector3 BodySize=>metade*2;
    public bool HasParkingGoal=>parkingBay!=null;
    bool NeedsReverseClearance=>reversing||(parkingBay!=null&&!parked&&!leavingParking&&parkingStep<parkingBay.entry.Length&&parkingBay.entry[parkingStep].reverse);
    public bool IsManeuvering=>reversing||correctionNeeded||parkingBay!=null&&!parked;
    TrafficParkingBay parkingBay;TrafficParkingBay.Step[] parkingAbort;float parkingWait;int parkingStep;bool parked,leavingParking;float parkingDeadline;
    public bool RequestParking(TrafficParkingBay bay){
        if(bay==null||corpo==null||Guidance!=null||reserva!=null||parkingBay!=null||velocidadeAtual>.2f||bay.approach==null||Vector3.Distance(corpo.position,bay.approach.position)>.6f||Quaternion.Angle(corpo.rotation,bay.approach.rotation)>8)return false;
        if(!bay.Reserve(this,metade*2))return false;parkingBay=bay;parkingAbort=null;parkingWait=0;parkingStep=0;parked=leavingParking=false;parkingDeadline=Time.time+bay.timeout;return true;
    }
    public bool RetryParking(){if(parkingBay==null||parked)return false;parkingDeadline=Time.time+parkingBay.timeout;return true;}
    public bool LeaveParking(){if(parkingBay==null||!parked)return false;parked=false;leavingParking=true;parkingStep=0;parkingDeadline=Time.time+parkingBay.timeout;return true;}
    bool ParkingStep(float dt){
        if(parkingBay==null)return false;velocidadeAtual=0;
        if(parked){diagnostic=DiagnosticState.PARKED;return true;}if(Time.time>parkingDeadline){diagnostic=DiagnosticState.WAITING_OBSTACLE;diagnosticDetail="Manobra de estacionamento suspensa: tempo limite; conferir trajetória e obstáculo";return true;}
        var steps=parkingAbort??(leavingParking?parkingBay.exit:parkingBay.entry);
        if(parkingStep>=steps.Length){if(leavingParking){parkingBay.Release(this);parkingBay=null;parkingAbort=null;trail.Clear();}else parked=true;return true;}
        var target=steps[parkingStep];if(target.point==null)return true;
        Vector3 delta=Vector3.ProjectOnPlane(target.point.position-corpo.position,Vector3.up);
        if(delta.magnitude<.02f&&Quaternion.Angle(corpo.rotation,target.point.rotation)<4){parkingStep++;return true;}
        float distance=Mathf.Min(.65f*dt,delta.magnitude);
        Vector3 direction=delta.sqrMagnitude>.0001f?delta.normalized:target.point.forward*(target.reverse?-1:1);
        var wanted=target.point.rotation;
        var rotation=Quaternion.RotateTowards(corpo.rotation,wanted,distance/raioGiro*Mathf.Rad2Deg);
        var position=corpo.position+direction*distance;
        if(Vector3.Angle(direction,rotation*Vector3.forward*(target.reverse?-1:1))>12){diagnostic=DiagnosticState.WAITING_OBSTACLE;diagnosticDetail="Trajetória de baliza incompatível com raio de giro";return true;}
        if(!RoadFootprint(position,rotation)||!SweepSafe(position,rotation,distance)){
            diagnostic=DiagnosticState.WAITING_OBSTACLE;parkingWait+=dt;
            if(parkingWait>5&&!leavingParking){
                if(parkingStep==0){parkingBay.Release(this);parkingBay=null;trail.Clear();}
                else {var recover=new System.Collections.Generic.List<TrafficParkingBay.Step>();for(int i=parkingStep-1;i>=0;i--)recover.Add(new TrafficParkingBay.Step{point=parkingBay.entry[i].point});parkingAbort=recover.ToArray();leavingParking=true;parkingStep=0;parkingDeadline=Time.time+parkingBay.timeout;}
                parkingWait=0;
            }return true;
        }
        parkingWait=0;
        diagnostic=target.reverse?DiagnosticState.REVERSING:DiagnosticState.MANEUVERING;corpo.MovePosition(position);corpo.MoveRotation(rotation);return true;
    }
    bool RoadFootprint(Vector3 position,Quaternion rotation){
        for(int i=0;i<4;i++){var at=position+rotation*new Vector3((i%2==0?-1:1)*metade.x,0,(i<2?-1:1)*metade.z);
            if(!Physics.Raycast(at+Vector3.up*.6f,Vector3.down,out var hit,1.2f,obstaculos,QueryTriggerInteraction.Ignore)||Proprio(hit.collider)||!hit.collider.name.StartsWith("Road ")||hit.normal.y<.9f||Mathf.Abs(hit.point.y-position.y)>.12f)return false;
        }return true;
    }
    bool SweepSafe(Vector3 position,Quaternion rotation,float distance){
        float margin=(metade.magnitude+centroLocal.magnitude)*Quaternion.Angle(corpo.rotation,rotation)*Mathf.Deg2Rad;
        var volume=new Vector3(metade.x+.05f+margin,Mathf.Max(.05f,metade.y-.3f),metade.z+.05f+margin);
        if(Ocupado(position+rotation*centroLocal,rotation,volume))return false;
        int n=Physics.BoxCastNonAlloc(corpo.position+corpo.rotation*centroLocal,volume,(position-corpo.position).normalized,impactos,corpo.rotation,distance+.05f,obstaculos,QueryTriggerInteraction.Ignore);
        if(n==impactos.Length)return false;for(int i=0;i<n;i++)if(!Proprio(impactos[i].collider)){bloqueador=impactos[i].collider;return false;}return true;
    }
    void Remember(){if(trail.Count==0||Vector3.Distance(trail[trail.Count-1].position,corpo.position)>.2f){trail.Add(new Pose{position=corpo.position,rotation=corpo.rotation,point=pontoAtual});if(trail.Count>24)trail.RemoveAt(0);}}
    bool ReverseStep(float dt){
        if(!reversing){if(!safeManeuvers||!correctionNeeded||Time.time<maneuverCooldown||maneuverAttempts>=2||trail.Count<2||(reserva!=null&&reserva.PedestrianOccupied)||(autorizado&&reserva==null))return false;
            reversing=true;reverseDistance=0;maneuverUntil=Time.time+4;maneuverAttempts++;}
        if(Time.time>maneuverUntil||reverseDistance>=maximumReverseDistance||trail.Count==0){EndReverse();return true;}
        var target=trail[trail.Count-1];float step=Mathf.Min(reverseSpeed*dt,Vector3.Distance(corpo.position,target.position));
        if(step<.005f){pontoAtual=target.point;trail.RemoveAt(trail.Count-1);return true;}
        var next=Vector3.MoveTowards(corpo.position,target.position,step);var rotation=Quaternion.RotateTowards(corpo.rotation,target.rotation,45*dt);
        var volume=new Vector3(metade.x+.08f,Mathf.Max(.05f,metade.y-.3f),metade.z+.08f);
        float margin=(metade.magnitude+centroLocal.magnitude)*Quaternion.Angle(corpo.rotation,rotation)*Mathf.Deg2Rad;
        volume.x+=margin;volume.z+=margin;
        bool blocked=Ocupado(next+rotation*centroLocal,rotation,volume);
        int count=Physics.BoxCastNonAlloc(corpo.position+corpo.rotation*centroLocal,volume,(next-corpo.position).normalized,impactos,corpo.rotation,step+.1f,obstaculos,QueryTriggerInteraction.Ignore);
        if(count==impactos.Length)blocked=true;
        for(int i=0;i<count;i++)if(!Proprio(impactos[i].collider)){blocked=true;bloqueador=impactos[i].collider;}
        if(!RoadFootprint(next,rotation))blocked=true;
        if(blocked){EndReverse();motivoParada=MotivoObstaculo();return true;}
        corpo.MoveRotation(rotation);corpo.MovePosition(next);reverseDistance+=step;velocidadeAtual=0;return true;
    }
    void EndReverse(){reversing=false;correctionNeeded=false;maneuverCooldown=Time.time+12;velocidadeAtual=0;progressPosition=corpo.position;}
    Vector3 progressPosition;

    public DiagnosticState Diagnostic => diagnostic;
    private void LateUpdate(){
        stationaryTime=velocidadeAtual<.05f?stationaryTime+Time.deltaTime:0;
        if(parkingBay!=null)return;
        diagnostic=reversing?DiagnosticState.REVERSING:correctionNeeded?DiagnosticState.MANEUVERING:motivoParada==StopReason.Semaforo?DiagnosticState.WAITING_TRAFFIC_LIGHT:motivoParada==StopReason.Pedestre?DiagnosticState.WAITING_PEDESTRIAN:motivoParada==StopReason.Veiculo?DiagnosticState.WAITING_VEHICLE:motivoParada==StopReason.CruzamentoBloqueado?DiagnosticState.BLOCKED_INTERSECTION:motivoParada==StopReason.Obstaculo?DiagnosticState.WAITING_OBSTACLE:motivoParada==StopReason.SemRota?DiagnosticState.INVALID_ROUTE:stationaryTime>5?DiagnosticState.STUCK:DiagnosticState.MOVING;
        if(detailRoute!=rota||detailPoint!=pontoAtual||detailBlocker!=bloqueador||detailReason!=motivoParada||detailGate!=gateDetail){
            detailReason=motivoParada;detailGate=gateDetail;
            detailRoute=rota;detailPoint=pontoAtual;detailBlocker=bloqueador;
            diagnosticDetail=(rota!=null?rota.name:"Sem rota")+" / ponto "+pontoAtual+" / "+motivoParada+" / "+gateDetail+(bloqueador!=null?" / "+bloqueador.name:"");if(logStopChanges&&velocidadeAtual<.05f)Debug.Log(name+": "+diagnosticDetail,this);
        }
    }
    public StopReason MotivoParada => motivoParada;
    private Vector3 saidaReservada, direcaoSaida;
    private Rigidbody corpo;
    private int pontoAtual = 1;
    private float velocidadeAtual;
    private bool autorizado, amareloDecidido, passarAmarelo;
    private TrafficRoute proxima;
    private bool escolheu;
    private readonly RaycastHit[] impactos = new RaycastHit[64];
    private readonly Collider[] sobrepostos = new Collider[64];
    private Vector3 centroLocal, metade;
    public float VelocidadeAtual => velocidadeAtual;
    public TrafficRoute RotaAtual => rota;
    public ITrafficGuidance Guidance {get;private set;}
    public int NextPoint=>pontoAtual;bool restoringLogical;
    public bool RestoreLogicalRoute(TrafficRoute route,int next,Vector3 at,Quaternion rotation){if(route==null||!route.Valida||next<1||next>=route.pontos.Length||DistanciaSegmento(at,route.pontos[next-1].position,route.pontos[next].position)>2)return false;LiberarReserva();trail.Clear();restoringLogical=true;rota=route;pontoAtual=next;velocidadeAtual=0;autorizado=amareloDecidido=passarAmarelo=escolheu=false;proxima=null;transform.SetPositionAndRotation(at,rotation);if(corpo!=null){corpo.position=at;corpo.rotation=rotation;}enabled=true;Start();restoringLogical=false;return enabled;}
    public float MaximumSpeed=>velocidade;
    public void SetGuidance(ITrafficGuidance guidance){Guidance=guidance;escolheu=false;proxima=null;}
    public void RefreshGuidance(){escolheu=false;proxima=null;}
    public void ConfigureProfile(BoxCollider box, bool heavy, float maximum, float acceleration, float braking)
    {
        carroceria = box; velocidade = Mathf.Clamp(maximum, 3f, 8f);
        aceleracao = Mathf.Clamp(acceleration, .8f, 2.5f); frenagem = Mathf.Clamp(braking, 3f, 5f);
        velocidadeCurva = heavy ? 2.5f : Random.Range(2.6f, 3.2f);
        raioGiro = heavy ? 7f : 4.5f; distanciaParada = 1f; distanciaDoPonto = .25f;
        distanciaDesaceleracao = 22f; distanciaSeguranca = heavy ? 2.5f : 2f;
        tempoReacao = Random.Range(.9f, 1.3f); atrasoRetomada = Random.Range(.15f, .5f);
    }
    public void ResetRoute(TrafficRoute route)
    {
        LiberarReserva();trail.Clear();reversing=correctionNeeded=false;maneuverAttempts=0;cooperativeReverse=false; rota = route; pontoAtual = 1; velocidadeAtual = 0f;
        autorizado = amareloDecidido = passarAmarelo = escolheu = false; proxima = null;
        aguardava = true; contadorRetomada = 0f; enabled = true;
        var rigidbody = GetComponent<Rigidbody>();
        if (rigidbody != null) { rigidbody.position = transform.position; rigidbody.rotation = transform.rotation; }
        if(gameObject.activeInHierarchy)Start();
    }
    public void SynchronizeSpawnPose(Vector3 position,Quaternion rotation){transform.SetPositionAndRotation(position,rotation);var body=GetComponent<Rigidbody>();if(body!=null){body.position=position;body.rotation=rotation;}Physics.SyncTransforms();}
    private void LiberarReserva() { if (reserva != null) reserva.Release(this); reserva = null; reservaAtravessada = false; }
    private void OnDisable() { Active.Remove(this); LiberarReserva();if(parkingBay!=null){parkingBay.Release(this);parkingBay=null;} }

    private void Start()
    {
        corpo = GetComponent<Rigidbody>();if(GetComponent<GridShiftVehicleLamps>()==null)gameObject.AddComponent<GridShiftVehicleLamps>();
        if (corpo == null) { Falhar("Adicione um Rigidbody cinemático ao veículo."); return; }
        if (carroceria == null) carroceria = GetComponent<BoxCollider>();
        if (rota == null || !rota.Valida || carroceria == null || !carroceria.enabled || carroceria.isTrigger || carroceria.transform != transform)
        {
            Falhar("Atribua rota válida e BoxCollider sólido no mesmo objeto de TrafficCar."); return;
        }
        if (GetComponentsInChildren<Rigidbody>(true).Length != 1)
        { Falhar("Use somente um Rigidbody na raiz do veículo."); return; }
        centroLocal = Vector3.Scale(carroceria.center, transform.lossyScale);
        metade = Vector3.Scale(carroceria.size * 0.5f, transform.lossyScale);
        metade = new Vector3(Mathf.Abs(metade.x), Mathf.Abs(metade.y), Mathf.Abs(metade.z));
        corpo.isKinematic = true;
        corpo.useGravity = false;
        corpo.interpolation = RigidbodyInterpolation.Interpolate;
        corpo.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        // Preserva a posição de spawn; nunca teletransporta para outra rua.
        if (!restoringLogical && DistanciaSegmento(transform.position, rota.pontos[0].position, rota.pontos[1].position) > toleranciaFaixa)
        { Falhar("Spawn fora do primeiro segmento da rota. Reposicione o veículo e alinhe seu +Z com a faixa."); return; }
        if (rota.GetSemaforo() != null && (!rota.GetParada(out _, out _) || !rota.TemSaidaCruzamento))
        { Falhar("Rota sem linha de parada ou saída de cruzamento configurada."); }
    }
    private void Falhar(string mensagem)
    { velocidadeAtual = 0f; motivoParada=StopReason.SemRota;diagnostic=DiagnosticState.INVALID_ROUTE;diagnosticDetail=mensagem;Debug.LogError(name + ": " + mensagem, this); enabled = false; }
    private void EscolherSaida()
    {
        if (escolheu && (proxima != null || Time.time < retryAt)) return;
        proxima = Guidance!=null?Guidance.NextRoute(rota):rota.EncontrarProximaRota(); escolheu = true;
        retryAt=Time.time+2;
    }
    private static float DistanciaSegmento(Vector3 p, Vector3 a, Vector3 b)
    {
        p.y = a.y = b.y = 0f;
        Vector3 ab = b - a;
        return Vector3.Distance(p, a + ab * Mathf.Clamp01(Vector3.Dot(p - a, ab) / Mathf.Max(0.0001f, ab.sqrMagnitude)));
    }
    private float LimiteParada(float distancia)
    { return Mathf.Sqrt(2f * frenagem * Mathf.Max(0f, distancia)); }
    private bool Proprio(Collider c) => c.attachedRigidbody == corpo || c.transform.IsChildOf(transform);
    private StopReason MotivoObstaculo() => bloqueador != null && (bloqueador.GetComponentInParent<PedestrianAgent>() != null || bloqueador.GetComponentInParent<PlayerMovement>() != null) ? StopReason.Pedestre :
        bloqueador != null && bloqueador.GetComponentInParent<TrafficCar>() != null ? StopReason.Veiculo : StopReason.Obstaculo;
    private bool Ocupado(Vector3 centro, Quaternion rotacao, Vector3 tamanho,bool flowingExit=false)
    {
        int n = Physics.OverlapBoxNonAlloc(centro, tamanho, sobrepostos, rotacao, obstaculos, QueryTriggerInteraction.Ignore);
        if (n == sobrepostos.Length) return true; // Buffer cheio: segurança conservadora.
        for (int i = 0; i < n; i++) if (!Proprio(sobrepostos[i])) {var leader=sobrepostos[i].GetComponentInParent<TrafficCar>();if(flowingExit&&rota.Cruzamento!=null&&rota.Cruzamento.MovingLeader(leader,this))continue;bloqueador = sobrepostos[i]; return true; }
        return false;
    }
    private float EspacoFrente(Vector3 direcao, Quaternion rotacao, float alcance)
    {
        veiculoFrente = null;
        Vector3 centro = corpo.position + rotacao * centroLocal;
        // Sensor elevado acima do solo; mantém a largura e o comprimento do caminhão.
        Vector3 sensor = new Vector3(metade.x + 0.05f, Mathf.Max(0.05f, metade.y - 0.3f), metade.z);
        if (Ocupado(centro, rotacao, sensor)) return 0f;
        int n = Physics.BoxCastNonAlloc(centro, sensor, direcao, impactos, rotacao, alcance, obstaculos, QueryTriggerInteraction.Ignore);
        if (n == impactos.Length) return 0f;
        float livre = alcance;
        for (int i = 0; i < n; i++) if (!Proprio(impactos[i].collider) && impactos[i].distance < livre)
        {
            livre = impactos[i].distance;
            bloqueador = impactos[i].collider;
            var rigidbody = impactos[i].collider.attachedRigidbody;
            veiculoFrente = rigidbody != null ? rigidbody.GetComponent<TrafficCar>() : null;
        }
        return livre;
    }
    private void FixedUpdate()
    {
        if (corpo == null || rota == null || !rota.Valida || pontoAtual>=rota.pontos.Length || rota.pontos[pontoAtual]==null || rota.pontos[pontoAtual-1]==null) {
            motivoParada=StopReason.SemRota;velocidadeAtual=0;
            if(corpo!=null&&rota!=null&&reserva==null&&Time.time>=retryAt){retryAt=Time.time+2;
                if(TrafficRoute.Continuation(rota,corpo.position,corpo.rotation*Vector3.forward,out var recovered,out var index)){rota=recovered;pontoAtual=index;escolheu=autorizado=amareloDecidido=passarAmarelo=false;proxima=null;}}
            return;
        }
        if(Guidance is CompanyVehicle companyVehicle&&!companyVehicle.CanDrive){velocidadeAtual=0;diagnostic=DiagnosticState.PARKED;diagnosticDetail="Frota: combustível, manutenção ou motorista indisponível";return;}
        float dt = Time.fixedDeltaTime;
        if(Vector3.Distance(progressPosition,corpo.position)>8){maneuverAttempts=0;cooperativeReverse=false;progressPosition=corpo.position;}
        if(ParkingStep(dt))return;
        if(ReverseStep(dt))return;
        if(correctionNeeded&&maneuverAttempts>=2&&Time.time>=maneuverCooldown&&TrafficRoute.Continuation(rota,corpo.position,corpo.rotation*Vector3.forward,out var alternative,out var alternativePoint)){
            rota=alternative;pontoAtual=alternativePoint;proxima=null;escolheu=false;correctionNeeded=false;trail.Clear();diagnostic=DiagnosticState.RECOVERING_ROUTE;
        }
        Remember();
        motivoParada = StopReason.Nenhum; bloqueador = null;gateDetail="";
        Vector3 pos = corpo.position;
        // A traseira pode sair somente na rota seguinte. O compromisso da reserva
        // não pode ser apagado junto ao estado do semáforo ao trocar de rota.
        if (reserva != null && reservaAtravessada)
        {
            Vector3 centro = pos + corpo.rotation * centroLocal;
            float projecao = Mathf.Abs(Vector3.Dot(direcaoSaida, corpo.rotation * Vector3.right)) * metade.x +
                Mathf.Abs(Vector3.Dot(direcaoSaida, corpo.rotation * Vector3.forward)) * metade.z;
            // The clearance point already includes the crossing's physical safety margin.
            // Adding the following-distance margin again traps long vehicles on short blocks.
            if (Vector3.Dot(centro - saidaReservada, direcaoSaida) - projecao > .1f) LiberarReserva();
        }
        // Só avança pontos que foram efetivamente alcançados; não corta a esquina com lookahead longo.
        Vector3 segmentDirection=Vector3.ProjectOnPlane(rota.pontos[pontoAtual].position-rota.pontos[pontoAtual-1].position,Vector3.up).normalized;
        bool passed=Vector3.Dot(pos-rota.pontos[pontoAtual].position,segmentDirection)>=0&&DistanciaSegmento(pos,rota.pontos[pontoAtual-1].position,rota.pontos[pontoAtual].position)<=toleranciaFaixa;
        if (passed||Vector3.ProjectOnPlane(rota.pontos[pontoAtual].position - pos, Vector3.up).magnitude <= distanciaDoPonto)
        {
            if (pontoAtual < rota.pontos.Length - 1) pontoAtual++;
            else
            {
                EscolherSaida();
                if (proxima == null) { velocidadeAtual = 0f; motivoParada = StopReason.SemRota; return; }
                rota = proxima; pontoAtual = 1; escolheu = false;trail.Clear();
                autorizado = amareloDecidido = passarAmarelo = false;
                if (rota.GetSemaforo() != null && (!rota.GetParada(out _, out _) || !rota.TemSaidaCruzamento))
                { Falhar("Configure retenção e saída na rota " + rota.name); return; }
            }
        }
        Vector3 alvo = rota.pontos[pontoAtual].position;
        Vector3 direcao = Vector3.ProjectOnPlane(alvo - pos, Vector3.up);
        float distanciaAlvo = direcao.magnitude;
        if (distanciaAlvo < 0.001f) return;
        direcao /= distanciaAlvo;
        float desejada = Mathf.Min(velocidade, rota.VelocidadeMaxima);
        float curva = Vector3.Angle(corpo.rotation * Vector3.forward, direcao);
        float ateCurva = distanciaAlvo;
        // Procura curvas adiante para frear antes, inclusive em rotas com muitos pontos.
        for (int i = pontoAtual; i < rota.pontos.Length - 1; i++)
        {
            Vector3 anterior = i == pontoAtual ? direcao : (rota.pontos[i].position - rota.pontos[i - 1].position).normalized;
            Vector3 seguinte = (rota.pontos[i + 1].position - rota.pontos[i].position).normalized;
            if (Vector3.Angle(anterior, seguinte) > 1f)
                desejada = Mathf.Min(desejada, Mathf.Sqrt(velocidadeCurva * velocidadeCurva + 2f * frenagem * Mathf.Max(0f, ateCurva - raioGiro)));
            ateCurva += Vector3.Distance(rota.pontos[i].position, rota.pontos[i + 1].position);
            if (ateCurva > distanciaDesaceleracao + velocidade * velocidade / (2f * frenagem)) break;
        }
        if (curva > 10f) desejada = Mathf.Min(desejada, velocidadeCurva);
        float limitePasso = float.PositiveInfinity;
        bool entradaPermitida = false;
        Vector3 linha = Vector3.zero, sentido = Vector3.forward;
        TrafficLight sinal = rota.GetSemaforo();
        if ((sinal != null || rota.Cruzamento != null) && rota.GetParada(out linha, out sentido))
        {
            Vector3 frente = pos + corpo.rotation * (centroLocal + Vector3.forward * metade.z);
            float ateLinha = Vector3.Dot(linha - frente, sentido);
            if (!autorizado)
            {
                float livre = ateLinha - distanciaParada;
                bool amarelo = sinal != null && sinal.EstaAmarelo();
                if (!amarelo) amareloDecidido = false;
                if (amarelo && !amareloDecidido)
                {
                    passarAmarelo = livre < velocidadeAtual * velocidadeAtual / (2f * frenagem) + velocidadeAtual * tempoReacao;
                    amareloDecidido = true;
                }
                Quaternion orientacaoSaida = Quaternion.LookRotation((rota.pontos[rota.pontos.Length - 1].position - rota.pontos[rota.pontos.Length - 2].position).normalized);
                // A base das rodas pode tocar o asfalto; o piso não bloqueia a saída.
                Vector3 sensorSaida = new Vector3(metade.x + 0.05f, Mathf.Max(0.05f, metade.y - 0.3f), metade.z + distanciaSeguranca);
                bool saidaLivre = rota.TemSaidaCruzamento && !Ocupado(rota.Saida + orientacaoSaida * new Vector3(0f, centroLocal.y, metade.z + distanciaSeguranca), orientacaoSaida, sensorSaida,true);
                bool sinalPermite = sinal == null || sinal.PodePassar() || (amarelo && passarAmarelo);
                if (!sinalPermite && reserva != null && !reservaAtravessada) LiberarReserva();
                gateDetail="sinal="+sinalPermite+" saída="+saidaLivre+" bloqueioSaída="+(bloqueador!=null?bloqueador.name:"nenhum")+" faixa="+(rota.Cruzamento!=null&&rota.Cruzamento.PedestrianOccupied)+" reservas="+(rota.Cruzamento!=null?rota.Cruzamento.VehicleCount:0);
                bool pode = sinalPermite && saidaLivre;
                if (pode && rota.Cruzamento != null)
                {
                    pode = rota.Cruzamento.Available(this);
                    if (pode && ateLinha < Mathf.Max(distanciaParada + 2f, velocidadeAtual * velocidadeAtual / (2f * frenagem) + distanciaParada))
                    {
                        pode = (reserva == null || reserva == rota.Cruzamento) && rota.Cruzamento.TryEnter(this);
                        if (pode) { reserva = rota.Cruzamento; saidaReservada = rota.Cruzamento.ClearancePoint(orientacaoSaida * Vector3.forward); direcaoSaida = orientacaoSaida * Vector3.forward; }
                    }
                }
                if (!pode)
                { motivoParada = rota.Cruzamento != null && rota.Cruzamento.PedestrianOccupied ? StopReason.Pedestre : !sinalPermite ? StopReason.Semaforo : StopReason.CruzamentoBloqueado;
                  desejada = Mathf.Min(desejada, LimiteParada(livre)); limitePasso = Mathf.Max(0f, livre); }
                // Compromisso apenas na entrada legal; mudanças posteriores de luz não prendem o veículo no meio.
                else entradaPermitida = true;
            }
        }
        float alcance = Mathf.Max(distanciaDesaceleracao, velocidadeAtual * velocidadeAtual / (2f * frenagem) + velocidadeAtual * tempoReacao + distanciaSeguranca);
        // A projeção reta termina na próxima curva: não sonda a calçada atrás dela.
        float sensorReach=alcance,along=distanciaAlvo;
        for(int i=pontoAtual;i<rota.pontos.Length-1&&along<alcance;i++){
            Vector3 segment=(rota.pontos[i+1].position-rota.pontos[i].position).normalized;
            if(Vector3.Angle(corpo.rotation*Vector3.forward,segment)>12){sensorReach=Mathf.Min(sensorReach,Mathf.Max(.5f,along-metade.z));break;}
            along+=Vector3.Distance(rota.pontos[i].position,rota.pontos[i+1].position);
        }
        float espaco = EspacoFrente(corpo.rotation * Vector3.forward, corpo.rotation, sensorReach);
        // O fim da região sondada não é um obstáculo. A varredura do passo continua obrigatória.
        if(espaco>=sensorReach-.001f){espaco=alcance;veiculoFrente=null;}
        if (veiculoFrente != null)
        {
            if(veiculoFrente.NeedsReverseClearance&&!veiculoFrente.cooperativeReverse&&velocidadeAtual<.05f&&stationaryTime>1&&espaco<distanciaSeguranca+1&&maneuverAttempts<1){correctionNeeded=true;cooperativeReverse=true;}

            float folga = espaco - distanciaSeguranca - velocidadeAtual * tempoReacao;
            desejada = Mathf.Min(desejada, Mathf.Max(0f, veiculoFrente.VelocidadeAtual + folga / tempoReacao));
            // Zona morta impede que uma fila parada continue rastejando por erro numérico.
            if (veiculoFrente.VelocidadeAtual < .05f && espaco <= distanciaSeguranca + .15f) desejada = 0f;
        }
        float disponivel = Mathf.Max(0f, espaco - distanciaSeguranca);
        if (LimiteParada(disponivel) < desejada || (veiculoFrente != null && espaco < distanciaSeguranca + velocidadeAtual * tempoReacao + .2f))
            motivoParada = veiculoFrente != null ? StopReason.Veiculo : MotivoObstaculo();
        desejada = Mathf.Min(desejada, LimiteParada(disponivel));
        limitePasso = Mathf.Min(limitePasso, disponivel);
        // Freia também no fim da rota; não entra num vão sem trajetória autorizada.
        float restante = distanciaAlvo;
        for (int i = pontoAtual; i < rota.pontos.Length - 1; i++) restante += Vector3.Distance(rota.pontos[i].position, rota.pontos[i + 1].position);
        if (restante < alcance)
        {
            EscolherSaida();
            if (proxima == null) desejada = Mathf.Min(desejada, LimiteParada(Mathf.Max(0f, restante - distanciaDoPonto * 0.5f)));
            else desejada = Mathf.Min(desejada, Mathf.Sqrt(proxima.VelocidadeMaxima * proxima.VelocidadeMaxima + 2f * frenagem * restante));
        }
        if(Guidance!=null){float stop=Guidance.DistanceToStop(rota,pontoAtual,pos);if(!float.IsPositiveInfinity(stop)){float remaining=Mathf.Max(0,stop);desejada=Mathf.Min(desejada,LimiteParada(Mathf.Max(0,remaining-.03f)));limitePasso=Mathf.Min(limitePasso,remaining);}}
        if (desejada <= .01f) aguardava = true;
        else if (aguardava) { contadorRetomada = atrasoRetomada; aguardava = false; }
        if (contadorRetomada > 0f) { contadorRetomada -= dt; desejada = 0f; }
        velocidadeAtual = Mathf.MoveTowards(velocidadeAtual, desejada, (desejada < velocidadeAtual ? frenagem : aceleracao) * dt);
        float passo = Mathf.Min(velocidadeAtual * dt, Mathf.Min(limitePasso, distanciaAlvo));
        if (passo <= 0.00001f) { velocidadeAtual = 0f; return; }
        float graus = Mathf.Min(velocidadeRotacao * 30f * dt, passo / raioGiro * Mathf.Rad2Deg);
        Quaternion rotacao = Quaternion.RotateTowards(corpo.rotation, Quaternion.LookRotation(direcao), graus);
        Vector3 destino = pos + rotacao * Vector3.forward * passo;
        if (DistanciaSegmento(destino, rota.pontos[pontoAtual - 1].position, alvo) > toleranciaFaixa)
        { velocidadeAtual=0;motivoParada=StopReason.Obstaculo;correctionNeeded=true;
          diagnosticDetail="Curva sem espaço: "+rota.name+" / "+pontoAtual+"; tentativa "+maneuverAttempts;return; }
        // Varredura da translação e volume conservador cobrindo a rotação inteira neste passo.
        float margemRotacao = (metade.magnitude + centroLocal.magnitude) * Quaternion.Angle(corpo.rotation, rotacao) * Mathf.Deg2Rad;
        Vector3 volume = metade + Vector3.one * margemRotacao;
        volume.y = Mathf.Max(0.05f, metade.y - 0.3f);
        if (Ocupado(destino + rotacao * centroLocal, rotacao, volume)) { motivoParada = MotivoObstaculo(); velocidadeAtual = 0f; return; }
        float livrePasso = EspacoFrente((destino - pos).normalized, rotacao, passo + 0.05f);
        if (livrePasso + 0.0001f < passo + 0.05f) { motivoParada = MotivoObstaculo(); velocidadeAtual = 0f; return; }
        if (entradaPermitida && Vector3.Dot(destino + rotacao * (centroLocal + Vector3.forward * metade.z) - linha, sentido) >= 0f)
        { autorizado = true; if (reserva != null) reservaAtravessada = true; }
        corpo.MoveRotation(rotacao);
        corpo.MovePosition(destino);
    }
    private void OnValidate()
    {
        velocidade = Mathf.Max(0.1f, velocidade); aceleracao = Mathf.Max(0.1f, aceleracao);
        frenagem = Mathf.Max(0.1f, frenagem); raioGiro = Mathf.Max(1f, raioGiro);
        distanciaDoPonto = Mathf.Max(0.05f, distanciaDoPonto);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = motivoParada == StopReason.Nenhum ? Color.green : Color.red;
        if (carroceria != null) { Gizmos.matrix = transform.localToWorldMatrix; Gizmos.DrawWireCube(carroceria.center, carroceria.size); Gizmos.matrix = Matrix4x4.identity; }
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + transform.forward * distanciaDesaceleracao);
        if (bloqueador != null) Gizmos.DrawWireCube(bloqueador.bounds.center, bloqueador.bounds.size);
    }
}




