# GridShift — atualização 2.0

## Entrega aplicada

A SampleScene e a Office foram configuradas automaticamente e salvas. Não é necessário abrir nem preparar os veículos individualmente. Caso o editor ainda esteja com a versão anterior em memória, saia do Play Mode e reabra a SampleScene salva.

### Trânsito

TrafficManager inicia com alvo de 15 veículos, incluindo o caminhão já existente, e máximo de 30. A densidade multiplica o alvo inicial (0 a 2), respeitando o máximo e preservando os veículos autorais já presentes na cena. Os 18 prefabs compatíveis foram localizados na pasta real `Assets/SimplePoly City - Low Poly Assets/Prefab/Vehicles/Vehicle with Separated Wheels`. Modelos maiores que o envelope validado do caminhão foram excluídos.

Carros, SUVs, picapes, táxis, veículos de serviço, ônibus e caminhões usam a mesma TrafficCar. Os valores de velocidade, aceleração, frenagem, raio de giro e tempo de reação variam dentro de limites definidos no gerenciador. Os veículos são preparados ao instanciar, sem alterar os prefabs de origem: um Rigidbody cinemático, uma carroceria BoxCollider medida pelos renderers e um único controlador de direção. Colliders redundantes são desativados nas instâncias.

Os spawns usam o primeiro trecho reto das 111 rotas existentes, depois da travessia anterior e antes da próxima retenção. Verificam espaço para a carroceria e folga, objetos sólidos, distância do jogador e concentração de veículos próximos. As partidas são escalonadas. Há geração durante a partida e pooling; a redução de densidade retira veículos gerados fora da visão da câmera do jogador e longe dele. A câmera do minimapa não impede essa redução.

O acompanhamento mantém uma folga mínima acrescida do tempo de reação conforme a velocidade. A velocidade do veículo da frente limita a aproximação. Há uma pequena zona de parada para eliminar rastejamento em filas e um atraso individual de retomada. Os sensores de emergência continuam ativos em cada passo de física.

Os 18 TrafficJunction controlam a entrada nas interseções, incluindo as não semaforizadas. O veículo precisa de saída livre e reserva antes de entrar; a reserva só é liberada quando a traseira inteira passou da saída, ou quando o veículo é desativado. As luzes e os ciclos existentes foram preservados. As escolhas de rota permanecem aleatórias e estáveis durante cada manobra.

### Office e continuidade

CompanyEntrance agora pede a transição a WorldSession. A cidade permanece carregada e ativa; Office é carregada de forma aditiva e descarregada na volta. O trânsito, os sinais e os objetos de dados da cidade continuam os mesmos. Não há restauração por teletransporte dos veículos.

Durante a visita, somente o avatar, as câmeras, os listeners, as interfaces e as luzes de apresentação da cidade são suspensos. O jogador da cidade fica inativo e não recebe comandos. Office mantém seu próprio jogador e exatamente um EventSystem ativo. Na saída, as configurações anteriores da cidade são restauradas e o jogador reaparece no local de entrada. O deslocamento configurável do Office separa suas coordenadas das da cidade durante o carregamento aditivo.

O relógio tem uma fonte única de tempo na sessão. Isso mantém o avanço mesmo quando o componente de interface está dentro do jogador desativado. As interfaces da cidade e do Office consultam o mesmo tempo. O salvamento de horário já existente foi preservado. FadeManager continua sendo o único objeto persistente já usado pelo projeto; não foram espalhados DontDestroyOnLoad pelos veículos ou pelos gerenciadores.

Os dados existentes não são recriados porque a cena da cidade não é descarregada. Esta atualização não implementa um novo sistema de salvar partidas em disco nem inventa um sistema de economia; preserva o estado dos objetos que já existem.

### Primeira pessoa e pulo

O modelo Character 04 tem cabeça e corpo na mesma malha, com olhos/pálpebras separados. Foram geradas variantes locais sem os triângulos da cabeça. A malha animada local mantém 9.052 dos 13.536 triângulos originais e usa os mesmos ossos. A malha original não foi modificada e continua disponível nas outras câmeras.

As layers 7 (PlayerFullBody) e 8 (PlayerLocalBody) separam a versão completa da versão local. A câmera do jogador vê a versão sem cabeça; as outras câmeras veem o modelo original. HUD e sprites não foram recortados. A altura de olhos, o limite de inclinação e a sensibilidade do mouse existentes foram mantidos. A câmera continua na raiz estável do jogador.

PlayerMovement usa o CharacterController existente com uma única chamada de movimento por passo. Um salto consome a possibilidade de pular até haver aterrissagem real, confirmada por contato inferior e superfície com normal caminhável. Espaço no ar, pausa e controles bloqueados não iniciam outro salto. A gravidade é aplicada continuamente, inclusive no quadro do salto, e colisão com teto cancela a subida. Colliders extras no mesmo objeto do CharacterController foram desativados automaticamente.

## Configuração central

No objeto TrafficManager da SampleScene:

- `quantidadeInicial`: alvo inicial total, padrão 15.
- `quantidadeMaxima`: teto de veículos, padrão 30.
- `densidade`: multiplicador do alvo; o caminhão colocado na cena é preservado.
- `intervaloGeracao`: intervalo entre tentativas de repor/ajustar a frota, padrão 2 s.
- `distanciaDoJogador`: distância mínima para aparecer, padrão 18 m.
- `modelos`: lista já preenchida com perfis e limites por tipo de veículo.

Para refazer a configuração após alterações futuras, use **GridShift > Atualização 2.0 > Configurar cidade e jogador**. A ferramenta salva as cenas abertas, reaproveita componentes existentes, recria as variantes locais e configura as duas cenas. A configuração desta entrega já está aplicada.

## Arquivos

Criados:

- Assets/Scripts/Traffic/TrafficManager.cs — população, distribuição, densidade e pooling.
- Assets/Scripts/Traffic/TrafficVehicleFactory.cs — preparação automática e perfis dos veículos.
- Assets/Scripts/Traffic/TrafficJunction.cs — reserva de cruzamentos.
- Assets/Scripts/WorldSession.cs — cenas aditivas, apresentação e relógio da sessão.
- Assets/Scripts/FirstPersonBody.cs — corpo local animado e separação por câmera.
- Assets/Scripts/Editor/GridShiftV2Setup.cs — configuração automática.
- Assets/Scripts/Editor/GridShiftV2Checks.cs — verificações automatizadas em Play Mode para uma cópia de teste.
- Assets/Generated/FirstPerson/*.asset — 18 variantes de malha e respectivos metadados.

Modificados:

- TrafficCar.cs e TrafficRoute.cs — acompanhamento, retomada, reservas e reinicialização de instâncias do pool.
- PlayerMovement.cs — salto único, solo, gravidade e bloqueio dos controles.
- CompanyEntrance.cs — transição aditiva e reativação do prompt.
- ComputerInteraction.cs — ignora comandos da cena inativa e durante transições.
- FadeManager.cs — coordenação do fade com a transição aditiva.
- RealTimeClock.cs — fonte de tempo compartilhada.
- Assets/Scenes/SampleScene.unity e Assets/Scenes/Office.unity — referências e componentes já configurados.
- ProjectSettings/TagManager.asset — duas layers do corpo local/completo.

TrafficLight.cs, TrafficLightGroup.cs, TrafficIntersection.cs e os prefabs originais dos veículos foram preservados.

## Validação e limites

As verificações foram executadas no Unity 6000.0.83f1, em uma cópia isolada do projeto, com Play Mode real para os testes de ciclo de vida, corrotinas, população e cenas aditivas. O produto dessa cópia tem nome próprio para não sobrescrever o PlayerPrefs do jogo principal. Chamadas meteorológicas foram desativadas somente na cópia de testes.

A cobertura inclui geração e variedade, ausência de sobreposição entre veículos, circulação simultânea, fila de três carros no vermelho e retomada progressiva, salto único/aterrissagem/bloqueio/pausa, malha local e ossos compartilhados, três visitas consecutivas ao Office com os mesmos IDs de veículos, avanço do trânsito, dos ciclos dos semáforos e do relógio, preservação dos IDs dos sinais, contagem de gerenciadores, pooling e redução de densidade. Os resultados efetivos da rodada final estão em VALIDACAO_2_0.txt.

As câmeras ficaram desativadas na execução automatizada sem dispositivo gráfico. Portanto, a aparência final do corpo, as animações, a sensação do mouse, os fades e os cliques da interface devem ser conferidos visualmente no Unity. Também é recomendável observar congestionamentos por mais tempo antes de aumentar muito o máximo de veículos. A reserva de um veículo por cruzamento é conservadora e prioriza evitar conflitos.

