# Validação executada — GridShift Audio 13

Executada em cópia isolada do projeto, em 29/10/2026 (horário de São Paulo), usando Unity 6000.0.83f1, modo batch com renderização. Não foi usado `-nographics`. O projeto original aberto permaneceu aberto. Os testes não controlaram a interface do computador.

O gerenciador de pacotes selecionou URP 17.0.4 e Unity UI 2.0.0 compatíveis com esse editor na cópia; Input System 1.20.0. Os manifests do projeto original não foram alterados. Isso limita a equivalência com outros editores/pacotes.

## Resultado

`runtime-validation.txt`: **RESULT SUCCESS** na revisão final. `unity-validation.log` contém o log integral desse processo. Não houve erro de compilação dos scripts finais; o projeto mantém avisos de código já existente. As tentativas anteriores detectaram e corrigiram a configuração obsoleta do importador e a seleção de uma fonte recém-criada ainda sem clipe no início da logo.

`asset-validation.txt`: **RESULT SUCCESS**, 63 WAVs. Arquivos PCM 16-bit/48 kHz, conteúdo não silencioso, sem clipping digital, hashes consistentes e diferença máxima de duas unidades PCM entre extremos dos loops. Esses são testes dos arquivos, não uma avaliação auditiva da mixagem final.

## O que foi exercitado

- Bootstrap em menu real, efeito único da logo e loop de menu com fade.
- Importação dos 63 clipes; PCM não nulo em motores de carro e caminhão; referências distintas.
- Sete parâmetros reais do mixer em 0,5 linear (aproximadamente -6,02 dB); música em silêncio (-80 dB) preservando feedback de UI.
- Escrita e leitura das preferências de áudio em PlayerPrefs. As preferências anteriores ao teste foram restauradas ao terminar. Não foi executado um reinício completo manual do aplicativo.
- Clique em botão real, sessão real da cidade e uma única escuta ativa.
- Componentes em todos os veículos ativos; presença de perfis de carro e caminhão. Motor próximo em reprodução 3D, silêncio de reprodução além da distância de corte e limite de seis veículos durante 180 quadros.
- Movimento real via PlayerMovement em piso físico isolado: parado sem passos, caminhada, corrida com cadência maior, salto aceito, ausência de passos no ar e som ao aterrissar. O teste usa o método de simulação existente, sem modificar o controlador.
- Tablet real abrindo e fechando com suas animações.
- Transação financeira válida, aceitação de oferta real, repetição idempotente e compra real de upgrade.
- Métodos reais de conclusão de entrega e confirmação de missão com uma fixture de estado isolada. Verificado que a segunda conclusão não repete o efeito. Não se tratou de conduzir essa entrega pelo trajeto físico inteiro.
- Avanço de ato, primeiro aviso de atraso, ausência de repetição contínua e seleção musical para missão especial ativa.
- Mudança do relógio real para dia e noite; crossfade de ambientes.
- Transição real para Office, listener único após carregamento aditivo, ventilação interna e interação real de entrada/saída do computador.
- Limite do pool de efeitos e gravação pelo DeliveryPersistence em caminho temporário isolado. Não foram usados slots pessoais para os testes.

## Medidas parciais

- Cidade: marcador `GridShift.Audio.Update`, média **0,2541 ms**, 180 amostras.
- Office: mesmo marcador, média **0,1734 ms**, 180 amostras.

Esses valores cobrem somente o gerenciador principal, incluindo suas varreduras periódicas. Não incluem o processamento de áudio/DSP, GridShiftVehicleAudio.Update, passos em LateUpdate, renderização, GPU ou a diferença no FPS global. O teste escreve verificações fora do marcador. Não há comparação idêntica antes/depois, teste longo ou afirmação de ausência de impacto no FPS.

Os callbacks adicionais de erros de slots, confirmação de sobrescrita, lembretes, novas demandas e serviços passaram pela compilação e pela regressão final acima, mas nem todos foram disparados individualmente nessa suíte. Hover, buzina por obstáculo, alerta de ré, murmúrios e zonas verdes também não tiveram validação auditiva ou teste individual de cada situação.

## Assets entregues

63 WAVs (41.470.132 bytes no total), mixer, biblioteca referenciada, metadados de importação e componentes em 18 prefabs de veículos e um prefab de árvore usado como zona ambiental. Relação exata em `prefabs.txt`. As cenas do projeto original não foram gravadas ou substituídas pela atualização. Não foi exportado um novo build executável.

A escuta humana, o balanço no equipamento de apresentação e uma sessão prolongada permanecem pendentes. Motores e ambientes são síntese original, não gravações reais.
