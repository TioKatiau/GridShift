# GridShift 7.0 — Operação, tutorial e progressão

## Uso rápido

Abra MainMenu e inicie/continue a sessão. A cidade e o Office continuam usando a arquitetura existente.

- **W/A/S/D**: caminhar; **Shift esquerdo**: correr; **Espaço**: pular.
- **E**: interação com entrada do Office e computador, quando o aviso estiver disponível.
- **G**: tablet; **J**: abre o tablet diretamente no painel local de operações.
- **M**: ampliar/reduzir mapa; roda do mouse ajusta o zoom no mapa ampliado.
- **F1**: tutorial; **F2**: controles; **U**: upgrades da empresa.
- **ESC**: fecha a interface atual antes de abrir pausa. Funciona como saída de segurança mesmo após remapear a ação Pausa.

As teclas podem ser remapeadas em F2. O guia e o rodapé do painel usam os valores atuais. Letras digitadas no navegador não movimentam o avatar nem abrem outras telas. G só guarda o tablet quando não há campo em edição; ESC sempre permite sair.

## Central de operações

A página local foi reorganizada em Operação, Disponíveis, Histórico, Upgrades e Como jogar. Informações detalhadas ficam no computador/tablet; o HUD oferece somente um resumo.

Operação mostra entregas ativas, veículo atribuído, retirada, destino, progresso estimado, distância, recompensa, prazo e mapa esquemático das posições reais. O formulário de novo pedido fica recolhido até o jogador clicar em Nova entrega. Disponíveis oferece contratos para aceitar; cada contrato usa locais válidos, cria uma DeliveryOrder real e só pode ser aceito uma vez. Ao aceitar as seis ofertas do lote, outro lote é disponibilizado com IDs novos e outras origens da rede; pedidos antigos não são duplicados. Histórico separa as conclusões.

Há seis modalidades: simples, pesada, urgente, prioritária, longa e local. Ofertas pesadas usam cargas maiores; ofertas longas buscam destinos mais distantes e locais buscam destinos próximos. Pedidos urgentes têm prioridade de despacho e bônus de 40% se concluídos em até 900 segundos da simulação; após o prazo recebem a recompensa normal. Prioritários têm prioridade de despacho e bônus de 20%. O peso e a capacidade continuam determinando o tipo de veículo, independentemente do rótulo escolhido no formulário. Uma modalidade personalizada não obriga o veículo a percorrer distância artificial.

A confirmação é validada no DeliveryManager. Reenvios por requestId e aceites por offerId são idempotentes. Atualizações continuam a cada segundo, sem envio por quadro. A página preserva o clique em andamento quando recebe um snapshot para não substituir um botão entre pressionar e soltar o mouse.

## Login demonstrativo

Antes do painel aparece uma identificação local com usuário/e-mail, senha temporária e modo visitante. **Não é autenticação online e não verifica uma conta ou senha cadastrada.** O próprio formulário explica isso.

O nome deve ter entre 1 e 80 caracteres. O formulário exige pelo menos quatro caracteres temporários no campo de senha; essa senha é descartada no JavaScript, não é enviada à Unity e não é salva. Experimentar modo demo identifica o operador como Visitante DEMO. A ponte local recusa comandos de gerenciamento antes dessa identificação. Sair do painel encerra a identificação, preservando os pedidos e a empresa. A identificação dura a sessão, não é um perfil de salvamento multiusuário.

O site Urban Flow publicado e seu login permanecem separados. Não houve alteração no backend remoto, armazenamento de credenciais reais ou criação de endpoints fictícios. A mesma restrição de origem exata e token da sessão, implementada na versão anterior, continua protegendo a ponte UWB. Uma integração futura deve substituir a identificação demo por autenticação autorizada com validação no servidor.

## Tutorial real

O guia tem dez etapas: movimento, corrida, tablet, Office, acesso à demo, primeiro pedido, seleção da frota, mapa, upgrades e controles.

Na primeira utilização, uma caixa compacta mostra a etapa atual enquanto o personagem continua jogável. As etapas observam ações reais: MovementInput, IsRunning, tablet aberto, presença no Office, identificação local, pedidos existentes, veículo designado, mapa visitado, upgrade comprado e tela de controles consultada.

F1 abre a explicação completa. Nessa tela, o jogador pode voltar à prática, avançar manualmente para consultar as etapas, pular ou reiniciar. Etapa, conclusão e preferência de pular são salvas em PlayerPrefs. Pular não registra conclusão; F1 permite reabrir depois. No painel web, Como jogar usa as teclas remapeadas e permite preparar a reabertura do tutorial; ele assume o foco após guardar o dispositivo.

## Destinos e expansão

O catálogo passou de **31 para 59 locais acessíveis**, preservando os IDs anteriores. São 41 pontos iniciais e 18 parceiros desbloqueáveis: dez no primeiro nível e oito no segundo. Há residências, prédios, comércios, padarias, oficinas, mercados, cafeterias e posto, conforme os modelos existentes.

Novas paradas são projetadas em trechos retos da rede atual, com piso de rua, volume livre, distância mínima de outras paradas e margem de 12 metros antes do cruzamento/faixa. A ferramenta também verifica conexão dirigida com a rede. Os marcadores no mundo ficam próximos ao acesso do estabelecimento, fora do volume das construções.

**Não foram adicionadas ruas ou quarteirões nesta etapa.** A expansão consiste em tornar a cidade atual mais útil e preparar os pontos com níveis de acesso. Isso evita criar uma extensão sem rotas, sinalização ou pedestres integrados. A ferramenta GridShift → Atualização 7.0 → Configurar destinos e minimapa automatiza o catálogo e as máscaras das câmeras; use com cenas salvas, fora do Play Mode. O limite de parceiros respeita os pontos realmente existentes.

## Upgrades com efeitos reais

CompanyProgression mantém o catálogo e os níveis. O custo inicial e o efeito são:

- **Ampliar frota — R$ 150**: mais um carro da empresa por nível, adotado do tráfego existente. Não instancia veículos extras; aguarda um carro elegível quando necessário.
- **Capacidade reforçada — R$ 120**: +25% da capacidade base por nível, aplicado à frota. Caminhão inicial de 2.500 kg passa a 3.125 kg no primeiro nível.
- **Doca eficiente — R$ 100**: tempo de carga/descarga multiplicado por `1 / (1 + 0,3 × nível)`.
- **Central de despacho — R$ 140**: mais duas missões simultâneas por nível, partindo de três. Vagas sem veículo compatível não criam veículos automaticamente.
- **Contratos melhores — R$ 180**: +15% de recompensa por nível nos novos pedidos; requer Doca eficiente nível 1. Não altera retroativamente a recompensa de pedidos existentes.
- **Rede de parceiros — R$ 130**: libera os pontos do nível seguinte. No catálogo atual, há dois níveis com efeito real.

O custo de um nível seguinte é `custo inicial × (1 + nível atual × 1,5)`. Os demais upgrades têm até três níveis. Saldo insuficiente, pré-requisito não atendido e nível máximo impedem a compra. Os efeitos de velocidade, novos modelos de caminhão, decoração do Office e bairros inteiros não foram inventados como upgrades sem implementação.

O dinheiro é descontado uma vez por compra válida. Níveis são gravados no salvamento existente e restaurados sem aplicar multiplicadores repetidamente. Pedidos e saldo antigos permanecem legíveis no formato de save versão 1; campos novos usam valores padrão. Ao reabrir o aplicativo, pedidos pendentes continuam retornando à fila conforme o comportamento anterior. Na viagem cidade/Office dentro da sessão, trajetórias, pedidos, relógio e upgrades permanecem em andamento.

## HUD e mapas

O painel de entregas continua em Canvas world space, ligado à câmera, com tema escuro/amarelo e posição calculada no canto superior direito. Mostra ativas/concluídas e um resumo da entrega selecionada. O destino é abreviado, e notificações duram alguns segundos. Ele desaparece ao usar dispositivos ou interfaces modais. A composição existente de dinheiro, bússola, hora e minimapa foi preservada.

Foram removidas as malhas elevadas de carro/seta e destino que cresciam na perspectiva da câmera do minimapa. Agora os símbolos são elementos UI projetados pela câmera real: jogador vermelho, veículo amarelo e pequeno losango ciano de destino. O tamanho é proporcional à área do mapa, não à distância entre a câmera e um objeto 3D. Somente a frota em missão recebe ícones.

A câmera do minimapa exclui HUD 3D, corpo do jogador, antigas malhas DeliveryMap e anéis de missão. Os anéis no chão permanecem visíveis na cidade, numa camada DeliveryWorld separada. O ícone antigo do jogador dentro do minimapa é desativado para evitar duplicação; o ponteiro da bússola permanece.

M abre um mapa maior com transição, ruas da textura real, pontos liberados, frota e destino selecionado. É possível ajustar zoom, centralizar no jogador e voltar a ver a cidade toda. A câmera usa vista ortográfica voltada para o norte nesse modo e restaura a configuração do minimapa ao fechar. O mapa inclui a extensão das rotas e os destinos; no Office, usa a posição do avatar preservado na cidade. Não há arrasto do mapa nesta versão.

## Controles e foco

GridShiftControls centraliza as ações. A tela F2 separa movimentação, interação, interface, empresa e sistema, mostra a tecla atual, permite capturar outra tecla e restaurar padrões. Duplicatas são recusadas. ESC cancela uma captura; ela também termina após 12 segundos. As preferências são gravadas e recarregadas.

GridShiftOperationsUI controla o foco das novas telas e restaura o estado anterior do personagem/cursor. Não abre telas sobre o navegador enquanto o jogador digita. Tutorial modal, controles, mapa e upgrades bloqueiam movimento. O coach não modal permite praticar. Tablet e computador mantêm seu bloqueio já existente. ESC consome o evento antes de a pausa processá-lo. Controles abertos a partir da pausa retornam à pausa, mantendo o estado do tempo. A simulação continua com mapa/guia/upgrades abertos; a pausa continua pausando.

## Arquivos

Criados: GridShiftControls.cs, GridShiftOperationsUI.cs, GridShiftTutorial.cs, CompanyProgression.cs, GridShiftMapMarkers.cs e os verificadores/ferramenta Editor/GridShiftV7Setup.cs, GridShiftV7Checks.cs, GridShiftV7NativeChecks.cs e GridShiftV7FinalChecks.cs.

Modificados: DeliveryData, DeliveryManager, DeliveryPoint, DeliveryPersistence, DeliverySaveStore, CompanyVehicle, DeliveryPresentation, GridShiftDeliveryBridge, GridShiftBrowserNavigation, GridShiftUwbInputRouter, PlayerMovement, PortableTablet, ComputerInteraction, CompanyEntrance, PauseMenu, MinimapCamera e StreamingAssets/GridShiftDemo/index.html.

Cenas: SampleScene (catálogo e câmeras) e Office (câmeras). MainMenu foi preservada. TagManager recebe DeliveryWorld. Não foram substituídos prefabs de veículo, tablet, navegador, cidade ou personagem. As novas telas e símbolos UI são construídos em execução, sem dependência de arrastar referências manualmente.

## Verificação e limites

A validação foi executada em uma cópia separada com Unity 6000.0.83f1, sem controlar a janela principal do Editor e sem alterar as preferências/salvamentos reais do jogador. Relatórios e capturas estão em Docs/Validacao7. Os arquivos de cena do projeto principal são comparados à cópia de segurança antes de aplicar a configuração, para preservar edições salvas durante o trabalho.

O teste de lógica verifica destinos, ofertas idempotentes, atribuição por peso, capacidade, custo/efeito/pré-requisitos dos upgrades, frota adotada do tráfego, limite de missões, tutorial, teclas, prioridade de M/G/ESC, salvamento, Office e conclusão física sem pagamento duplicado. O teste nativo usa Chromium, mouse/teclado virtuais e capturas da Unity para verificar as telas e a ponte.

Ainda é recomendável testar uma build Windows no computador final, especialmente legibilidade em outras resoluções e preferências pessoais de HUD. A sincronização online continua dependendo do backend autorizado. Não há novas ruas, modelos de veículo, decoração de Office ou autenticação de conta online nesta atualização.

### Resultados executados

- **v7-core.txt — SUCCESS**: atribuição real de 5/50/500 kg; seis modalidades; aceite idempotente e renovação de lotes; custos, pré-requisitos e efeitos de upgrades; frota adotada; limite simultâneo; M/G/ESC, captura de tecla, conflito, releitura e restauração; avanço/pulo/reabertura/conclusão do tutorial; salvamento; viagem ao Office e três conclusões físicas com pagamento único. Nenhuma exceção capturada.
- **v7-play.txt — SUCCESS**: CEF real a 200%, login local com descarte da senha, bloqueio da ponte antes de entrar, contrato aceito pelo mouse, pedido personalizado de 500 kg, guia com teclas atuais, saída e visitante, PC/tablet compartilhados, G durante edição e M sem abrir telas indevidas. As duas entregas concluíram fisicamente; o histórico refletiu as conclusões e uma compra pelo navegador descontou R$ 120 e elevou o caminhão para 3.125 kg. Nenhuma exceção capturada.
- **v7-structure.txt — SUCCESS**: MainMenu, SampleScene e Office sem scripts ausentes; 59 paradas alinhadas e antes dos cruzamentos; leitura de salvamento 6.0 e de níveis salvos pela versão 7.0.
- **v7-visual.txt — SUCCESS**: mapa ampliado voltado ao norte, ações de centralização visíveis, HUD oculto durante a interface e capturas finais de HUD/minimapa, mapa, controles, tutorial e upgrades.

As capturas finais estão na pasta Imagens. Os testes nativos usam dispositivos virtuais do Input System e a mesma ponte do produto. Não foram usados credenciais reais ou dados de contas do Urban Flow publicado. Para carregar as configurações de cena novas no Editor principal, abra MainMenu e inicie a sessão; uma SampleScene que já estivesse aberta na memória precisa ser reaberta antes de testar diretamente.