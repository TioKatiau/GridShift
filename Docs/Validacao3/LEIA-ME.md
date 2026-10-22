# Validação da atualização 3.0

Executada em Unity 6.0.83f1, em uma cópia independente do projeto. Os scripts e as cenas aplicados no projeto foram comparados com a cópia de validação. Não foi automatizada a janela do Unity do usuário.

## Play Mode

`play-mode.txt` contém o resultado completo:

- Salto nos pisos reais de SampleScene e Office, subida, aterrissagem e liberação do próximo salto.
- Eventos programáticos do InputSystem mantendo Espaço pressionado: exatamente um impulso em cada cena. A entrega dos eventos foi controlada pelo teste para não depender do foco da janela em execução sem interface.
- Modelos variados com controlador Idle/Walk associado.
- Caminho da calçada até uma porta real, sem atravessar a fachada.
- Dois pedestres com destinos opostos conseguem passar um pelo outro.
- Três minutos de circulação simultânea de veículos e pedestres; 37 travessias concluídas nessa execução. A cada segundo foi verificada a ausência de penetração relevante entre os volumes de veículos e pedestres.
- Redução da população na madrugada e aumento durante a manhã, inclusive enquanto o Office estava ativo.
- Luzes noturnas dentro do orçamento de distância/quantidade e desligamento ao amanhecer.
- Três ciclos de acesso e saída do terminal, restaurando câmera local, rotação, FOV, controles e posição do personagem, com tela preta fora do uso.
- Cidade preservada e relógio avançando no Office; ausência de gerenciador de pedestres no Office.
- Nenhuma mensagem de erro ou exceção capturada pela execução final.

## Editor e física isolada

`editor.txt` e `travessias.txt`: referências de scripts nas duas cenas, 24 travessias individuais completas e dez casos de trânsito com vermelho, verde, amarelo, obstáculos, saída ocupada, conexões e curvas.

`filas.txt`: teste adicional em Play Mode com três veículos. Confirma retenção no vermelho, distância mínima entre eles, retomada progressiva e saída de toda a fila após o verde.

`configuracao.txt`: 4.735 pontos, 12.424 ligações, 24 travessias, 12 acessos às casas, dez modelos e 127 postes identificados. Os acessos adicionais ficam fora das rotas normais de spawn.

## Imagens

Os seis PNGs foram renderizados pelo Unity com URP e revisados: cidade de dia e à noite, pedestre, fachada de casa, terminal ativo e retorno à câmera do Office. Não foram gerados por IA nem representam uma página web simulada.

## Limites

A página respondeu HTTP 200, conforme `url.txt`, mas a WebView não está instalada. Cliques, rolagem, teclado e formulários do site dentro do jogo continuam pendentes da importação e validação do SDK descrito em `../ATUALIZACAO_3_0.md`.

Não houve medição de FPS ou build de distribuição. As capturas e os testes de física não substituem uma verificação de desempenho no hardware de destino. As verificações periódicas de colisão não garantem todos os cenários possíveis de densidade ou futuras alterações do mapa.
