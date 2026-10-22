# GridShift — atualização 3.0

## Funcionamento

- Pulo: o contato efetivo do CharacterController libera um salto. A sonda não precisa confirmar novamente um contato já informado pelo movimento. A velocidade ascendente impede rearmar o salto e o input usa a borda de pressionamento do Espaço.
- Trânsito: a reserva do cruzamento acompanha a traseira mesmo depois da troca de rota. Os semáforos agrupados recebem seu estado exclusivamente do controlador do cruzamento. Sensores e regras de frenagem permanecem ativos.
- Cada TrafficCar mostra `Motivo Parada` e `Bloqueador` no Inspector, com volume, alcance e bloqueador nos Gizmos de seleção. Motivos: semáforo, veículo, pedestre, cruzamento bloqueado, obstáculo e falta de conexão.
- Pedestres: dez prefabs Hodaart originais, controlador novo usando os clips Idle/Walk existentes, sem root motion. Rede própria amostrada sobre a geometria elevada das calçadas, com verificação de obstáculos. Não usa as rotas dos veículos.
- Travessias: aguardam todos os sinais de veículos associados ficarem vermelhos e o cruzamento ficar livre. A reserva permanece até a chegada à outra calçada. Nos cruzamentos sem sinais, também verificam a aproximação de veículos. É uma política conservadora: um pedestre por cruzamento de cada vez, sem alterar o ciclo das luzes.
- Casas: acessos são extraídos dos painéis de porta dos quatro modelos. Somente trajetos fisicamente verificáveis são conectados. Sem interiores, o personagem aguarda junto à entrada e é recolhido quando sai do campo de visão, sem atravessar a fachada.
- Postes: somente instâncias de `Props_Street Light`; luz quente, acendimento gradual com pequenas diferenças entre postes, emissão em uma lâmpada separada, sem sombras dinâmicas. Até 24 luzes próximas ficam ativas, conforme distância configurável.
- Céu: gradiente diurno/noturno, estrelas discretas e lua, controlados pelo horário existente. A iluminação ambiente permite enxergar a cidade à noite.
- Rotina: densidades de veículos e pedestres interpoladas entre horários configuráveis; reaproveitamento gradual fora da vista. O multiplicador da rotina é separado da densidade manual da frota.
- Persistência: a cidade permanece carregada durante a visita ao Office. Relógio, agentes, reservas e rotina continuam; a apresentação do céu é aplicada à cena que está ativa.
- Terminal: interação exclusiva por proximidade, `[E] Acessar`, aproximação e retorno suaves, fade do conteúdo, restauração da câmera local, FOV, HUD, controles e cursor. O jogador não é deslocado. A tela permanece preta fora do acesso.

## WebView: pendência externa explícita

Não existe navegador instalado no projeto. O terminal exibe uma mensagem de indisponibilidade; não apresenta uma imagem ou uma página simulada. A URL `https://urbanlink-flow.onrender.com` respondeu HTTP 200 durante a verificação, com o título UrbanLink Flow. Isso não comprova cliques, login ou formulários dentro do jogo.

A integração preparada usa **Vuplex 3D WebView for Windows and macOS**, que é comercial e declara suporte a Unity 2019.4 ou posterior, Windows 10+ x64 e macOS com Metal. O pacote não cobre Linux. Não houve compra, importação do pacote ou teste de navegador real nesta atualização.

Para finalizar essa parte:

1. Importar uma cópia licenciada ou de avaliação compatível do pacote Windows/macOS.
2. Adicionar `GRIDSHIFT_VUPLEX` em Scripting Define Symbols do alvo correspondente.
3. Executar `GridShift > Atualização 3.0 > Configurar cidade e terminal` fora do Play Mode. A ferramenta conecta CanvasWebViewPrefab e o adaptador à tela do monitor.
4. Testar a página real, mouse, rolagem, campos de texto, teclado, conexão lenta/indisponível e ESC na plataforma de destino. O adaptador possui tempo limite de 30 segundos e limpa a página ao sair.

O adaptador condicional foi preparado a partir das APIs documentadas; sua compilação e execução com o SDK permanecem pendentes até a importação. Na configuração atual ele fica excluído da compilação.

Referências verificadas:

- https://store.vuplex.com/webview/windows-mac/
- https://developer.vuplex.com/webview/CanvasWebViewPrefab
- https://developer.vuplex.com/webview/IWebView

## Configuração automática

`GridShiftV3Setup` prepara SampleScene e Office, adiciona a layer Pedestrian em um slot livre conhecido, gera o grafo e os acessos seguros, associa os dez modelos e as animações, encontra os postes pelo nome do prefab e configura o monitor existente. Preserva os prefabs de origem. Também acrescenta profundidade à RenderTexture do minimapa e evita listeners de áudio duplicados por cena.

Os parâmetros ficam em PedestrianManager, CityEnvironment, TrafficManager e ComputerInteraction. O grafo e as travessias aparecem nos Gizmos ao selecionar PedestrianPaths. Alterações na geometria da cidade exigem executar novamente a preparação.

## Arquivos

Criados:

- `Assets/Scripts/InteractionFocus.cs`
- `Assets/Scripts/TerminalWebView.cs`
- `Assets/Scripts/VuplexTerminalWebView.cs`
- `Assets/Scripts/PedestrianGraph.cs`
- `Assets/Scripts/PedestrianAgent.cs`
- `Assets/Scripts/PedestrianManager.cs`
- `Assets/Scripts/CityEnvironment.cs`
- `Assets/Scripts/Editor/GridShiftV3Setup.cs`
- `Assets/Scripts/Editor/GridShiftV3Checks.cs`
- `Assets/Scripts/Editor/GridShiftV3Visual.cs`
- `Assets/Scripts/Editor/PedestrianCrossingChecks.cs`
- `Assets/Scripts/Editor/BuildingEntranceScan.cs`
- `Assets/Generated/CityLife/GridShiftSky.shader`
- `Assets/Generated/CityLife/CitySky.mat`
- `Assets/Generated/CityLife/MonitorBlack.mat`
- `Assets/Generated/CityLife/TerminalText.mat`
- `Assets/Generated/CityLife/WarmLamp.mat`
- `Assets/Generated/CityLife/Pedestrian.controller`
- Arquivos `.meta` dos novos scripts, da pasta e dos assets acima.

Modificados:

- `Assets/Scripts/PlayerMovement.cs`
- `Assets/Scripts/ComputerInteraction.cs`
- `Assets/Scripts/CompanyEntrance.cs`
- `Assets/Scripts/WorldSession.cs`
- `Assets/Scripts/Traffic/TrafficCar.cs`
- `Assets/Scripts/Traffic/TrafficJunction.cs`
- `Assets/Scripts/Traffic/TrafficLight.cs`
- `Assets/Scripts/Traffic/TrafficLightGroup.cs`
- `Assets/Scripts/Traffic/TrafficManager.cs`
- `Assets/Scenes/SampleScene.unity`
- `Assets/Scenes/Office.unity`
- `Assets/MinimapTexture.renderTexture`
- `ProjectSettings/TagManager.asset`

## Validação

Os relatórios de execução e as imagens finais ficam em `Docs/Validacao3`. Os testes são executados em uma cópia independente do projeto, fora da pasta Temp administrada pelo Unity. Capturas servem para verificar a apresentação; não substituem medição de desempenho em um build de destino.

A validação não cobre a WebView ausente nem constitui garantia de ausência de congestionamentos sob qualquer quantidade de agentes ou edição futura das ruas.

Se uma cena já estava aberta no Editor durante a atualização dos arquivos, reabra a cena para carregar a configuração salva. Não é necessário executar novamente a preparação para começar a testar o que já foi aplicado.
