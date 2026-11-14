# GridShift — Registro de desenvolvimento

Devlog consolidado · Versão 26.0 · 14 de novembro de 2026

## Sobre este registro

Este histórico foi reconstruído a partir de commits locais, documentos e evidências de validação. Datas de commit são identificadas como tal; as revisões 20 a 26 usam a data de seus registros. Não foram inventadas sessões, horas de trabalho ou testes ausentes.

## 4 de setembro de 2026 — início

Os primeiros commits registram a criação do projeto. O desenvolvimento parte de um jogo Unity com a organização básica de assets, cenas e configurações.

## 8 de setembro de 2026 — mapa e jogador

O commit 547411d registra mapa e personagem. Essa base estabelece a navegação em uma cidade tridimensional, usada depois pelas operações de entrega.

## 22 de outubro de 2026 — trânsito e pedestres

Os commits 51d5b9c e 66d484d registram trânsito, pedestres e correções. O mundo passa a exigir cuidados com rotas, semáforos, travessias e obstáculos, além da movimentação do jogador.

## 29 de outubro de 2026 — gestão, áudio e apresentação

O commit fdfcefb reúne sistemas empresariais, áudio e materiais de pitch. A comunicação do projeto usa capturas reais do jogo. O vídeo sem narração preserva a preferência por apresentar a experiência sem voz artificial.

## 1 de novembro de 2026 — entregas pessoais e van

Os commits 3ee8ff9, 3f91568 e bfa1900 registram proteção de pedestres contra obstáculos, entrega pessoal, descarga de motoristas, histórico e van com depósito interativo.

A operação deixa de ser apenas seleção de opções: a caixa tem identidade, precisa ser preparada e pode ser colocada no veículo. Estoque, pagamento, frota e persistência continuam integrados aos gerenciadores existentes. O registro da revisão 16 contém 57 verificações específicas e regressões próprias; isso é evidência histórica dessa fase.

## 2 a 13 de novembro de 2026 — Factory e fluxo de entrega

As revisões intermediárias acrescentam preparação na Factory, transições, compartimento da van e ajustes de interface. Os problemas apresentados em imagens pelo responsável orientam correções incrementais, preservando o projeto e suas partidas.

## 13 de novembro de 2026 — revisão 20

Carlos assume a orientação do tutorial após a movimentação inicial. A Factory recebe sinalização e detalhes. As interfaces usam a marca GS e eliminam opções redundantes. A van recebe ajustes de posição, contato com piso, luzes e infrações. Cidadãos ganham variação e interações; chuva e céu nublado ampliam o ambiente.

Aprendizado: corrigir a física exige investigar colisores e contato com a estrada, não apenas aumentar a velocidade. Testes em um trecho não comprovam a integridade de toda a cidade.

## 13 de novembro de 2026 — revisões 21 a 23

As conversas sociais passam a ter ações no canto e uma área de diálogo com os nomes dos participantes. O perfil e o personagem recebem informações próprias e personalização.

A residência substitui o Office na experiência do jogador. O kit de apartamento serve de base para cama, banho, cozinha, entretenimento e guarda-roupa. Surgem necessidades de vida, alimento, energia, higiene e conforto, compras com espera e comida finita. A morte permanente torna o cuidado pessoal parte da progressão.

Os ajustes seguintes tratam interações entre cenas, recuperação de pedestres, persistência do tutorial e apresentação das rotinas. As build antigas permanecem identificadas por versão e não devem ser confundidas com a entrega 26.

## 14 de novembro de 2026 — revisão 24

A casa fica separada da cidade, sem corredor para o vazio. A chegada passa a ocorrer dentro da residência. O sono usa posição deitada e as mãos recebem braços e dedos em estilo mais quadrado. O HUD adota o rótulo Fome solicitado pelo responsável.

## 14 de novembro de 2026 — revisão 25

A investigação do segundo pedido identifica a apresentação da caixa ligada ao avatar da cena descarregada. A correção preserva o objeto no sistema existente e remove rotas de rua dos interiores.

Banho ganha entrada, torneira e saída distintas. Móveis incompatíveis são bloqueados. A casa compartilha clima e relógio, recebe fundo externo e iluminação por cômodo com oito interruptores. A documentação registra 65 verificações específicas e 165 de regressão no Editor, sem afirmar uma build 25.

## 14 de novembro de 2026 — revisão 26

O vidro da banheira volta a correr lateralmente. Objetos fixos são preparados nas cenas para facilitar edição. Fontes de pedestres, tráfego, mãos, efeitos, utensílios, aparências e encomendas ficam na hierarquia; as cópias continuam dinâmicas.

Modelos de layout aplicam alterações visuais à interface funcional sem substituir seus comandos. Um teste muda posição e nome do modelo do HUD e confirma a aplicação. Outro move um interruptor e verifica que a inicialização não sobrescreve a edição.

As otimizações reduzem recriação de geometria, consultas de câmera e escritas de materiais. Faróis decorativos deixam de aumentar limites de colisão. Arquivos introdutórios e ferramentas antigas sem uso são arquivados de forma reversível.

Resultado registrado: 72 verificações específicas e 165 de regressão no Editor; compilação runtime e Editor com retorno zero; zero componentes ausentes nas quatro cenas auditadas. Persistem avisos do Animator e limites de cobertura. Não foi publicado um número de ganho de FPS.

## Entrega acadêmica

Esta etapa consolida GDD, testes/bugs e devlog em formatos editáveis e PDF, prepara a distribuição Windows e organiza o repositório e a página do itch.io. Os resultados da build nova e os endereços efetivamente publicados ficam no índice da entrega; intenção de publicar não é tratada como publicação concluída.

## Próximos passos

Fazer exploração manual de todas as ruas e rotinas; revisar transições do Animator; medir desempenho em uma máquina de referência; coletar feedback sobre leitura do HUD e orientação dos pedidos; priorizar problemas reproduzíveis com evidências.

## Verificação da distribuição 26

A geração Windows terminou com sucesso e zero erros. O próprio GridShift.exe executou a regressão com 165 verificações aprovadas e encerrou normalmente. Os documentos foram consolidados em três PDFs e versões editáveis. A publicação externa é confirmada pelos links do índice, sem confundir arquivo preparado com upload concluído.


