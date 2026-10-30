# GridShift 8.0 — cidade, navegação e estabilidade

Implementação e validação em 25/10/2026, Unity 6000.0.83f1.

## O que mudou

- Um quarteirão acrescentado a leste da cidade, com três ligações viárias bidirecionais, oito peças de rua, dois cruzamentos novos, três comércios e duas residências.
- Quinze movimentos dirigidos novos conectados à rede existente. Uma aproximação sem entrada correspondente na rede antiga foi descartada durante a configuração.
- Cinco destinos novos; o catálogo passou de 59 para 64 destinos, mantendo os IDs antigos.
- Grafo com 3.828 nós, 31 travessias e 14 entradas residenciais. Há 350 nós conectados na região leste e navegação entre as regiões antiga e nova.
- População diurna configurada para 40, respeitando o limite de 50 e a redução noturna existente.
- Uma vaga de baliza opcional para veículos ambientes compatíveis, com limite de 5,2 m de comprimento e 2,2 m de largura. Veículos da empresa são excluídos desse objetivo.
- Terreno adicional além da área jogável, quatro limites físicos discretos e recuperação do jogador em caso de queda anormal.

## Diagnóstico e correções

### Tablet, computador e DEMO

O frontend local reconstruía listas e botões a cada atualização, incluindo abas ocultas. Agora mantém os elementos quando seus dados não mudaram, atualiza somente a aba visível e adia a troca de dados durante um clique. Listas extensas usam `content-visibility`; o formulário preserva seu estado.

O catálogo de ofertas também fazia muitas consultas Dijkstra repetidas para descobrir quais destinos eram alcançáveis. A medição anterior do gerador em execução ficou em aproximadamente 188 ms por chamada. Agora a descoberta de alcance visita cada rede uma vez por origem e o catálogo é reutilizado enquanto contratos aceitos, parceiros liberados e disponibilidade dos destinos permanecerem iguais. A busca ponderada usada pelas entregas continua sendo a existente.

A ponte deixa de reenviar identificação e instruções inalteradas. A resolução solicitada é acompanhada separadamente da textura recebida, evitando reaplicações durante o redimensionamento assíncrono. A textura continua com boa resolução e atualização limitada; não foi reduzida drasticamente para produzir os resultados.

Guardar um dispositivo interrompe seus uploads de textura e eventos de entrada. Mídias que estavam reproduzindo são pausadas e retomadas ao reabrir. A sessão é preservada. O compartilhamento existente do navegador entre tablet e computador do Office foi mantido.

Não foram modificados `Library/PackageCache`, APIs do UWB ou o site publicado.

### Mapa

O mapa ocupa aproximadamente 80% da altura útil do painel, tem textura própria de maior resolução e se adapta à área da interface. Seus limites abrangem ruas, construções, rotas e destinos, incluindo a área residencial ao norte e o bairro novo.

- **M:** abrir/fechar; **ESC:** fechar respeitando a prioridade de interfaces.
- **Arrastar com o botão esquerdo:** mover, com limites.
- **Roda:** aproximar/afastar dentro da superfície do mapa.
- **Centralizar no jogador:** aproximar e acompanhar sua posição.
- **Ver toda a cidade:** restaurar o enquadramento completo.
- **Ícones da frota e de entregas:** selecionar uma entrega ativa. O destino selecionado fica destacado.

O minimapa e o HUD voltam ao estado anterior ao fechar. A simulação continua durante a consulta.

### Trânsito

Foram encontradas quatro associações antigas de reserva com cruzamentos errados. A configuração agora deriva o cruzamento da geometria da chegada e da saída, confirma a existência da peça física e reposiciona linha de retenção e saída. As cabeças dos semáforos são associadas à aproximação correta. Os detalhes estão em `Validacao8/configuracao.txt`.

Veículos deixavam de funcionar permanentemente quando uma curva excedia a tolerância da faixa. Agora permanecem diagnosticáveis, reconhecem pontos já ultrapassados dentro do corredor e podem corrigir a trajetória com ré limitada sobre o caminho efetivamente percorrido. A recuperação mantém as sondas de volume, o asfalto sob a carroceria e a reserva exclusiva do cruzamento; não empurra agentes através de obstáculos.

A ré de correção usa velocidade baixa, distância máxima de 2 m, tempo de 4 s, intervalo de 12 s e até duas tentativas antes de procurar uma continuação compatível. O veículo de trás pode cooperar com uma correção curta quando existe espaço; a solicitação não se propaga indefinidamente para formar uma cadeia de ré.

O estacionamento exige uma vaga explícita, dimensões compatíveis e aproximação alinhada. A entrada usa avanço e dois arcos de ré; a saída retorna à faixa. Se uma entrada for bloqueada, o carro aguarda e pode desfazer o trecho de ré de forma segura. Uma trajetória inválida permanece parada para diagnóstico, sem atravessar obstáculos. Vagas e tempos são configuráveis no Inspector.

Uma sequência de pedestres não pode reocupar imediatamente o mesmo cruzamento: após a saída há uma janela de quatro segundos para a retomada dos veículos. A reserva de um pedestre que ainda está na rua não é liberada por um simples temporizador.

### Pedestres

Os agentes guardam seis destinos recentes, evitam destinos triviais e repetidos, verificam a conexão antes de adotá-los e limitam as tentativas de recuperar uma rota bloqueada. Curvas fechadas reduzem a velocidade de deslocamento enquanto o corpo gira progressivamente; animação e movimento usam a velocidade efetiva.

Uma travessia bloqueada pode ser abortada caminhando de volta ao início, mantendo a reserva durante o retorno. O desvio continua restrito a superfícies de calçada verificadas. As entradas residenciais existentes foram reutilizadas e duas novas foram integradas ao grafo; a porta do Office continua excluída.

Ao chegar à porta, o personagem se alinha antes da transição. A devolução ao pool só ocorre fora de vista. Se o jogador continuar observando, o NPC pode desistir da visita e retomar outra rota, em vez de permanecer desativado indefinidamente na entrada.

As animações fora de vista usam culling, sem interromper a navegação. A população continua gerenciada pelo pool existente.

### Entregas e acesso aos estabelecimentos

O acesso visual é separado da parada do veículo. O marcador acompanha um nó de acesso verificado e a parada continua no segmento de rua correspondente. O vínculo com a construção é explícito, evitando escolher outro prédio apenas por estar mais próximo da parada.

Resultado da configuração:

- **10 destinos:** porta identificada e acesso verificado.
- **46 destinos:** calçada acessível validada; a porta exata ainda deve ser confirmada.
- **8 destinos:** não havia vínculo/superfície suficiente para uma correção automática segura; foram registrados para ajuste manual.

O campo `DeliveryPoint.accessNeedsReview` identifica os acessos que ainda exigem revisão. Os IDs e construções constam em `Validacao8/configuracao.txt`. Não foi inventada uma porta para esses casos. A cor ciano, a escala moderada e a pulsação discreta do marcador foram preservadas.

## Diagnóstico durante o jogo

**F3** alterna o painel. Além das métricas existentes, mostra os veículos e pedestres mais próximos, seus estados, rota/nó, velocidade, tempo sem progresso, bloqueador, tentativas e destinos recentes. O painel começa oculto e não altera as decisões da IA. Componentes também mantêm dados no Inspector e gizmos quando selecionados.

## Validação executada

Relatórios completos ficam em `Docs/Validacao8`:

- `estrutura.txt`: MainMenu, Office e SampleScene sem scripts ausentes; IDs únicos; rotas novas sobre asfalto, com saída e conexão de ida e volta; destinos alcançáveis; ligação das calçadas e presença dos limites.
- `transito.txt`: dez cenários existentes de física, mais ré com reserva exclusiva, obstáculo traseiro, baliza e saída de vaga.
- `cidade-play.txt`: abertura e dimensão do mapa, arraste, limites, zoom, retorno do minimapa e dos controles; progresso de pedestres/veículos e uma entrega real concluída no bairro novo, com pagamento.
- `navegador.txt`: mouse e digitação no CEF, identificação DEMO, menus, oferta e pedido personalizado de 500 kg, computador, tablet, histórico, upgrades, prioridade de interfaces, conclusão física de entregas e saída por ESC.
- `pedestres.txt`: desvio de poste e do jogador com chegada ao destino e permanência na calçada; rejeição de asfalto abaixo do meio-fio.
- `desempenho.txt`: roda normalizada e nativa do Windows, rolagem interna e externa, cursor fora da tela, interface sobreposta, computador, tablet, transições repetidas, F3, presets e estresse com 50 pedestres e 30 veículos.
- `regressao-core.txt`: seis modalidades, atribuição por peso, aceitação idempotente, renovação de ofertas, economia, upgrades, salvamento, remapeamento, tutorial, foco de interfaces, Office e três entregas físicas com pagamento único.
- `ofertas-auditoria.txt`: equivalência entre o alcance otimizado e o Dijkstra existente, além de idempotência e renovação do catálogo.
- `demo-performance.json`: teste do HTML antes/depois, com 100 contratos e 30 atualizações. A lista `#orders` recebeu 62 mutações antes e zero depois; o botão focado passou a ser preservado.
- `casas-limites.txt`: cenário específico de entrada residencial e limites do jogador.
- `iluminacao.txt`: seis postes novos integrados ao ciclo de dia/noite e ao orçamento existente de 24 luzes próximas.

As capturas ficam em `Validacao8/Imagens`. Os testes foram executados em cópia separada do projeto, com salvamentos de teste e preferências isoladas. Uma execução do teste de interface iniciada sem renderização foi descartada e repetida com renderização; ela não foi usada como evidência de aprovação.

### Medições e limites da comparação

Na suíte de desempenho, cidade diurna, noturna, Office com navegador fechado, computador com navegador e tablet tiveram média de **16,67 ms por quadro**. O cenário com **50 pedestres e 30 veículos** também registrou 16,67 ms de média e 16,78 ms no percentil 95.

São medições do editor com limite de 60 FPS, na máquina desta validação. Não representam uma promessa de FPS para qualquer computador ou uma comparação controlada com uma build anterior. A memória registrada pelo profiler da Unity não inclui todos os processos Chromium.

A medição do gerador antigo e do cache em execução está em `ofertas-cache-runtime.txt`; a auditoria da geração final otimizada está em `ofertas-auditoria.txt`. O teste de HTML mede estabilidade do DOM e tempo até pintura em navegador de teste, não a latência integral do UWB ou do site publicado.

A geração final registrou 11,45 ms e a consulta ao cache 0,264 ms na auditoria em modo de edição. Essas medições têm contexto diferente da medição em execução de 188 ms/0,024 ms; não devem ser apresentadas como uma comparação controlada de latência do dispositivo.

Foram usados os pacotes efetivamente instalados no projeto: Input System 1.20.0, UWB/CEF 2.2.8, URP 17.0.4 e UGUI 2.0.0. O manifesto principal já solicitava versões diferentes de URP/UGUI das presentes no arquivo de lock e no cache. Ele foi preservado; a cópia de testes referenciou os pacotes instalados, sem executar uma migração de dependências.

## Arquivos e modelos

Scripts criados:

- `GridShiftCityBoundary.cs`
- `GridShiftMapDrag.cs`
- `Traffic/TrafficParkingBay.cs`
- `Editor/GridShiftV8Setup.cs`
- `Editor/GridShiftV8Checks.cs`
- `Editor/GridShiftV8PlayChecks.cs`

Scripts modificados:

- `DeliveryManager.cs`, `DeliveryPoint.cs`
- `GridShiftDeliveryBridge.cs`, `GridShiftUwbTerminal.cs`
- `GridShiftOperationsUI.cs`, `GridShiftMapMarkers.cs`
- `GridShiftPerformanceDiagnostics.cs`
- `PedestrianAgent.cs`, `PedestrianManager.cs`
- `Traffic/TrafficCar.cs`, `Traffic/TrafficJunction.cs`, `Traffic/TrafficManager.cs`

Frontend modificado: `Assets/StreamingAssets/GridShiftDemo/index.html`.

Cena alterada: **SampleScene**. Office e MainMenu foram preservadas e testadas. Nenhum prefab existente foi substituído. A expansão reutiliza os modelos/materiais de `Road Lane_01`, `Road Corner_01`, `Road Intersection_01`, semáforos, postes, árvores, Books Shop, Chicken Shop e House_01 da coleção SimplePoly já instalada. Os modelos de personagens e o controlador de animação existente foram mantidos.

As duas peças T na borda mantêm seus objetos e referências, recebendo a geometria de cruzamento completo. Um outdoor e dois aerogeradores que ocupavam o novo corredor foram reposicionados na faixa de paisagem. A expansão reutiliza materiais e não acrescenta uma grade de luzes dinâmicas com sombras. As luzes e a população continuam sujeitas aos orçamentos existentes.

## Uso, backup e pendências

Para carregar a cena atualizada no editor já aberto: sair do Play Mode, reabrir `Assets/Scenes/SampleScene.unity` e iniciar novamente. **M** abre o mapa; **J** abre operações; **G** abre o tablet; **E** acessa o computador; **F3** mostra diagnóstico.

Backup anterior à edição: `C:/Users/Gb191/.codex/backups/GridShift-v8-before-20261025`. As cenas foram comparadas por hash antes de aplicar a versão validada. O configurador também produz backups antes de alterações estruturais e recusa aplicar a expansão duas vezes.

Restam a revisão visual das 46 portas aproximadas e o ajuste dos oito acessos listados. Cenários extremos de congestionamento e combinações não cobertas de dimensões/obstáculos devem continuar sendo acompanhados com F3; os testes não provam ausência de todo bloqueio possível.

O site UrbanLink Flow publicado não foi modificado. A inspeção pública e o trabalho que depende do repositório/backend estão separados em `URBANLINK_FLOW_PENDENCIAS_8_0.md`.
