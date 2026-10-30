# Navegador do computador — Office

> Este registro descreve a integração anterior. A atualização 5.0 acrescenta zoom de 150%, navegação e tablet e mantém uma superfície comum de aproximadamente 1600 × 1489 pixels. Consulte `ATUALIZACAO_5_0.md` para a configuração atual e a limitação de redimensionamento do CEF instalado.

O UrbanLink Flow usa o Unity Web Browser/CEF instalado no projeto. A página é renderizada e interativa, sem imagem estática substituindo o site.

## Configuração

- `Old Monitor 2`: `ComputerInteraction` e `GridShiftUwbTerminal`.
- `TerminalScreen/BrowserViewport/UrbanLinkBrowser`: imagem, navegador e roteamento de entrada.
- `Assets/Generated/Terminal`: configuração própria de entrada e comunicação, sem editar PackageCache.
- Menu `GridShift > Computador > Configurar UWB no monitor`: reaplica o encaixe e as referências na cena Office salva.

O encaixe usa a geometria do vidro do monitor. Nesse modelo a área útil é quadrada: a configuração inicial adapta os 1600 pixels de largura para **1600 × 1600 pixels reais**, a 30 FPS. No Inspector do adaptador, desative `Match Monitor Aspect` para usar exatamente `Resolution`, por exemplo 1600 × 900 ou 1920 × 1080; a imagem mantém a proporção com espaço livre nas bordas. `Actual Resolution` mostra a textura real durante o uso. Alterações de resolução são encaminhadas ao Chromium.

A interação usa exclusivamente o novo Input System. O adaptador próprio também evita o acesso legado ao IME presente no handler fornecido pelo pacote instalado. O EventSystem existente utiliza InputSystemUIInputModule. A imagem recebe cliques através do GraphicRaycaster; a conversão de coordenadas usa a câmera do jogador e as dimensões reais da textura.

E abre o computador com a interação existente. ESC libera teclado e mouse, oculta a página e executa o retorno da câmera, sem abrir pausa junto. As teclas digitadas no site não acionam a porta próxima. O ponto de vista e a duração da animação existentes foram preservados.

## Validação

Testes executados numa cópia isolada do projeto, com Unity 6000.0.83f1 e Chromium real. Consulte `ValidacaoNavegador/resultados.txt` e as capturas nessa pasta.

- Carregamento do site real e clique em seu botão de acesso, com navegação efetiva.
- Correspondência de nove posições visuais com as coordenadas do navegador.
- Textura e viewport Chromium em 1600 × 1600, 1600 × 900 e 1920 × 1080.
- Clique esquerdo, direito, arraste, scroll, texto e acentos, Backspace, Tab, Enter, Ctrl+A e copiar/colar pelo Chromium. Os casos de formulário usam uma página diagnóstica temporária apenas no teste, sem substituir o site do jogo.
- Digitação continua com o cursor fora da tela; E/W/Shift/Espaço não acionam porta, movimento ou pulo durante o uso.
- ESC, restauração da câmera e FOV, ausência de captura após sair e duas reentradas.

Os testes usam dispositivos de entrada simulados e renderização real na cópia isolada. Não constituem teste manual no editor aberto, avaliação de desempenho em diferentes máquinas, nem validação de todos os formulários externos ou métodos de composição de todos os idiomas. Nenhum formulário de conta foi enviado.
