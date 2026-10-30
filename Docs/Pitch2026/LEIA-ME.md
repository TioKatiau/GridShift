# GridShift — pitch SENAI 2026

## Vídeos finais

- **GridShift_Pitch_Com_Trilha.mp4** — 1min40s, 1920×1080, 30 FPS, H.264, trilha instrumental discreta e efeitos suaves. Sem narração, dublagem, voz artificial ou TTS.
- **GridShift_Pitch_Sem_Audio.mp4** — a mesma montagem, completamente sem faixa de áudio, pronta para sua voz e sua própria mixagem.

Os dois arquivos têm logo oficial, títulos curtos, animação suave de escala, detalhes amarelos, transições de 0,4s, imagens reais e créditos dos cinco integrantes. As capturas mostram somente o jogo, sem Inspector, Console ou painéis do editor.

## Sincronizar sua voz

- 00:00–00:12: abertura e cidade.
- 00:12–00:27: trânsito, caminhões e pedestres.
- 00:27–00:47: tablet, contratos, frota, operação e mapa.
- 00:47–01:08: Office, financeiro, funcionários, upgrades e missões.
- 01:08–01:27: primeira pessoa, noite, veículos e Office.
- 01:27–01:40: panorama final, logo, slogan e créditos.

`Roteiro_Narracao.md` contém sugestões escritas de fala para cada bloco. Nenhum áudio dessas falas foi produzido. `Tempos_Detalhados.csv` registra cada tomada, título, início e fim em segundos.

## Projeto editável

Abra **GridShift_Pitch_Editavel.mlt** no Shotcut, mantendo as pastas ao lado dele. Ele organiza os segmentos e as dissolvências em uma faixa de vídeo e mantém a trilha sem voz em uma faixa de áudio separada. É possível reorganizar, cortar ou substituir clipes e ajustar a mixagem. Para gravar sua voz no editor, acrescente uma faixa de áudio.

**Limite de edição:** os títulos e o tratamento visual estão incorporados nos segmentos de `edicao/`. Os textos, tempos e ordem continuam editáveis no projeto-fonte `projeto/timeline.json`; `projeto/render.cjs` reconstrói os segmentos e os dois MP4s pelo FFmpeg. O arquivo MLT foi gerado e verificado como XML, mas não foi aberto nem testado dentro do Shotcut nesta máquina. A exportação final foi feita e validada pelo FFmpeg.

Para reconstruir: tenha Node.js e FFmpeg disponíveis, defina a variável FFMPEG se o executável estiver em outro local, e execute `node projeto/render.cjs` a partir desta pasta. Sem essa variável, o script usa o executável portátil localizado nesta máquina durante a produção. `create-graphics.ps1` recria os cartões de abertura/encerramento e `create-edit-project.cjs` recria a timeline e o CSV. Não é necessário executar esses arquivos para assistir aos MP4s ou adicionar sua voz no editor.

Copie a pasta **Pitch2026 inteira** para levar o projeto de edição a outro computador. Se o editor pedir para localizar recursos após a mudança de pasta, indique a nova pasta Pitch2026. O vídeo final isolado pode ser entregue sem as demais pastas.

## Origem das imagens

Os 19 arquivos em `clipes/` foram capturados do projeto GridShift em uma cópia isolada, rodando na Unity 6000.0.83f1 com renderização. `Arquivos_Gameplay.json` identifica os arquivos, duração e hashes. Não foram usados vídeos antigos da pasta pessoal de vídeos, imagens geradas de gameplay, cenas externas ou funcionalidades simuladas.

O código de captura ficou apenas na cópia de trabalho; seu registro está em `projeto/GridShiftPitchCapture.cs`. Ele controla enquadramentos, captura a câmera e usa ações existentes do jogo. Não foi instalado em `Assets/Scripts` do projeto original. Os scripts, prefabs e cenas originais do jogo não foram editados para fazer o pitch.

As cenas externas usam enquadramentos cinematográficos do mundo real do jogo. Tablet e computador mostram os painéis nativos. A operação usa um contrato realmente aceito e um caminhão realmente designado, em deslocamento para coleta. **Não há encenação de uma entrega concluída nem alteração de veículos para forçar trânsito.** Contratação e compra de upgrade usam o saldo inicial real da sessão de demonstração. Os horários diurno/noturno usam o ciclo já existente; a montagem não representa um dia inteiro filmado continuamente.

O fluxo de captura foi feito quadro a quadro em 30 FPS para o arquivo, sem medir ou prometer o FPS do jogo em execução normal. A renderização direta de câmera evita depender da captura da janela do editor. A cópia de trabalho usa os pacotes compatíveis resolvidos pela Unity 6000.0; os manifests do projeto original não foram alterados.

`capture-take1.log` e `capture-take2.log` registram tentativas e tomadas descartadas; `capture-take3.log` registra a passagem completa com os painéis descobertos. `capture-detalhes.log` registra o pedestre em travessia real e o enquadramento do mapa. `capture-log.txt` registra a última passagem do Office. Telas com a janela de perfil cobrindo o painel e enquadramentos com personagens encobertos foram regravados e não fazem parte da edição final.

## Áudio e licença

Foram usados apenas os arquivos originais da atualização de áudio do próprio GridShift:

- `audio/fontes/Menu_CityInMotion.wav`: instrumental original por síntese, 90 BPM.
- `audio/fontes/Logo_GS.wav`: efeito original da marca.
- `audio/fontes/UI_Tab.wav`: transição eletrônica curta.

A música fica baixa, com fade de entrada e saída. Não foram incluídos murmúrios, falas de NPC ou captação de microfone. O som da cidade não foi gravado junto com o vídeo. `audio/Trilha_Discreta_Sem_Voz.wav` é a mixagem separada usada na versão com trilha. A licença CC0 desses WAVs está em `audio/fontes/LICENSE.txt`; não se aplica às imagens ou aos assets de terceiros do jogo.

## Verificação

`Validacao_Exportacao.json` registra resolução, duração, 3.000 quadros de vídeo, codec, presença/ausência da faixa de áudio, hashes e decodificação dos dois MP4s. Foram revisados quadros das capturas e da montagem final para conferir enquadramento, títulos, painéis e créditos. A prévia em mosaico está em `previas/Montagem_Final.png`.

Referências técnicas usadas para a captura: [Camera.Render](https://docs.unity3d.com/cn/6000.0/ScriptReference/Camera.Render.html) e [ReadPixels](https://docs.unity.cn/6000.0/Documentation/ScriptReference/Texture2D.ReadPixels.html).
