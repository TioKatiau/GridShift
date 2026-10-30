# GridShift 12.0 — correções de missões, trânsito, rotas e Urban Flow

Aplicação de 27/10/2026. A implementação reutiliza os gerenciadores existentes. Chromium, WebView e URLs externas não foram adicionados. Backup dos scripts antes desta atualização: `Docs/Backup12/20261027/Scripts`.

## Causas e correções

### Missões e saves

O HUD exibia `step + 1` como progresso. Isso representava a posição da etapa atual, mas parecia uma contagem de objetivos cumpridos: uma missão com um objetivo mostrava 1/1 antes da realização. Expansão e Crise também mudavam para `Completed` antes de o jogador confirmar a recompensa.

Agora a central e o HUD usam a mesma apresentação de estado, em português. Objetivos cumpridos aguardam confirmação; somente a confirmação aceita conclui a missão e recebe o bônus. Expansão detalha entregas, especialista contratado, upgrade e reputação. Crise detalha entregas, contas, manutenção e reputação. Uma contratação adicional de motorista não substitui o requisito de especialista da expansão. Contratos verificam a entrega vinculada e sua procedência, além do estado Completed. Entregas em transporte não satisfazem o objetivo.

O pagamento mantém a chave financeira única de missão. Uma segunda confirmação é recusada. Ao receber o bônus, o objetivo antigo é desmarcado e uma notificação “MISSÃO CONCLUÍDA” usa a fila e o fade existentes. O desbloqueio posterior continua nos gerenciadores atuais.

A campanha passa à versão interna 2, mantendo o formato v3 dos slots e a leitura das campanhas v1. Estados de conclusão sem recompensa são reavaliados; contadores são limitados às etapas válidas e não substituem a entrega ou os requisitos reais. Recompensas já recebidas são preservadas. A atualização não apaga nem regrava em lote partidas do usuário: a migração ocorre na memória ao carregar e a próxima gravação normal usa a escrita atômica e o backup existentes.

### Filas e cruzamentos

TrafficJunction tinha um único proprietário. Mesmo dois carros na mesma trajetória eram tratados como conflitantes até a traseira do primeiro ultrapassar a região de liberação. Além disso, a verificação de espaço na saída podia interpretar o líder em movimento como uma obstrução fixa.

Agora as reservas registram os veículos e a trajetória de entrada. Veículos na mesma rota dirigida podem compartilhar a travessia; rotas diferentes continuam exclusivas, de maneira conservadora. A reserva permanece até o último veículo liberar a região, protegendo pedestres e tráfego conflitante. Objetos desativados ou destruídos são removidos das reservas.

Na verificação antecipada da saída, somente um líder já autorizado na mesma trajetória, à frente e em movimento compatível pode ser reconhecido como fluxo. Veículos parados, outros obstáculos e faixas ocupadas continuam bloqueando. Sensores frontais, intervalo proporcional à velocidade, atraso de reação, aceleração, frenagem e varreduras físicas de cada passo foram mantidos. Não há teletransporte nem desativação de obstáculos para aumentar vazão.

O Inspector mantém MotivoParada, Diagnostic e DiagnosticDetail. Os detalhes incluem sinal permitido, saída livre, bloqueador da saída, pedestres, reservas, rota e ponto. `logStopChanges` permite registrar mudanças de bloqueio no Console sem emitir mensagens a cada frame.

### Rotas e acompanhamento

O traçado anterior interrompia uma volta na primeira ocorrência da rota de coleta, mesmo quando o ponto já estava atrás do carro. Também identificava todas as ocorrências de uma rota repetida como se fossem o segmento atual. Isso podia desenhar atalhos aparentes ou repetir a volta depois do retorno à via.

GridShiftTrackedRoute produz um traçado compartilhado a partir do caminho real de CompanyVehicle, NextPoint e Target. Ele distingue a saída inicial em laço do retorno final, valida conexões dirigidas e interrompe uma cadeia inválida. O endpoint é o ponto real de parada. A visualização acompanha o trecho operacional atual: primeiro coleta, depois entrega. O destino final tem marcador próprio.

Mapa e minimapa atualizam o traçado a cada 0,15 s, inclusive após recálculo. Segmentos que cruzam a borda do mapa são recortados, sem desaparecer inteiramente só porque uma extremidade está fora da tela. Veículo acompanhado recebe cor distinta dos outros veículos.

O padrão é somente mapa e minimapa. As configurações nativas permitem ativar a visualização 3D e alterar cor, espessura, transparência e distância. A opção 3D amostra a superfície das ruas e interrompe trechos sem asfalto reconhecido; não projeta uma ponte por cima de prédios. Marcadores antigos no mundo ficam ocultos no modo somente mapa. As preferências ficam em PlayerPrefs, separadas do progresso da empresa.

Criar uma entrega não ativa destaque automaticamente. Parar acompanhamento limpa a seleção, mantendo veículo e entrega. Concluir a entrega encerra o destaque. IDs persistem entre aparelhos, cenas e saves; referências ausentes ou seleções antigas de entregas concluídas são encerradas com segurança.

### Urban Flow e botão Funcionários

O painel tinha dimensões lógicas fixas de 1280 × 800, encaixadas com a menor escala. As superfícies reais eram aproximadamente quadradas ou verticais, deixando grandes faixas pretas. O RectTransform agora adapta sua altura à proporção da superfície e ocupa 95% da largura e da altura, com escala uniforme. Não houve mudança destrutiva na moldura, no monitor, nos prefabs ou na câmera.

A navegação ficou mais larga, com botões de 64 unidades, legendas maiores e seleção destacada. O cabeçalho mostra empresa, saldo, dia e hora. O conteúdo usa cards com altura calculada pelos filhos, rolagem vertical e títulos/estados separados. Trocar de aba inicia no topo e aplica fade de 0,14 s. O hover existente foi preservado; sua configuração não adiciona componentes duplicados a cada troca de aba.

DeliveryPresentation procurava qualquer texto contendo “Funcionários” dentro do jogador e, se não encontrasse um contador numérico associado, usava a própria legenda como destino da quantidade. Como o tablet é filho da câmera do jogador, sua aba também podia ser encontrada. A busca agora exclui explicitamente GridShiftNativeOperations e aceita somente o contador numérico do HUD. O total aparece no título do painel; o nome da aba permanece “Funcionários”. A mesma proteção vale para os indicadores de frota e dinheiro.

G, animação do tablet, E, zoom/fade do computador, ESC e restauração dos controles foram preservados. Ambos continuam lendo e alterando a mesma empresa, finanças, empregados, upgrades e missões.

## Arquivos e referências

Criados: `GridShiftTrackedRoute.cs`, `GridShiftRouteView.cs`, `Editor/GridShiftV12Checks.cs` e `Editor/GridShiftV12TrafficChecks.cs`.

Modificados: `GridShiftMissions.cs`, `GridShiftCampaignData.cs`, `GridShiftCampaignValidation.cs`, `GridShiftNativeOperations.cs`, `GridShiftUITheme.cs`, `DeliveryPresentation.cs`, `DeliveryManager.cs`, `CompanyVehicle.cs`, `GridShiftMapMarkers.cs`, `GridShiftMapRouteGraphic.cs`, `Traffic/TrafficCar.cs`, `Traffic/TrafficJunction.cs` e o gancho opcional no teste `Editor/GridShiftV11Checks.cs`.

Cenas e prefabs do projeto original não precisaram ser alterados nesta atualização. As interfaces e o componente de preferências de rota são vinculados em execução. Não há referências novas para preencher manualmente. Se o editor mantinha uma sessão antiga em Play, encerre-a e inicie uma nova para recarregar os scripts e a sessão.

## Validação

Os relatórios e capturas ficam em `Docs/Validacao12`. Os testes usam cópia isolada do projeto e perfis temporários; não usam partidas pessoais. A regressão de requisitos de missão utiliza cenários de estado controlados e o sistema real de comandos, finanças e armazenamento. A regressão de entrega usa deslocamento físico da frota na cidade.

Os resultados finais, o teste com tráfego da cena real e a exportação são registrados abaixo após sua execução.
