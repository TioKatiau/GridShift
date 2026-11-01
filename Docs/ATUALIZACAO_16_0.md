# GridShift 16.0 — primeiro ciclo van + depósito

Data: 01/11/2026. Implementação incremental sobre a versão 15, na branch `codex/gridshift-16-van-depot`. Recuperação anterior: tag `gridshift-16-recovery-20261101`. Sem push. A fonte LiberationSans com alteração preexistente foi preservada fora deste trabalho.

As 100 ideias estão registradas em `Docs/Expansao16/OBJETIVOS.md` e `backlog.json`, com fase, dependências, escopo e evidência. Neste lote, oito ideias têm cenários testados, 25 estão parciais e 67 não foram iniciadas. Isso não significa que as oito fases ou todas as partes das ideias parciais foram concluídas.

## Como acessar a jogabilidade nova

1. Abra o tablet com G, entre em **Equipamentos** e use **Localizar depósito no mapa**.
2. Vá ao pátio de expedição, junto à área comercial/industrial existente. O quadro permite aceitar um pedido pessoal real de oito quilos.
3. Use as estações com a tecla de interação configurada: conferir código, escolher embalagem, imprimir etiqueta, separar no setor indicado e liberar expedição.
4. Retire a caixa da prateleira e leve ao compartimento traseiro do furgão.
5. Aproxime-se de uma das laterais e entre. WASD controla aceleração/ré e direção; a ação de pulo, Espaço por padrão, freia. A ação de interação, E por padrão, desembarca somente com o veículo parado e espaço livre.
6. Estacione, saia, retire a encomenda e conclua a entrega no acesso correto. O pedido usa os mesmos pagamentos, reputação e histórico da empresa.
7. A missão **Operador de Expedição** acompanha as ações e oferece confirmação de bônus após a conclusão efetiva. O histórico permite abrir o comprovante da operação.

O tablet continua bloqueando a caminhada durante interação; a pausa continua disponível ao dirigir. Durante condução, o HUD do personagem é preservado e ocultado, e um painel contextual 3D informa velocidade, combustível, condição e carga. Ao desembarcar, câmera, colisores, controles e HUD anteriores são restaurados.

## Veículo pessoal

O furgão inicial é uma adaptação explícita da picape SimplePoly já existente, com carroceria fechada simples, materiais próprios e faixa amarela. Não é um modelo novo de van com interior e portas animadas. O prefab é `Assets/Resources/GridShiftPlayerVan.prefab`.

O veículo possui Rigidbody dinâmico, collider simples medido pelo modelo, aceleração, ré, frenagem, direção e colisões. A condução é uma solução inicial para superfícies predominantemente planas; ainda não há suspensão por WheelCollider, simulação completa de pneus ou certificação de todos os declives da cidade.

Não há TrafficCar nem CompanyVehicle autônomo no veículo pessoal. Assim, não é necessário desligar ou alterar a IA dos demais carros. A colisão usa a camada TrafficCar existente, mantendo os sensores do trânsito capazes de reconhecer a van como obstáculo.

Entrada, saída e compartimento são calculados pelas dimensões reais do collider. Não se usa uma posição fixa que possa ficar dentro da lataria. O desembarque verifica piso, corpo livre e trajeto sem paredes, ignorando apenas o próprio veículo e o avatar avaliado.

Combustível, desgaste e manutenção usam CompanyVehicleHealth no GridShiftCompany, com ID `GS-VAN01`. Distância consome combustível e condição pelas mesmas taxas empresariais existentes. Abastecimento e manutenção chamam o financeiro real; custos e descontos do mecânico são reaproveitados. O áudio usa Car_Engine_Loop e o canal Vehicles já existentes.

O compartimento leva a encomenda pessoal ativa e valida a capacidade nominal de 80 kg. O limite pessoal atual continua em 20 kg e um pacote por vez; múltiplas encomendas, mochila e equipamentos ampliados permanecem pendentes. A carga guardada não pode ser entregue remotamente. É necessário retirá-la com a van estacionada.

Há checkpoint de piso seguro para recuperar uma queda excepcional fora do mundo, preservando a carga. Isso não substitui a implantação e o teste de novos limites físicos em todas as bordas.

## Depósito interativo

O pátio utiliza uma área existente, com verificação de piso e espaço de 9,6 × 7,6 m, sem sobrepor centros de trajetórias rodoviárias. A posição configurada está próxima de (339,95; -4,57; 221,32). Bancada, leitor, impressora, duas prateleiras, marcação da vaga, quadro e estações são apresentados por GridShiftActiveOperations a partir do asset GridShiftActiveConfig.

Foi acrescentado um ponto `GS-DEPOT16` à SampleScene. Ele é pessoal: não entra nas ofertas automáticas da frota nem pode ser escolhido para um despacho automático. Sua ligação de referência reaproveita uma rota existente, sem alterar vias, conexões ou reservas do trânsito. A integração de docas reais para caminhões é uma etapa posterior.

O scanner confere o identificador da caixa física, `PKG-<ID do pedido>`. Código inexistente, embalagem incompatível, separação em setor errado e coleta antes da liberação são recusados com motivo. A etiqueta física mostra código e destino e é reconstruída depois de carregar.

As etapas ficam no próprio DeliveryOrder: conferido, embalado, etiquetado, separado e liberado. Não foi criado outro inventário de pedidos. O pátio atual prepara uma encomenda pessoal de cada vez; estoque com quantidades, carrinhos e filas de docas não estão concluídos.

## Delegação e missão

O operador logístico existente, Elisa, precisa estar realmente contratado para assumir a preparação. A solicitação física de delegação cobra R$ 3 pelo financeiro, com chave única por pedido, e realiza oito segundos de trabalho lógico. O tempo restante é salvo. Sem operador contratado, a delegação é recusada e a preparação manual continua disponível.

A preparação delegada restaura as mesmas flags e o mesmo pacote. Ainda não foi acrescentado um NPC de operador animado na bancada; a automação é lógica, não uma animação humana completa de trabalho.

A missão Operador de Expedição pertence ao GridShiftMissions existente. O requisito de conclusão verifica o pedido vinculado, preparação liberada, deslocamento real com carga na van e entrega concluída. O contador sozinho não libera a recompensa. O bônus usa a chave financeira de missão existente e é pago uma vez.

## Interface

- Equipamentos mostra a van e as operações disponíveis no depósito, com localização no mapa.
- Frota inclui o veículo pessoal com combustível, condição, carga e autorização de serviços reais.
- Mapa e minimapa mostram a van separadamente dos veículos autônomos.
- Entregas concluídas continuam sem Acompanhar e agora oferecem **Ver comprovante**, com ID, encomenda, destinatário, responsável, calendário quando registrado, valor e transação.
- As estações novas reutilizam InteractionFocus. A referência do jogador é compartilhada por cena, evitando uma busca global independente para cada estação.

Não foi refeito todo o Dashboard ou o Financeiro; gráficos e módulos adicionais continuam no plano de expansão.

## Saves

O formato atual passa a v5. Leitura de v1 a v4, slots, perfis, backup e gravação atômica são mantidos. Não foram lidos ou gravados slots pessoais para executar os testes.

O save inclui posição/rotação e checkpoint da van, carga por ID, preparação, tempo delegado e distância conduzida com a encomenda. Combustível e condição permanecem nos dados empresariais existentes. Salvar ao volante registra um desembarque seguro: ao carregar, a van volta estacionada e o jogador fica fora, com os controles válidos e a carga preservada.

A validação recusa carga sem pedido correspondente, etapas de depósito incoerentes, números inválidos e versões futuras desconhecidas. Não há migração em lote nem exclusão das partidas antigas.

## Arquivos

Novos scripts: GridShiftActiveConfig, GridShiftActiveOperations, GridShiftPlayerVan, GridShiftWorldAction, GridShiftPackageIdentity, GridShiftPersonalVehicleEconomy e Editor/GridShiftV16Setup, GridShiftV16Checks, GridShiftV16SaveChecks.

Scripts existentes alterados: DeliveryData, DeliveryPoint, DeliveryManager, DeliverySaveStore, DeliveryPersistence, GridShiftProfiles, GridShiftCompany, GridShiftSaveValidation, GridShiftPersonalDelivery, GridShiftMissions, GridShiftNativeOperations, GridShiftMapMarkers, InteractionFocus e InteractionPrompt. GridShiftV15Checks aceita a versão atual do save na regressão existente.

Assets novos: Resources/GridShiftPlayerVan.prefab, Resources/GridShiftActiveConfig.asset e quatro materiais em Generated/Operations16. Modelo base e biblioteca de áudio são do projeto existente. Não foram instaladas dependências ou assets externos.

Cena alterada: SampleScene, por inclusão do ponto de expedição e serialização do indicador personalOnly. MainMenu e Office não foram reconstruídos. Não há referências novas para o usuário preencher manualmente.

## Validação executada e alcance

Relatórios em Docs/Validacao16. Testes em cópia integral dos assets, Unity 6000.0.83f1, DX11, com renderização. Pacotes locais: URP 17.0.4, Unity UI 2.0.0 e Input System 1.20.0. O manifest e o PackageCache originais não foram editados. Avisos de compatibilidade do asset global URP são preexistentes nessa composição; não há alegação de equivalência com todos os editores ou de build final.

`runtime.txt` registra 57 verificações aprovadas: preparação, recusa de códigos/caixas/setores incorretos, etiqueta física, coleta, carga, condução do Rigidbody, direção, ré, câmera principal, áudio no canal Vehicles, combustível, pausa, colisão contra obstáculo, parada e desembarque, reentrada, save ao volante, carga após recarregar, recuperação de queda, destino e pagamento únicos, missão e bônus, inspeção, abastecimento, equipamentos, comprovante e delegação persistente sem cobrança duplicada.

O deslocamento medido no teste de aceleração foi aproximadamente 1,27 m no pátio antes de testar colisão e frenagem. Isso não é uma viagem completa dirigindo por todas as ruas. Os testes posicionam o avatar nas estações e no destino e chamam os comandos reais; não são uma sessão manual de teclado e não comprovam todas as combinações de E/G/M/ESC.

`regression-deliveries.txt` registra a regressão da entrega pessoal anterior, com caminhada real pelo CharacterController, save/load e uma entrega de caminhão com NPC saindo, entregando, retornando e pagamento único. `compatibility.txt` cobre os testes físicos e armazenamento v1/v3 já existentes. `storage.txt` registra a compatibilidade adicional de v4 sem campos novos e a rejeição de estados v5 inválidos.

A captura do comprovante acompanha os relatórios e confirma a apresentação da interface. A tentativa de vista externa do depósito ficou obstruída pela construção e não comprova a disposição visual completa. A revisão visual do pátio e um percurso completo de condução permanecem pendentes.

## Pendências e próxima prioridade

As 100 ideias não estão todas implementadas. Permanecem pendentes campainhas/clientes ausentes, múltiplos pacotes, carrinhos, estoque e docas completos, viagens de van em vários trajetos, treino de estacionamento, oficinas/postos distribuídos e integralmente conectados, ajuda na descarga, dez missões secundárias, contratos recorrentes, relacionamento, eventos urbanos, upgrades pessoais, bicicleta, conquistas e evolução visual do Office.

Também faltam novos limites físicos validados em todas as bordas, teste prolongado, comparação de CPU/memória/GPU/FPS e executável 16.0. O veículo é um protótipo de condução em cidade plana, sem animação de portas ou suspensão completa. Não foi reivindicado ganho global de desempenho.

Próxima prioridade: validar a saída do pátio e uma viagem completa até vários clientes, com estacionamento e entrega; depois ampliar docas/cargas e a participação do jogador na descarga dos caminhões. As fases seguintes devem manter os estados e pagamentos únicos já usados neste lote.
