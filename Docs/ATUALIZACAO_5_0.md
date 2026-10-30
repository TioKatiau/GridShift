# GridShift — atualização 5.0

## Como usar

- **E**: computador do Office, com a aproximação e o retorno de câmera existentes.
- **G**: abrir ou guardar o tablet. **ESC** fecha a interação antes de permitir a pausa.
- Barra do navegador: voltar, avançar, recarregar e Home. Os estados dos botões vêm do histórico real do Chromium, inclusive navegação interna do site.
- Configurações > Jogabilidade > Movimento do HUD: reduzir ou desligar a oscilação. As opções de câmera e head bob existentes continuam disponíveis.

O site permanece `https://urbanlink-flow.onrender.com`. O zoom inicial é **150% nativo do Chromium**, ajustável em `Page Zoom`. A confirmação foi feita pela escala de pixels e pelo viewport real da página, pois a consulta de zoom do pacote instalado retorna zero nessa integração. Não há zoom CSS adicional.

## Navegador e tablet

O tablet possui corpo 3D, moldura grafite, vidro e câmera frontal, com tela quase quadrada. Esse formato mantém a mesma superfície virtual do monitor e permite transferir o **mesmo navegador** na Office, preservando histórico e sessão. Não são criados dois motores para computador e tablet.

Na cidade, o navegador portátil é criado no primeiro uso e reutilizado nas próximas aberturas. Ao suspender o avatar para carregar Office, esse navegador é descartado. Na Office, o tablet empresta o navegador do computador e devolve sua hierarquia ao fechar. O relógio e a simulação permanecem ativos.

A superfície usa aproximadamente **1600 × 1489 pixels**, descontada a barra de navegação, e limite de 30 FPS do navegador. O CEF instalado apresentou quadros pretos após determinados redimensionamentos durante o uso; a troca entre os dispositivos evita esse caminho mantendo as dimensões iguais. Para alterar a qualidade, prefira configurar as resoluções iniciais dos dispositivos/prefab antes do Play e mantê-las iguais. O redimensionamento arbitrário durante a sessão continua sujeito à limitação do pacote, sem alteração de PackageCache.

`PortableTablet` permite configurar posição, rotação, escala, duração, URL, zoom, resolução e velocidade de scroll. A movimentação fica bloqueada por padrão. A opção `Allow Walking` permite passos com **setas**, mantendo as letras disponíveis para digitação e a câmera bloqueada. Não há mãos artificiais: o modelo funciona como ViewModel ligado à câmera.

## HUD e jogador

O controlador `HUDParallax` combina o efeito existente de mouse com respiração e passos suaves. Um objeto pai `HudMotion` recebe esse efeito; os objetos originais do HUD, suas referências e o zoom do painel permanecem no filho. Isso impede que dois controladores disputem a mesma posição.

As velocidades foram configuradas a partir dos valores existentes: caminhada **5 → 5,4 m/s** (+8%) e corrida **8 → 9 m/s** (+12,5%). O efeito de corrida usa FOV adicional de 4 graus e amplitude de head bob de 0,042, modulada pelas preferências existentes. Esses efeitos ficam suspensos enquanto os controles estão bloqueados por interações.

## Office e semáforos

A cobertura possui collider e abrange todos os pisos medidos, com face inferior em aproximadamente **3,85 m**. Foram criadas **20 luminárias locais**, com spots quentes, alcance de 4,7 m, intensidade 6 e sombras duras no nível baixo de resolução. `OfficeInteriorLighting` permite ajustar intensidade, cor, alcance e sombras. Não altera RenderSettings nem a iluminação ambiente da cidade. A configuração móvel do URP pode desativar sombras adicionais conforme seu perfil de qualidade.

Os **57 semáforos** receberam o mesmo reparo de referências, renderers e materiais das três cores. Os materiais HDR usam o Bloom já existente para uma luminosidade discreta, sem acrescentar uma nova luz dinâmica por lâmpada. Os grupos e os seis estágios existentes foram verificados para impedir verdes conflitantes; não foi alterado o algoritmo de condução ou o grafo de pedestres.

## Arquivos

Criados em `Assets/Scripts`: `PortableTablet.cs`, `GridShiftBrowserNavigation.cs`, `GridShiftRefreshIcon.cs` e `OfficeInteriorLighting.cs`.

Modificados: `GridShiftUwbTerminal.cs`, `ComputerInteraction.cs`, `PauseMenu.cs`, `GamePreferences.cs`, `HUDParallax.cs`, `HudBottomdZoom.cs` e `PlayerCameraMotion.cs`. O controlador de movimento mantém seu algoritmo de colisão, gravidade e salto; as velocidades são valores das cenas.

Criados em `Assets/Scripts/Editor`: `GridShiftV5Setup.cs` e `GridShiftV5Checks.cs`. Os verificadores existentes `GridShiftUwbChecks.cs` e `GridShiftV4Checks.cs` também receberam casos de regressão.

Cenas configuradas: `Office.unity` e `SampleScene.unity`. Prefabs novos: `Assets/Generated/Experience5/TabletViewModel.prefab` e `PortableBrowser.prefab`. Materiais novos nessa pasta: `Graphite`, `TabletGlass`, `OfficeCeiling`, `FixtureRim` e `FixtureGlow`. Os materiais `Red`, `Amber` e `Green` em `Assets/Generated/TrafficSignals` foram padronizados. Os prefabs originais dos assets importados foram preservados.

As referências de jogadores, câmeras, telas, navegador, entrada e prefabs são configuradas automaticamente. O menu `GridShift > Atualização 5.0 > Configurar experiência` reaplica a configuração às cenas salvas; use fora do Play. Não é necessário arrastar referências manualmente.

## Validação e limites

Relatórios e capturas ficam em `Docs/Validacao5`. Os testes usam uma cópia isolada do projeto, Unity 6000.0.83f1, Chromium real e dispositivos de entrada simulados. Não são testes manuais na janela aberta nem uma medição de desempenho em outras máquinas. Nenhum formulário de conta foi enviado.

São verificados o site real, zoom, entrada, histórico, fechamento e restauração, a viagem cidade–Office–cidade, a conservação do gerente de trânsito, iluminação às 12h e 23h, cobertura dos pisos, três cores dos sinais e ausência de verdes conflitantes. A regressão do jogador mede saltos, aterrissagem, corrida, câmera, pausa e desligamento da oscilação do HUD. As capturas do navegador também verificam conteúdo efetivamente renderizado, para não tratar apenas DOM carregado como tela funcionando.

Ajustes estéticos opcionais continuam no Inspector: zoom entre 125% e 150%, posição do tablet, intensidade de HUD/câmera e iluminação. Antes de jogar na janela já aberta do editor, reabra a cena salva para carregar as referências atualizadas.
