# GridShift — otimização e rolagem 7.0

Esta atualização complementa a atualização de operação/progressão já existente. As cenas, os controles do personagem, a física de trânsito, as travessias, a lógica de entregas e o formato do salvamento foram preservados.

## Correção da rodinha

O adaptador anterior sempre dividia `Mouse.current.scroll.y` por 120. O Input System 1.20 instalado usa `UniformAcrossAllPlatforms` por padrão: uma posição da roda corresponde a aproximadamente 1. Assim, o roteador multiplicava 1/120 por 110 e enviava apenas 1 ao Chromium em vez de aproximadamente 110. O movimento ficava cerca de 110 vezes menor, com perdas adicionais em deltas fracionários. Não era ausência de suporte ao scroll no Chromium.

`GridShiftUwbInput` agora distingue os deltas normalizados dos deltas nativos do Windows. `GridShiftUwbInputRouter` acumula frações, converte para o inteiro esperado por `WebBrowserClient.SendMouseScroll` e envia a posição real no navegador. O sinal é preservado. O Chromium escolhe o painel rolável sob o cursor, incluindo encadeamento de rolagem nativo.

O roteador confere foco exclusivo, conexão, limites da imagem e o primeiro resultado do raycast do EventSystem. Uma interface sobreposta ou cursor fora da imagem impede o envio. A captura de arrasto não amplia a área de rolagem. O mapa ampliado ignora a roda enquanto o navegador tem foco. Não há eventos de scroll sintéticos via JavaScript, arrasto artificial, input legado ou outro EventSystem.

Os testes inserem conteúdo HTML controlado somente na cópia de validação para observar `scrollY` e `scrollTop`. A implementação do jogo envia eventos nativos ao CEF; não depende desse HTML.

## Medições e decisões

A captura foi feita com Unity 6000.0.83f1, em cópia isolada do projeto, com renderização real e Chromium real, a 60 FPS/VSync desligado. `ProfilerRecorder` mede Main Thread, GC por frame e memória Unity; o CPU Profiler também foi gravado em arquivo binário. Amostras de 12 segundos seguem aquecimento de 3 segundos por situação.

O trecho retido do primeiro perfil corresponde ao final do teste do tablet, e não representa sozinho toda a cidade. A análise lê cada quinto frame dos 300 frames retidos pelo Profiler (60 amostras). Os tempos são inclusivos e não devem ser somados. No perfil inicial:

- câmera do minimapa: cerca de 1,55 ms/frame no marcador externo;
- upload da textura UWB: cerca de 1,51 ms/frame;
- atualização dos marcadores UI: cerca de 0,10 ms/frame;
- física da cena: cerca de 0,14 ms/frame;
- TrafficCar.FixedUpdate: cerca de 0,12 ms/frame no conjunto;
- PedestrianAgent.FixedUpdate: cerca de 0,10 ms/frame no conjunto.

Havia também custo do próprio Editor, incluindo a extensão Unity AI. Não foi alterado o ambiente de desenvolvimento do usuário para esconder esse custo. A captura não comprova que veículos ou pedestres sejam o maior gargalo no computador final.

Os relatórios `antes.txt`, `depois.txt`, `profiler-antes.txt` e `profiler-depois.txt` ficam em `Docs/ValidacaoDesempenho7`. Os perfis binários grandes ficam na cópia temporária de validação; não foram adicionados aos Assets nem ao controle de versão.

## Otimizações aplicadas

- Navegador: o upload via API pública `LoadTextureData` passa a ter cadência própria, padrão 30 Hz configurável de 15 a 60, em vez de acompanhar a frequência de física do componente UWB. O input continua sendo encaminhado por frame. O componente UWB inicializa normalmente e seu Update é suspenso após ficar pronto; o terminal assume somente a atualização da textura. Ao ocultar, a textura deixa de ser atualizada e o áudio fica mudo. A resolução configurada de cada dispositivo, o zoom e a instância continuam preservados nas reaberturas existentes.
- A versão UWB 2.2.8 expõe `windowlessFrameRate` na inicialização, mas não oferece método público para suspender dinamicamente o renderizador CEF. Não foi inventada uma API de suspensão nem modificado o PackageCache. O Chromium oculto ainda pode executar tarefas; parar o upload Unity não equivale a parar o processo externo.
- MinimapCamera: renderização configurável, padrão 30 Hz. Rastreamento e projeção continuam atualizados; a RenderTexture e sua resolução são mantidas. Um acumulador evita perder cadência por pequenas variações de frame. A câmera da cidade não é reativada pelo script enquanto o jogador está no Office.
- Marcadores: deixam de ser todos desativados e reativados em cada LateUpdate. Somente ícones que deixaram de ser usados são ocultados. A identidade visual é mantida.
- Trânsito: o detalhe textual de diagnóstico só é reconstruído quando rota, ponto ou bloqueador mudam. Sensores, freios, reservas de cruzamento, semáforos e movimento continuam na cadência original.
- Pedestres: fila de replanejamento reaproveitada, hash do parâmetro de animação armazenado e remoção do array temporário em cada tentativa de desvio. Navegação, sensores de apoio, colisões e desvios não foram espaçados ou desativados.
- Relógio: atualiza o texto somente quando muda o minuto exibido; o sol e o horário da simulação continuam atualizados.
- Teclado do navegador: arrays vazios compartilhados quando não existem eventos de tecla.

Os gerenciadores já possuíam pooling de veículos e pedestres. A transição existente já usa carregamento assíncrono e mantém a cidade simulando enquanto o Office está ativo. Não foram reescritos nem substituídos. Não há chamadas novas de GC.Collect ou UnloadUnusedAssets.

## Configurações

Configurações → Vídeo oferece Baixa/Média/Alta, resolução, tela cheia, VSync e limite de FPS. O projeto original tem dois pipelines, Mobile e PC; Baixa usa Mobile, enquanto Média/Alta usam PC com parâmetros distintos de sombras e distância. Não foram criados três pipelines fictícios.

Configurações → Desempenho acrescenta sombras, distância de visão, densidade de pedestres, densidade de trânsito comum e intensidade dos efeitos. O peso dos Volumes é multiplicado pela intensidade escolhida; em zero, o pós-processamento da câmera do jogador é suspenso. As alterações URP usam cópias em memória, sem editar assets compartilhados. As preferências são gravadas ao fechar as configurações e reaplicadas após carregar cenas.

A densidade reduz metas de população ambiente, respeitando o piso de veículos autorais existente. A reciclagem existente recusa veículos com orientação de missão/empresa. As entregas não têm limite reduzido pela configuração gráfica. Sem selecionar novas opções, densidade e parâmetros visuais autorais são preservados, exceto pela cadência de apresentação otimizada.

## Diagnóstico

F3 abre/fecha um painel no Editor ou em builds de desenvolvimento. Ele começa oculto e não interpreta F3 digitado no navegador. Mostra FPS/média de frame em janela de 0,5 segundo, veículos ativos e aguardando, pedestres, entregas, Chromium conectados, memória Unity e bytes de GC por frame. Contadores indisponíveis são identificados. A memória não inclui os processos externos CEF.

## Arquivos desta intervenção

Criados:
- Assets/Scripts/GridShiftPerformanceSettings.cs
- Assets/Scripts/GridShiftPerformanceDiagnostics.cs
- Assets/Scripts/Editor/GridShiftPerformanceChecks.cs
- Assets/Scripts/Editor/GridShiftPerformanceProfileAnalysis.cs
- respectivos arquivos .meta e este documento.

Modificados:
- GridShiftUwbInput.cs, GridShiftUwbInputRouter.cs e GridShiftUwbTerminal.cs;
- GridShiftOperationsUI.cs, GridShiftMapMarkers.cs, MinimapCamera.cs e RealTimeClock.cs;
- PauseMenu.cs;
- Traffic/TrafficCar.cs e Traffic/TrafficManager.cs;
- PedestrianAgent.cs e PedestrianManager.cs.

Não foram modificadas cenas, PackageCache, controles do personagem ou a documentação da atualização 7.0 anterior por esta intervenção. O checkout já tinha diversas alterações anteriores; a lista acima se refere somente a este trabalho.

## Limites e validação manual

Não há medição GPU independente nesta execução, nem snapshot do pacote Memory Profiler. Memória estimada e tempo de CPU não provam ausência de vazamentos no CEF. A primeira medição diurna anterior teve um pico de aquecimento/compilação de shaders; sua média não deve ser usada para anunciar ganho percentual.

O cap de 60 FPS faz várias médias permanecerem perto de 16,67 ms. A conclusão deve se apoiar nos custos dos marcadores e nas alocações, sem prometer aumento global de FPS. Comparações também sofrem variação de população, tarefas do Editor e aquecimento. O uso de memória não demonstrou redução global.

Ainda é necessário validar uma build Windows no hardware final, com sessão prolongada, menus suspensos específicos do site remoto, diversas resoluções, captura GPU e investigação de picos na cidade em carga real. Não foram alterados LODs, materiais, iluminação noturna, occlusion culling ou batching às cegas. Não há ajuste de cena obrigatório para instalar os scripts novos: os serviços e configurações são inicializados automaticamente.
## Comparação observada

As alocações médias por frame incluem o Editor e o próprio ambiente de teste:

- Cidade de dia: 44.727 → 28.446 bytes/frame. Média 24,98 → 16,68 ms, mas o primeiro valor inclui um pico de aquecimento e não permite anunciar esse ganho como resultado da otimização.
- Cidade à noite: 43.914 → 28.335 bytes/frame; 16,67 → 16,67 ms.
- Office com navegador oculto: 41.700 → 24.466 bytes/frame; 16,67 → 17,05 ms, com P95 praticamente igual (17,05 → 17,06 ms).
- Office com navegador aberto: 18.269 → 21.812 bytes/frame; 16,68 → 16,67 ms. Houve aumento de GC nesta amostra; não há redução uniforme em todas as situações.
- Tablet: 34.631 → 21.769 bytes/frame; 16,68 → 16,67 ms.
- Carga ampliada, somente depois: 50 pedestres e 30 veículos, média 17,50 ms e P95 16,84 ms. Não existe medição anterior equivalente para calcular ganho nesse cenário.

A memória Unity no trecho do tablet passou de aproximadamente 600,6 para 603,5 MB decimais. Não foi demonstrada redução de memória total.

`depois.txt` registra SUCCESS nos testes de rodinha normalizada/nativa dos dois dispositivos, painel interno, cursor fora da tela, interface sobreposta, cliques/digitação/Backspace, ESC/G, foco do mapa, população ampliada, ausência de registros duplicados, câmera ativa no Office, persistência do gerenciador/relógio, F3 e presets URP. `configuracoes.txt` registra os testes da versão final dos presets, multiplicação de peso de Volume, pós-processamento em zero, distância solicitada, slider e gravação das preferências. A captura `configuracoes.png` foi inspecionada visualmente.

Para repetir `GridShiftPerformanceChecks.Batch` ou `SettingsBatch`, use uma cópia do projeto e defina seu productName como `GridShift Performance Validation`. O verificador recusa executar no produto principal para proteger preferências. Não use `-nographics` para testes de renderização/CEF. O teste de carga não equivale a uma sessão de várias horas.

No trecho final retido dos perfis CPU, o marcador externo da câmera do minimapa passou de 1,5506 para 0,7432 ms/frame (60 → 30 chamadas nas 60 amostras), e `UWB.LoadTexture.Apply` de 1,5063 para 0,8382 ms/frame (52 → 28 chamadas). `GridShiftMapMarkers.LateUpdate` passou de 0,1003 para 0,0279 ms/frame, e o diagnóstico `TrafficCar.LateUpdate` de 0,0237 para 0,0050 ms/frame. São custos observados nesses trechos de teste, não uma previsão de ganho de FPS em toda a cidade. O perfil posterior inclui verificações adicionais de foco/oclusão e os trechos não são replays idênticos; a redução de chamadas confirma a cadência de apresentação menor.

A regressão `GridShiftV7Checks.Batch` terminou com SUCCESS e nenhuma exceção de jogo capturada. O relatório `regressao-entregas.txt` inclui atribuição por peso, capacidade, upgrades, pré-requisitos, custos, teclas/captura/conflitos, tutorial, mapa, salvamento, transição ao Office, três entregas físicas concluídas, pagamento único, renovação idempotente de ofertas e salvamento após conclusão. Ela valida a lógica; seus próprios verificadores suspendem câmeras e não servem como benchmark gráfico.

A compilação final dos scripts também foi concluída com saída 0 e sem erros C#, conforme compilacao.txt. Os scripts e opções foram instalados no projeto principal; os testes de jogo ocorreram na cópia de validação.

