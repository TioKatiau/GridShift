# GridShift 11.0 — saves, perfis e central nativa

Implementação aplicada ao projeto em 27/10/2026. O gerenciamento usa os mesmos DeliveryManager, CompanyProgression, GridShiftCompany, GridShiftFinance e GridShiftMissions; tablet e computador compartilham esses dados.

## Como jogar

Entre no perfil local preenchendo e-mail e senha. Os dois campos precisam estar preenchidos; não há validação em servidor. A senha é descartada na confirmação. No menu principal, “Trocar perfil” permite escolher outra identificação.

“Novo Jogo” abre os cinco slots e o nome da empresa. Slots ocupados exigem confirmação. A partida nova executa a introdução pulável e começa no Office. “Continuar” procura o save válido mais recente do perfil. “Carregar Jogo” permite escolher qualquer slot válido, consultar dados e excluir com confirmação. A barra de espaço pula a abertura visual GS.

Na pausa, “Salvar Jogo” abre os destinos e pede confirmação de sobrescrita. “Menu Principal” salva a sessão antes de encerrá-la. O menu mantém configurações, controles e créditos da equipe SENAI.

G abre/guarda o tablet 3D; E acessa o computador com zoom suave; ESC fecha a interação e restaura câmera e controles. A Central de Operações é Unity UI e possui Visão Geral, Entregas, Frota, Financeiro, Funcionários, Missões, Upgrades, Mapa e Configurações. Criar/aceitar entregas, pagar contas, contratar/desligar, abastecer/reparar, remapear objetivos e comprar melhorias chamam os gerenciadores reais.

## Arquivos de save e perfis

No Windows, com as configurações atuais do projeto:

`C:\Users\Gb191\AppData\LocalLow\DefaultCompany\GridShift\Profiles\<identificador-do-perfil>\slot-01.json`

Há cinco arquivos independentes, de slot-01.json a slot-05.json. O diretório é Application.persistentDataPath/Profiles. O identificador resulta de SHA-256 do e-mail normalizado; o e-mail não é incluído no save. Esse identificador organiza arquivos e não funciona como autenticação ou proteção contra acesso local. Senhas não são armazenadas nem enviadas a serviços.

Cada slot tem um JSON versionado, backup .bak e miniatura .json.png. A escrita usa arquivo temporário e substituição atômica. A leitura valida os dados, rejeita versões futuras desconhecidas e tenta o backup quando o principal é inválido. Um erro não exclui os outros slots. O save anterior gridshift-session.json permanece intacto; “Importar partida anterior” permite trazê-lo para um slot vazio do perfil escolhido.

O formato v3 preserva empresa, saldo, reputação, atos, missões, recompensas, seleção de objetivo, funcionários, salários, contas, histórico, upgrades, relógio, calendário, tempo jogado, cena, posições do jogador, seleção de veículo e entregas. A frota contém ID, tipo, modelo lógico, capacidade, rota estável, segmento, posição/rotação, carga coletada e tempo restante de carga/descarga. Combustível e condição estão no estado empresarial.

Os formatos v1/v2 continuam legíveis. Como não possuíam posições lógicas da frota, entregas antigas sem dados de veículo retornam à fila preservando IDs e pagamentos. No v3, os veículos retomam a operação; rotas e modelos são validados antes da restauração. A frota reaproveita veículos existentes do trânsito.

O autosave atualiza somente o slot ativo, com intervalo padrão de 90 segundos, configurável em DeliveryPersistence. Eventos relevantes solicitam gravação com debounce de 0,6 segundo. A gravação espera estados estáveis, e a ida ao menu força o save. Não há autosave de uma sessão sem perfil/slot selecionado. JSON e miniaturas são produzidos em pontos controlados; a serialização e a leitura da pequena miniatura ainda usam a thread principal.

## Acompanhamento e HUD

O erro de retorno do Office estava no momento de localizar o avatar: a cena se tornava ativa antes de o jogador da cidade ser reativado. DeliveryPresentation agora também encontra o avatar inativo e vincula seu HUD ao trocar de cena.

A missão selecionada é persistida por ID. Desmarcar limpa o texto; selecionar novamente restaura o objetivo. O acompanhamento de veículo e entrega tem IDs próprios, é salvo e não é substituído pela simples criação de outra entrega. Veículos são resolvidos na frota atual, evitando referências destruídas.

O mapa permite acompanhar, parar, centralizar no veículo, consultar destino e voltar ao jogador. Os marcadores mantêm a projeção da câmera; a rota disponível é desenhada como uma linha ciano fina, sem setas gigantes. O HUD permanece em World Space, preso à câmera, com os movimentos e indicadores anteriores. Notificações têm fila pequena, deduplicação e fade próprios, sem desativar o painel de objetivos.

## Migração e arquivos

Cenas modificadas: MainMenu, SampleScene e Office. As rotas receberam IDs estáveis; foram retirados componentes e objetos exclusivos de navegador. O modelo TabletViewModel foi preservado.

Removidos das dependências: dev.voltstro.unitywebbrowser, dev.voltstro.unitywebbrowser.engine.cef e dev.voltstro.unitywebbrowser.engine.cef.win.x64. Adaptadores, ponte JavaScript, servidor de página local, assets de entrada/comunicação e PortableBrowser foram arquivados fora de Assets. O HTML antigo também foi arquivado. Nada foi editado manualmente em Library/PackageCache.

Backup anterior à mudança: Docs/Backup11/20261027. Fontes antigas: Docs/Legacy11. A relação completa de scripts criados, alterados e arquivados está em Docs/Validacao11/arquivos.txt. Os novos arquivos principais são GridShiftProfiles, GridShiftSavesUI, GridShiftSaveData, GridShiftSaveValidation, GridShiftSaveThumbnail, GridShiftUI, GridShiftNativeOperations, GridShiftNativeMapDrag, GridShiftMapRouteGraphic e GridShiftSession.

## Validação executada

Docs/Validacao11/runtime.txt registra RESULT SUCCESS, sem erros de execução capturados. Foram exercitados login nativo, cinco slots, confirmação de sobrescrita, criação real de entrega pelo tablet, todas as abas nativas, pagamento de conta, câmera/ESC do computador, compra de upgrade, save de entrega em andamento, retorno ao menu e carregamento no Office. O veículo restaurado concluiu fisicamente a entrega, e a recompensa permaneceu única. Missão e veículo acompanhados sobreviveram a tablet, pausa, mapa, transição e carregamento. Foram verificados slots independentes, perfis separados, exclusão confirmada e recuperação por backup.

Docs/Validacao11/audit.txt registra RESULT SUCCESS: as três cenas não apresentam componentes antigos nem scripts ausentes, os IDs de rotas são únicos, não há rótulos DEMO e as dependências de cenas/tablet não incluem o navegador. Um JSON v1 sem campos novos foi lido e convertido para um slot v3 válido.

As capturas e métricas estão na mesma pasta. A medição no editor DX11, cidade+Office/tablet, usou 10 segundos de aquecimento e amostra de 8 segundos. A referência Chromium utilizou about:blank; a versão nova exibiu o painel nativo. Memória alocada Unity: 414.035.017 e 391.413.526 bytes. Memória reservada: 846.303.232 e 858.808.320 bytes. CPU do editor: 2.775,39 e 3.033,20 ms/s. Frames médios: 4,31 e 4,19 ms. Os replays e conteúdos diferem; não há conclusão de ganho global de FPS. A API de working set do editor retornou dados indisponíveis, e a contagem global de processos CEF da referência não identifica isoladamente os processos deste projeto.

## Compatibilidade e limites

Os testes de jogabilidade e auditoria acima rodaram no Unity 6000.0.83f1. A exportação nessa versão encontrou uma incompatibilidade prévia entre o formato dos assets URP e a URP resolvida pelo editor. A verificação de build com Unity 6000.3.26f1 é registrada separadamente; o projeto original não teve seus parâmetros gráficos rebaixados.

A regressão completa também passou no Unity 6000.3.26f1, com RESULT SUCCESS e sem erros capturados. Essa execução identificou e permitiu corrigir a ausência de CanvasRenderer no desenho da rota. A inicialização dos carros foi ajustada para sincronizar Transform e Rigidbody depois da ativação do objeto; veículos ainda inativos não inicializam a rota prematuramente. A validação de posição dos veículos ativos permanece habilitada.

O guia/tutorial agora faz parte do save e usa chaves separadas por perfil e slot. Ao trocar de partida, as observações da central são reiniciadas, evitando herdar etapas de outra empresa. As transições incluem abertura GS, introdução pulável, fade/carregamento entre partidas, seleção de slot, animação do tablet, zoom/restauração do computador e notificações com fade.

As métricas anteriores comparadas no mesmo editor estão preservadas em native-performance-unity6000.0.txt. A amostra adicional no Unity 6000.3 registrou 405.834.190 bytes alocados, 881.758.208 reservados, CPU do editor de 3.371,09 ms/s e média de 4,33 ms por frame. Ela não constitui comparação direta de desempenho entre versões do Unity.

Executável Windows de desenvolvimento: Builds/GridShift11_20261027/GridShift.exe, exportado com Unity 6000.3.26f1. Build concluído com zero erros, 285.974.455 bytes e ausência de binários CEF/UWB. O teste do executável também passou: Office pronto, uma câmera principal ativa e nenhum erro capturado. A captura player-saves.png foi conferida visualmente, exibindo dados reais e miniatura do slot, os espaços vazios e o botão Voltar. O teste utiliza um perfil temporário, separado das partidas do usuário. Os scripts entregues conferem com os da cópia compilada. Não é necessário preencher referências manualmente; a limitação de exportação pelo Unity 6000.0 permanece registrada em build-unity6000.0.txt.

Não foram reivindicadas medições de GPU, teste prolongado em hardware final, validação de todas as proporções ultrawide ou conclusão completa dos atos posteriores da campanha. O trabalho 11.0 preserva os sistemas da campanha já existentes. Não há referências de UI para preencher manualmente. Um editor que já mantinha uma cena antiga aberta deve recompilar e recarregar a cena para receber a migração gravada em disco.
