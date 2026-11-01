# GridShift 15.0 — entregas pessoais e descarga física

Implementação incremental em 01/11/2026. Branch `codex/gridshift-15-active-deliveries`; recuperação anterior em `gridshift-15-recovery-20261101`. A alteração preexistente da fonte LiberationSans não integra este trabalho. Sem push e sem remoção de saves pessoais.

## Como jogar

Abra o tablet com G e vá a Entregas. Nos contratos leves com acesso físico disponível, escolha **Entrega pessoal**. Também é possível escolher a modalidade no formulário Criar entrega. Cargas pesadas continuam destinadas à frota; só uma encomenda pessoal pode ficar ativa por vez.

O marcador indica a coleta. Aproxime-se, olhe para o ponto e pressione E para pegar. A caixa aparece à frente da câmera, acompanha a caminhada e reduz moderadamente a velocidade pelo peso. O destino passa a ser mostrado no mapa e no mundo. No acesso correto, E confirma a entrega, aplica dinheiro e reputação reais e remove o acompanhamento. A pausa e o tablet continuam disponíveis; ocultar o pacote durante uma interface não elimina a carga lógica.

A missão opcional **Entrega Expressa** acompanha aceitação, coleta, transporte e confirmação da recompensa. Seu pedido é identificado por ID e sua conclusão depende de uma entrega pessoal concluída, não de avanço manual de contador.

Na modalidade Frota, os veículos continuam usando as rotas e paradas existentes. Ao terminar o tempo inicial de descarga, o pedido permanece em Descarregando enquanto o entregador caminha até o recebimento, confirma a entrega e retorna. O pagamento só ocorre após o retorno e o embarque simplificado. Não foi adicionado outro gerenciador de pedidos nem outro sistema financeiro.

## Implementação

- `DeliveryMode` distingue frota e jogador no mesmo DeliveryOrder. O despacho automático ignora pedidos pessoais.
- O pacote reutilizável está em `Assets/Resources/DeliveryPackage.prefab`, com materiais próprios simples de papelão e lacre amarelo.
- DeliveryPoint ganhou pedestrianAccess, separado da parada na rua. Foram vinculadas 64 áreas de recebimento aos pontos de calçada existentes com chão verificado. Isso não significa que 64 portas de modelos foram verificadas manualmente.
- Interação exige alcance, cena ativa e linha de visão; a seleção dá preferência à direção observada. O próprio corpo do jogador é ignorado na consulta de visão, mas paredes continuam bloqueando.
- O pacote usa apresentação suavizada, balanço, recolhimento perto de obstáculos e transição de entrega. Não possui colisão capaz de empurrar o jogador.
- O entregador usa o grafo e a navegação de pedestres existentes. O controle externo impede destinos aleatórios, entrada automática em prédios e reciclagem durante uma entrega. Recalcular mantém o destino operacional.
- A passagem inicial verifica apoio, meio-fio, espaço corporal e ligação ao passeio. Rotas ou saídas sem validação deixam a descarga pendente com diagnóstico, em vez de concluir sem entrega.
- Operações distantes mantêm navegação leve sem instanciar imediatamente o modelo completo. O visual é criado quando o jogador se aproxima e reaproveitado pelo veículo; fica desativado à distância. Não há benchmark global de FPS nesta entrega.

## Tablet e histórico

A superfície usa 98,5% da área útil com escala uniforme, resolução lógica menor para ampliar a leitura, menu lateral mais compacto e conteúdo mais largo. Tablet e computador continuam compartilhando a mesma central nativa.

Entregas agora possui **ENTREGAS DISPONÍVEIS**, **ENTREGAS EM ANDAMENTO** e **ENTREGAS CONCLUÍDAS**. O histórico mostra responsável, destino, calendário disponível e valor recebido. Não cria botão Acompanhar para concluídas. A seleção do gerenciador também recusa pedidos encerrados. O histórico tem filtro por tipo e páginas de 12 registros, ordenadas da conclusão mais recente para a mais antiga.

Saves antigos não possuíam a data no calendário: o histórico informa essa ausência em vez de inventar uma data. As listas continuam recebendo as mudanças reais do gerenciador sem exigir fechar o tablet.

## Persistência

Os novos saves usam versão 4. Versões 1, 2 e 3 continuam legíveis; modalidade ausente significa Frota. O formato novo impede que uma versão antiga do jogo trate uma encomenda pessoal como um pedido automático.

A etapa pessoal identifica se a caixa está aguardando coleta ou sendo transportada. Os estados de descarga guardam fase, nó e posição lógica do entregador no próprio pedido. O carregamento recria a apresentação a partir desses dados. Pagamentos continuam usando o ID financeiro único da entrega e o indicador rewardPaid. A validação adicional rejeita fases inválidas, posições não finitas e recompensa paga em pedido ainda ativo.

## Arquivos alterados

Scripts existentes: CompanyVehicle, DeliveryData, DeliveryManager, DeliveryPersistence, DeliveryPoint, DeliveryPresentation, DeliverySaveStore, GridShiftMapMarkers, GridShiftMissions, GridShiftNativeOperations, GridShiftProfiles, GridShiftSaveValidation, InteractionFocus, PedestrianAgent, PedestrianManager e PlayerMovement. O teste GridShiftV14Checks explicita a criação de um save v3 para manter a regressão de compatibilidade.

Scripts novos: GridShiftPersonalDelivery, GridShiftCourierDelivery, Editor/GridShiftV15Setup, Editor/GridShiftV15Checks e Editor/GridShiftV15PoseChecks.

Cena alterada: SampleScene, para referências de recebimento. MainMenu e Office não precisam de novos vínculos manuais. Prefab novo: DeliveryPackage. Materiais novos: PackageCardboard e PackageTape. Nenhum prefab funcional anterior foi substituído.

## Evidências e limites

Os relatórios executados e capturas ficam em `Docs/Validacao15`. A execução usa cópia integral dos assets, Unity 6000.0.83f1 e DX11, com renderização. Pacotes locais compatíveis: URP 17.0.4, Unity UI 2.0.0 e Input System 1.20.0. Manifest e PackageCache originais não foram editados. Essa composição não equivale a todos os editores/pacotes possíveis.

Foram exercitados coleta real, impedimento por parede e carga pesada, caixa na câmera, save durante transporte, recarregamento real da cidade com uma única caixa, caminhada pelo CharacterController até o destino, entrega, dinheiro, missão, idempotência de pagamento, histórico nativo e tablet. O ponto inicial de coleta é posicionado pelo teste; o deslocamento da coleta ao destino é caminhado pelo controlador real, dirigido pelo roteiro de teste.

O roteiro chama os mesmos comandos de coleta/entrega usados pela interação e abre o tablet pela API existente. Não é uma sessão manual nem prova de acionamento físico das teclas E/G. A pequena transição de colocar o pacote precisa de conferência manual adicional, pois o teste de pagamento chama diretamente o comando do gerenciador.

Também houve entrega pesada usando o caminhão real e o trânsito da cidade, descarga pelo grafo de pedestres, gravação/recarregamento durante o percurso do entregador, retorno e conclusão antes do pagamento. Isso comprova o cenário registrado, não todos os 64 destinos. O relatório mais recente contém o resultado da conferência visual próxima.

`compatibility.txt` registra 15 verificações aprovadas de movimento e armazenamento, incluindo leitura v1/v3 e recuperação por backup. `pose.txt` registra a pose no modelo real isolado: a mão passou de (-0,26; 0,79; -0,14) ao apoio (-0,15; 1,04; 0,38), com diferença residual de aproximadamente 0,00000012 m. A pose usa os ossos vinculados ao mesh renderizado, evitando alterar uma hierarquia que não controla a aparência. A captura isolada não é uma cena jogável; `courier.png` é a conferência no cenário da cidade.

Permanecem pendentes: validação de múltiplos destinos e várias operações consecutivas, revisão manual das portas e de todas as saídas de veículos, novas vagas de carga/descarga, animações próprias de mãos e portas, ajuda do jogador na descarga, novos estabelecimentos e demais missões solicitadas, reformulação financeira completa, barreiras adicionais e teste de todas as bordas, teste prolongado de desempenho e build 15.0. O projeto mantém os limites e a recuperação de queda anteriores; este lote não comprova todas as bordas.

O motorista usa caminhada existente e pose procedural dos braços no rig humanoide para apoiar a caixa. Essa alternativa não exige um novo controller nem substitui as pernas animadas. Saída e embarque são simplificados no lado externo do veículo; não foram inventados rigs de portas. Ainda não há clipes próprios de abrir portas ou manipulação detalhada dos dedos. Não há alegação de qualidade final de animação ou mixagem auditiva.

Prioridade seguinte: validar e ajustar recebimentos/estacionamento em vários bairros, depois permitir ajuda na descarga sobre a mesma carga e ampliar as atividades urbanas. As cinco fases completas não estão encerradas.
