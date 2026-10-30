# GridShift 13.0 — áudio

Esta versão acrescenta uma biblioteca original por síntese procedural e integração de áudio ao jogo existente. Não modifica as regras de trânsito, rotas, colisões, economia ou o formato dos saves. As alterações de gameplay são chamadas de áudio após eventos existentes; os novos componentes observam o movimento real.

## Arquivos e origem

- `BIBLIOTECA.md`: nomes, caminhos e duração de todos os 63 WAVs.
- `audio-manifest.json`: origem, licença, canais, duração e SHA-256 por arquivo.
- `Assets/Audio/LICENSE.txt`: dedicação CC0 dos WAVs originais. Não inclui código, imagens ou dependências.
- `Assets/Audio/Mixers/GridShift.mixer`: mixer editável da Unity.
- `Assets/Resources/GridShiftAudioLibrary.asset`: referências explícitas aos clipes e canais, incluídas no build.
- `asset-validation.txt`: auditoria numérica dos WAVs.
- `runtime-validation.txt`: resultado dos testes na cópia isolada; somente os PASS registrados devem ser tratados como executados.
- `VALIDACAO.md`: contexto, limites dos testes e medidas parciais da revisão final.
- `prefabs.txt`: caminhos exatos dos prefabs configurados. Os originais anteriores a esta alteração ficam em `BackupPrefabs/`.

As três composições instrumentais são Menu_CityInMotion (90 BPM), Office_AfterHours (80 BPM) e Mission_ForwardMotion (90 BPM). São sintetizadores, baixo e percussão originais, sem voz ou amostras de terceiros. Os motores e a cidade têm linguagem sonora sintetizada: não são gravações reais de motores ou de uma cidade. Os murmúrios de NPC são não verbais e sintetizados.

## Reprodução e mixer

O gerenciador persistente `GridShiftAudio13` é instalado automaticamente antes da primeira cena. A biblioteca e o mixer são assets normais, não gerados a cada partida. O menu `GridShift > Audio 13 > Install and validate` permite refazer a configuração no editor quando necessário. A primeira criação do mixer usa os métodos de autoria do editor por reflexão porque não há API pública de criação de grupos; essa dependência fica apenas no editor. O jogo usa APIs públicas de áudio.

Master contém Music, Ambience, Vehicles, Footsteps, UI e SFX. Todos os AudioSources criados recebem seu grupo. Cada grupo expõe seu parâmetro `<Canal>Volume`. A aba Áudio das configurações oferece os sete controles, incluindo passos. A conversão é `20 * log10(volume)`, com silêncio em -80 dB. Os valores transitam gradualmente e persistem em `GS.Audio.<Canal>`. A preferência anterior `GS.Volume` migra como valor inicial do Master; AudioListener.volume permanece em 1 para evitar multiplicação duplicada.

Objetos com fontes: `GridShiftAudio13/OneShot-0` até `OneShot-9`, `Music-A`, `Music-B`, `City_Ambience_Day`, `City_Ambience_Night`, `Office_Ventilation` e `Green_Wind`; nos veículos reais, filhos `Car-Engine`/`Car-Road` ou `Truck-Engine`/`Truck-Road`. Pedestres, jogador, tablet, computador e notificações usam essas fontes reutilizadas, sem uma nova fonte em cada evento. A câmera ativa recebe/reutiliza o AudioListener; o gerenciador mantém uma reserva desabilitada quando há câmera adequada.

Menu, Office e missão especial ativa escolhem trilhas diferentes. A exploração comum privilegia o ambiente. Duas fontes de música fazem crossfade; ao carregar/transitar, a música anterior reduz gradualmente. O efeito da logo dispara uma vez na abertura. Não há trilha, narração ou edição do vídeo pitch nesta tarefa.

## Mundo e desempenho

- Veículos reais recebem `GridShiftVehicleAudio`, inclusive os reutilizados pelo pool. Há um único componente por objeto e duas fontes por veículo: motor e rodagem. São criadas uma vez, nunca a cada frame ou a cada efeito. Prefabs usados pela cena recebem o componente; os veículos já presentes também são reconhecidos em execução.
- Som 3D mono, distância mínima de 3 m para carros e 4 m para caminhões, atenuação linear até silêncio em 65 m e Doppler discreto. Pitch e intensidade seguem a velocidade pública do TrafficCar. A marcha lenta continua nos semáforos. Veículos desativados, sem condições de operar ou estacionados silenciam.
- No máximo seis veículos mantêm vozes simultâneas (até doze fontes motor/rodagem). A vaga só é reutilizada após o fade do anterior. A escolha dos mais próximos ocorre quatro vezes por segundo. A buzina só ocorre após obstáculo persistente, com intervalo longo; não dispara em cada semáforo ou para pedestres. Ré de caminhão depende do estado real REVERSING.
- Dez fontes de efeitos reutilizadas: quatro reservadas para interface e seis para mundo/feedback. Sons importantes podem substituir efeitos de menor prioridade. Não há criação de fontes por passo ou por pedestre. A configuração global de vozes da Unity foi preservada.
- Passos do jogador acumulam deslocamento horizontal real no chão. Corrida muda distância por passada e intensidade; parar, bloquear controles, pausar ou ficar no ar interrompe os passos. Salto usa o contador de saltos aceitos; aterrissagem usa a mudança real de contato com o chão. Concreto é o padrão, Office usa piso interno, Terrain usa grama. `GridShiftFootstepSurface` permite marcar explicitamente asfalto, concreto, grama ou Office em outros pisos. A classificação de todos os pisos da cidade ainda não foi feita.
- Pedestres próximos usam deslocamento real, limite de disparos e murmúrios esparsos. Distantes não ganham fontes individuais. Árvores dos prefabs usados pela cena recebem zonas de 12 m para vento, pássaros e insetos; não foram desenhadas zonas manuais de parques.
- Ambientes diurno e noturno seguem WorldSession.ClockHour com transição suave entre 5–7h e 17–19h, sem reiniciar clipes. Office reduz o exterior e introduz ventilação e música de gerenciamento. A cidade pode continuar simulando ao fundo sem manter os motores audíveis no interior.
- Um único AudioListener ativo é selecionado na câmera do jogador da cena ativa. Um listener de reserva cobre telas sem câmera. Listeners redundantes são desabilitados, sem remover câmeras.
- Trilhas usam Vorbis/Streaming; efeitos curtos e loops pequenos usam PCM/DecompressOnLoad. Não há reverberação adicional. O marcador `GridShift.Audio.Update` mede o gerenciador no thread principal, sem incluir DSP, GPU, veículos ou passos em LateUpdate. As médias registradas não representam FPS global nem comparação completa antes/depois.

## Eventos conectados

Menu, logo, hover na entrada do botão, clique, pausa, voltar, abas, tablet, computador, seleção de save, gravação manual bem-sucedida, carregamento, confirmação de sobrescrita/exclusão, exclusão confirmada e erros de criação/gravação/leitura/exclusão de slots têm feedback. Autosaves ficam silenciosos para evitar avisos repetitivos.

Criação/aceitação de entrega, designação de veículo, coleta de carga, conclusão paga uma única vez e cruzamento do prazo produzem sons distintos. Restaurar uma partida já atrasada não repete o aviso. Transações financeiras válidas, conta paga, saldo insuficiente, contratação, upgrade, aumento de reputação, confirmação de missão e desbloqueio de ato têm efeitos. Abastecimento concluído, manutenção terminada, lembrete de conta e nova demanda diária usam seus eventos reais. As chamadas respeitam os retornos de sucesso e o estado de restauração existente.

## Scripts

Criados em `Assets/Scripts/`: GridShiftAudio.cs, GridShiftAudioLibrary.cs, GridShiftAudioButton.cs, GridShiftVehicleAudio.cs, GridShiftFootstepSurface.cs e GridShiftAudioZone.cs. Em `Assets/Scripts/Editor/`: GridShiftAudioSetup.cs e GridShiftAudioChecks.cs.

Modificados apenas para integração ou controles de áudio: CompanyProgression.cs, CompanyVehicle.cs, ComputerInteraction.cs, DeliveryManager.cs, DeliveryPersistence.cs, GridShiftCompany.cs, GridShiftFinance.cs, GridShiftMainMenu.cs, GridShiftMissions.cs, GridShiftNativeOperations.cs, GridShiftSavesUI.cs, GridShiftUITheme.cs, PauseMenu.cs e PortableTablet.cs. Os arquivos anteriores à integração estão em `Docs/Audio13/Backup/`. TrafficCar, TrafficManager, PlayerMovement, WorldSession, DeliverySaveStore, GridShiftProfiles e as cenas não foram editados por esta atualização.

## Limitações e validação restante

- Falta avaliação auditiva no computador final: timbres sintetizados, balanço, conforto, emendas percebidas e espacialização devem ser ouvidos com fones e caixas. A auditoria numérica não prova qualidade percebida.
- Não foi demonstrada ausência de queda global de FPS. As amostras automatizadas são curtas e em cópia do projeto, sem comparação idêntica antes/depois, DSP, GPU ou teste longo.
- Persistência foi verificada por escrita/leitura de PlayerPrefs e inicialização do código; um fechamento e reinício manual completo do aplicativo com os sliders escolhidos ainda deve ser verificado.
- Testes de conclusão de entrega/missão podem usar fixtures isoladas nos métodos reais, conforme indicado no relatório. Eles não substituem a regressão de uma entrega dirigida fisicamente de ponta a ponta.
- Delivery_Cancelled está pronto, mas não foi inventado um sistema de cancelamento só para dispará-lo. Sons de entrada em lojas e reações específicas de NPC ficam para a segunda etapa. Erros de saves usam UI_Error; sobrescrita usa UI_Confirm. Não há timbre exclusivo para cada mensagem administrativa.
- Não foi gerado um novo executável nesta tarefa. O áudio está nos assets e scripts do projeto; builds antigos continuam com seu conteúdo anterior.

Referências de API: [AudioSource](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/audiosource) e [AudioMixer.SetFloat](https://docs.unity.cn/6000.1/Documentation/ScriptReference/Audio.AudioMixer.SetFloat.html).
