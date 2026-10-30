# GridShift 6.0 — Entregas jogáveis

## Como jogar

Abra `Assets/Scenes/MainMenu.unity` e inicie o Play Mode. Em uma build, essa é a primeira cena. Novo Jogo começa uma empresa com saldo zero; Continuar restaura um arquivo válido. Configurações usa o painel existente.

Na cidade, pressione **G** para abrir o tablet. No Office, use **E** no computador. Clique em **DEMO** na barra superior para abrir o painel **DEMO LOCAL**, preencha mercadoria, peso, retirada e destino e confirme. O botão Home retorna ao Urban Flow publicado. **ESC** fecha o dispositivo e devolve os controles. Os botões **− / +** ajustam o zoom nativo em passos de 25%, entre 100% e 250%; o valor inicial é 200%.

O painel local cria pedidos reais na simulação: dois carros de 200 kg e o caminhão existente, com 2.500 kg, são adotados do tráfego. Até 200 kg usa carro; acima disso usa caminhão. Os 20 kg ficam registrados como limiar configurável para futura classificação leve, ainda atendida por carros. Não há motos nem veículos adicionais criados pelo sistema de entregas. Sem capacidade ou disponibilidade, o pedido aguarda.

Selecione um pedido no painel para destacar o veículo e o destino. O minimapa mostra apenas a frota em missão, com ícones amarelos e destino ciano; a cidade mostra anéis no veículo e no local selecionados. O painel oferece mapa esquemático das coordenadas locais, estado, histórico, capacidade e velocidade reais. O HUD mantém o dinheiro existente e acrescenta entregas, frota, pedido selecionado e notificações.

## Rotas e pontos

Há 31 locais acessíveis gerados das construções da SampleScene. Cada DeliveryPoint mantém ID, nome, tipo, referência à rota, segmento e VehicleStop. O registro pode ser ajustado no Inspector. A ferramenta **GridShift → Atualização 6.0 → Configurar entregas e menu** reconstrói o catálogo; faça isso fora do Play Mode, com as cenas salvas. Reconstruir pode alterar IDs e exige cuidado com salvamentos antigos.

O gerador projeta paradas no trecho reto de uma rota, verifica piso de rua e volume livre e mantém margem antes do cruzamento/faixa. Candidatos sem conexão dirigida de ida e volta são excluídos. Dijkstra usa exatamente as conexões aceitas pelo tráfego existente, incluindo sentido e continuidade. Quando o destino já ficou para trás, o veículo percorre o circuito necessário. Não há teletransporte para concluir pedidos.

CompanyVehicle orienta TrafficCar por ITrafficGuidance. TrafficCar continua responsável por aceleração, curvas, semáforos, cruzamentos, carros e pedestres. Veículos comuns mantêm sua escolha aleatória. A frota evita reciclagem enquanto pertence à empresa, porém continua dentro da quantidade normal de veículos do TrafficManager. Durante carregamento/descarga o veículo para por dois segundos; se a rota não estiver disponível, aguarda e tenta recalcular.

Recompensa inicial: `20 + pesoKg × 0,03 + percursoEstimadoMetros × 0,25`, configurável no DeliveryManager. É dinheiro fictício. A conclusão legítima registra horário, libera veículo e paga uma única vez. O percurso é uma estimativa sobre os trechos do grafo, e o progresso é aproximado; não é ETA nem distância rodoviária de uma cidade real.

## Persistência

WorldSession mantém a cidade carregada ao entrar no Office: veículos, pedidos, trajetórias, dinheiro e relógio continuam durante a sessão, sem recriação de caminhões.

O arquivo `gridshift-session.json`, em Application.persistentDataPath, guarda dinheiro, histórico, seleção, relógio e pose do jogador na cidade. Há gravação periódica a cada dez segundos e ao sair, com arquivo temporário e cópia anterior `.bak`. Continuar só é habilitado para arquivo legível e validado.

Ao reabrir o aplicativo, **pedidos ainda não concluídos retornam à fila e recebem uma nova atribuição**. O salvamento não restaura a posição física de toda a frota ou as fases de cada cruzamento. Essa limitação não se aplica à viagem cidade/Office dentro da mesma sessão, que preserva as trajetórias em andamento. Local removido de um catálogo posterior deixa o pedido aguardando rota, sem exceção ou destino inventado.

## Integração web: estado real

O usuário informou que **não possui o código do Urban Flow**. A versão publicada em `https://urbanlink-flow.onrender.com` foi apenas inspecionada publicamente. Não houve alteração, publicação, login com credenciais de terceiros ou envio de pedidos ao serviço remoto.

O frontend público usa HTML/JavaScript, login em `/api/login`, Bearer token em `sessionStorage.ul_token` e usuário em `ul_usuario`. O módulo de entregas usa `/api/entregas` (consulta/criação), `/api/entregas/{id}` (alteração), `/api/veiculos` e `/api/rotas`. Os campos observados incluem descricao, origem, destino, peso_kg, prazo_min, data_agendada, veiculo_id e rota_id. Estados observados: pendente, em_andamento, concluida e cancelada. Isso é uma observação do cliente público, **não uma garantia do contrato ou autorização de acesso aos dados**.

Não foi encontrado um contrato para associar contas a uma simulação Unity, publicar os IDs físicos da cidade, reservar pedidos de forma exclusiva ou confirmar eventos idempotentes. O jogo não presume que o login do Chromium esteja disponível no C#. Não armazena senha, não faz login automático e não inventa endpoints de integração.

Por isso, **DEMO LOCAL não está sincronizada com o site publicado**. O documento vem de StreamingAssets, servido exclusivamente em loopback com porta efêmera e caminho aleatório por sessão. Não existe API HTTP de escrita. Comandos de criação/seleção passam pela ponte UWB, restrita à URL local atual e ao token da sessão, com limite de tamanho e validação no gerenciador. A ponte não oferece comandos de pagamento ou conclusão arbitrária. Dados são devolvidos a cada segundo quando o navegador está ativo; recarregar reconecta ao estado existente. O frontend escapa dados com textContent e identifica perda de atualização.

### Para conectar o backend futuramente

É necessário acesso autorizado ao servidor e ao frontend ou uma API oficialmente documentada. O responsável pelo serviço deverá definir:

- Vinculação autenticada entre conta, empresa e sessão de jogo; autenticação separada do C# quando necessária, usando HTTPS e tokens revogáveis de escopo limitado.
- Catálogo versionado de pontos fictícios e IDs estáveis da frota; origem/destino não devem ser confundidos com endereços reais.
- Correspondência entre DeliveryOrder e os campos reais do serviço; valores/status internos não devem ser enviados cegamente ao contrato existente.
- Criação idempotente, reserva exclusiva de pedidos, revisão/ordenação de eventos, confirmação de recebimento e retomada após desconexão.
- Permissões e validação no servidor por usuário/empresa; o cliente não pode concluir entregas ou modificar saldos de outras contas.
- Cadência de posições, limites de requisições, tratamento de expiração de sessão e ambiente de homologação.

Esses itens são requisitos futuros, não endpoints já implementados. A estrutura DeliveryOrder/DeliveryRequest/DeliverySnapshot e o fluxo de estados podem ser reutilizados por um adaptador autenticado depois que o contrato real estiver disponível.

## Apresentação e assets

MainMenu usa uma imagem da cidade renderizada pela Unity, transição GS, fundo com movimento discreto, Novo Jogo, Continuar, Configurações, Créditos, Sair e carregamento assíncrono. O original GS encontrado no projeto tem apenas **32 × 20 pixels**. Ele foi reutilizado sem redesenhar a marca; uma versão original de alta resolução é necessária para acabamento final.

A imagem original Urban Flow não foi fornecida como asset. A captura do WhatsApp não foi recortada nem usada para recriar a marca. Resources/GridShiftBranding aceita os sprites originais GS e Urban Flow. Enquanto urbanFlow estiver vazio, o carregamento do computador/tablet usa o nome textual Urban Flow. Não há placa corporativa com logo aproximada.

O navegador usa zoom real pela API SetZoomLevel do Chromium; cliques continuam convertidos aos pixels nativos da textura. PC e tablet compartilham a instância no Office e a mesma área virtual de **1600 × 1489**, evitando o problema de textura preta observado ao redimensionar esse CEF em execução. Mantém proporção do monitor, rolagem, histórico, entrada/saída de câmera e fade. Não há injeção de CSS para alterar o site público. FOV de interação do PC: 40°.

## Arquivos da atualização

Criados:

- DeliveryData, DeliveryPoint, DeliveryRouting, CompanyVehicle, DeliveryManager.
- DeliveryPersistence e DeliverySaveStore.
- DeliveryPresentation e Traffic/ITrafficGuidance.
- DeliveryDemoServer, GridShiftDeliveryBridge e StreamingAssets/GridShiftDemo/index.html.
- GridShiftBranding, GridShiftBrowserLoading e GridShiftMainMenu.
- Editor/GridShiftV6Setup, GridShiftV6Checks, GridShiftV6NativeChecks e GridShiftV6MenuChecks.
- Assets/Generated/Experience6 (materiais e fundo), Resources/GridShiftBranding e cena MainMenu.

Modificados nesta atualização: TrafficCar, TrafficRoute, TrafficManager, WorldSession, PauseMenu, GridShiftBrowserNavigation, GridShiftUwbTerminal e PortableTablet. Cenas SampleScene e Office configuradas; EditorBuildSettings inclui MainMenu primeiro e TagManager inclui DeliveryMap. As mudanças anteriores 5.0 e a instalação UWB existente foram preservadas.

Prefabs existentes reutilizados: modelos de veículos já configurados no TrafficManager, caminhão Vehicle_Truck_separate da cena, navegador UWB e tablet gerados na atualização anterior. Não foram substituídos o controlador do jogador, pedestres, sinalização ou ciclo de dia/noite.

## Validação

Executada em uma cópia isolada do projeto com Unity 6000.0.83f1, sem controlar a janela do Editor principal ou tocar no salvamento do jogador. Relatórios e capturas ficam em Docs/Validacao6. Consulte os relatórios para distinguir verificações executadas de limitações externas.

- **v6-structure.txt — sucesso:** três cenas sem scripts ausentes, 31 pontos com IDs exclusivos, paradas dentro da faixa e antes do cruzamento, MainMenu como primeira cena da build.
- **v6-simulation.txt — sucesso:** pedidos de 5 e 50 kg atribuídos a carros, 500 kg ao caminhão; recusa de peso negativo/NaN e destino inexistente; recusa de atribuição acima da capacidade; 3.000 kg na fila; reenvio idempotente; coleta/transporte/descarga físicos das três entregas; frota liberada, pagamento único, salvamento e retirada do marcador após conclusão. Foram observadas paradas por semáforo, cruzamento bloqueado, veículo e pedestre. A ida e volta ao Office preservou as mesmas instâncias, relógio e caminhão em movimento.
- **v6-play.txt — sucesso:** Chromium real renderizado, site público acessível, zoom 200% confirmado por devicePixelRatio e viewport CSS, mouse/teclado/Backspace alinhados, FOV do PC, isolamento da ponte em relação ao site público, criação de 500 kg pelo formulário nativo, atribuição ao caminhão e histórico atualizado. Zoom manual 175%/200%, recarregar sem duplicar, interrupção/retomada da ponte com aviso, PC/tablet compartilhados, conclusão física com recompensa refletida no painel e ESC devolvendo controles. Também confirmou Continuar com dinheiro/histórico e o painel de configurações do menu. Nenhuma exceção capturada no teste final.
- **v6-menu-checks.txt — sucesso:** Continuar desabilitado sem arquivo, Créditos/Voltar funcionais, Novo Jogo carrega cidade com saldo zero e relógio às 9h, local removido aguarda rota, gravação/leitura da sessão nova e recusa de arquivo corrompido.

As imagens do menu, computador e tablet são capturas da Unity/CEF. Os testes de teclado usam dispositivos virtuais do Input System na cópia de validação. Um ensaio inicial exigiu esperar a animação de fechar o tablet antes de conferir controles; o teste final faz essa espera. Para a digitação automatizada, aguarda-se a rolagem da página e usa-se cadência de 60 quadros, evitando verificar coordenadas antes de o Chromium terminar de atualizar a tela.

O login remoto com conta real e a sincronização com backend **não foram testados**, pois não há credenciais autorizadas nem código/contrato de integração. Testes da DEMO verificam somente a ponte local com a simulação real. Ainda é recomendável uma sessão manual no Editor principal e uma build Windows no computador final.
