# GridShift — Game Design Document

Versão 26.0 · 14 de novembro de 2026 · Protótipo acadêmico para Windows

## 1. Visão do jogo

GridShift é uma simulação de logística e vida cotidiana em uma cidade 3D low poly. O jogador administra uma transportadora, prepara encomendas na Factory, dirige sua van, entrega aos clientes e cuida do personagem em seu apartamento.

Slogan: A cidade não para. Sua empresa também não.

O objetivo inicial é aprender a operação com Carlos e concluir uma entrega real. A progressão acrescenta contratos, funcionários, frota, despesas e reputação. O protótipo é desenvolvido para o Projeto Integrador SENAI 2026.

## 2. Equipe

Anna Clara Lara Pereira — Multimídia.

Geovanna Lopes Fagundes Dalla — Design Gráfico.

Gustavo Henriques Barros — Programação de Jogos Digitais.

Théo Henrique Barros dos Santos — Design Gráfico.

Pedro do Nascimento Braga — Análise e Desenvolvimento de Sistemas.

## 3. Público, plataforma e pilares

Plataforma de entrega: Windows 64 bits, teclado e mouse. Idioma principal: português brasileiro. Partida individual; não há modo multijogador implementado.

Pilares: executar tarefas físicas com consequências na gestão; orientar o iniciante sem substituir suas ações; manter trânsito e pedestres presentes no mundo; integrar trabalho e necessidades pessoais; preservar o progresso por perfil e partida.

O estilo visual combina cidade e personagens low poly, interiores de kits existentes, azul escuro, amarelo e a marca GS. O HUD tridimensional, o tablet e os computadores integram a identidade do projeto.

## 4. Ciclo principal e progressão

Receber orientação ou contrato → aceitar pedido → localizar a encomenda → conferir → escolher embalagem → etiquetar → separar no setor correto → liberar → carregar a van → dirigir e estacionar → retirar a caixa → entregar → consultar pagamento, despesas e progresso → salvar.

Estoque, identidade da caixa, destinatário e preparação são verificados pelo sistema real. Consultar uma explicação não conclui a tarefa. A campanha acompanha entregas, reputação e objetivos administrativos. Delegar operações utiliza funcionários, custos e temporizadores próprios da empresa.

## 5. Tutorial e Carlos

A abertura apresenta movimentação e câmera. Em seguida, o marcador leva ao Carlos, mentor presencial. As orientações abrangem tablet, Factory, recepção, aceite, retirada, preparação, carga, direção, entrega, finanças e save.

O progresso e a conclusão do tutorial são dados da partida. Reabrir ajuda não deve reiniciar uma etapa já concluída nem conceder recompensas novamente. Carlos continua como parceiro da campanha após o aprendizado.

## 6. Cidade, fábrica e cenas

MainMenu contém entrada, perfis e partidas. SampleScene contém cidade, trânsito, pedestres, estabelecimentos, Carlos e veículo pessoal. FactoryInterior contém recepção, terminal, estoques, conferência, embalagem e expedição. Apartment contém a casa do personagem.

Os interiores usam transição de cena. A residência não é uma cópia flutuante sobre a cidade; entrar leva diretamente à casa e sair retorna à cidade. A porta para um espaço sem interior e aberturas problemáticas de móveis ficam bloqueadas.

A Factory preserva uma circulação central livre. Sinalização, bancadas, caixas, vigas, cabos e pequenos objetos complementam os setores de trabalho. Elementos decorativos não devem acrescentar obstáculos invisíveis aos percursos.

## 7. Veículo e trânsito

A van pessoal tem condução física, câmera externa, compartimento de carga, combustível, condição, abastecimento, manutenção e recuperação. A posição inicial fica na vaga atrás da Factory; posições próprias salvas são preservadas.

A caixa mantém identidade e apresentação entre fábrica, cidade, mãos e carga. Enquanto a preparação estiver incompleta, o jogo informa a próxima tarefa e mantém o destino operacional na fábrica. Após liberar a encomenda, o destino passa a ser o cliente. Trajetos de ruas não são desenhados dentro do mapa dos interiores.

O trânsito possui rotas e semáforos. Infrações incluem sinal vermelho, contramão e colisões, com consequências financeiras. Faróis possuem modos desligado, baixo e alto; existem setas e pisca-alerta. Veículos do ambiente acendem os faróis à noite. O comportamento não pretende reproduzir um simulador automotivo certificado.

## 8. Cidadãos e diálogos

Pedestres têm nomes, variação visual e interações sociais. Cumprimentos, provocações, insultos e outras respostas usam falas variadas. A seleção de ações aparece na região inferior direita, com conversa e identificação dos participantes na parte inferior.

As respostas podem considerar higiene e cansaço do jogador. Carlos e o atendente mantêm ações ligadas às suas funções. Diálogos não substituem o tablet nem duplicam opções sem utilidade.

Impactos de veículos podem ativar ragdoll físico. A recuperação precisa evitar reaparecer em cima do carro. O personagem do jogador também pode sofrer atropelamentos. Trata-se de física aproximada do protótipo, sem promessa de equivalência à física de outros jogos comerciais.

## 9. Personagem, perfis e saves

O fluxo de perfil utiliza e-mail e senha local. Esse mecanismo identifica perfis da instalação; não é um serviço de autenticação on-line nem uma verificação real de propriedade do e-mail.

Cada perfil oferece cinco slots. A criação de personagem permite nome, idade, pronomes, origem, biografia, modelo e opções de pele e roupa. O guarda-roupa da residência permite alterar a aparência posteriormente.

Persistem empresa, contratos, dinheiro, frota, veículo pessoal, pedidos, preparação, tutorial, personagem, necessidades e compras. A gravação mantém os mecanismos existentes de validação e recuperação de dados. Os testes automáticos usam perfis temporários para não consumir partidas pessoais.

## 10. Casa, necessidades e morte

A casa oferece cama, banho, lavagem das mãos, cozinha, geladeira, entretenimento e guarda-roupa. O jogador deita para dormir. Rotinas usam mãos estilizadas com braços e dedos, utensílios e efeitos apropriados. Gavetas e armários com animações incompatíveis ficam sem abertura.

Vida, Fome, Energia, Higiene e Bem-estar são apresentados em barras no canto superior esquerdo junto ao símbolo GS. Apesar do rótulo Fome, a barra atual representa reserva alimentar: comer aumenta seu valor; zerar causa perda gradual de vida. Essa convenção precisa ser considerada ao avaliar a interface.

Comida é finita. A aba de compras do tablet permite solicitar ingredientes ou refeições; após um intervalo, o estoque é creditado na geladeira. Não há reposição infinita automática.

Energia diminui com o tempo e é recuperada dormindo; banho e mãos lavadas recuperam higiene; entretenimento contribui para bem-estar. A fome extrema reduz vida. A morte usa a regra de morte permanente da partida: o slot correspondente é removido. Essa é uma regra intencional do jogo e deve ser informada ao jogador.

## 11. Banho, clima e iluminação

Usar E na entrada do box ou banheira abre o vidro e posiciona o personagem dentro. Mirar na torneira permite tomar banho. A saída usa um alvo separado e restaura a posição segura. Um save feito durante o banho registra a saída segura.

Na banheira, o vidro corre lateralmente sem girar. O box separado utiliza dobradiça. Direção, distância e duração estão expostas no componente de banho.

Cidade e casa compartilham relógio e clima. Há dia, noite, céu nublado e chuva. O fundo residencial evita o cinza do vazio; chuva externa e fundo não entram no minimapa. Oito interruptores controlam os cômodos. Sala e circulação começam acesas; as escolhas da partida persistem.

## 12. Controles e interface

W/A/S/D movimentam ou dirigem; mouse controla a câmera; Shift permite correr a pé. E executa a interação apontada. G abre o tablet. F1 abre ajuda e Esc permite acessar a pausa. Consulte o menu de controles para os comandos adicionais e remapeamentos de faróis, setas e câmera.

O foco exige distância e visibilidade; paredes não devem permitir transferência de encomendas ou uso de objetos através delas. Diálogos, tablet e pausa preservam os bloqueios de entrada e restauram o controle ao fechar.

## 13. Implementação e edição

O projeto utiliza Unity 6, C#, URP, Input System, TextMesh Pro e Unity UI. A compilação de scripts foi verificada com Unity 6000.0.83f1; a geração do executável desta entrega utiliza a cópia de validação com Unity 6000.3.26f1. Não foram migrados os pacotes do projeto principal.

Os objetos fixos preparados ficam nas cenas. O grupo MODELOS EDITÁVEIS oferece fontes para NPCs, trânsito, aparência, mãos, efeitos, utensílios e encomendas. As cópias dinâmicas continuam controladas pela simulação. Modelos de interface aplicam apresentação à UI funcional sem substituir callbacks ou valores.

As otimizações da revisão 26 reduzem reconstrução de geometria, consultas repetidas de câmera, atualizações de nomes e alterações de materiais dos faróis. Não há medição comparável de ganho de FPS publicada nesta revisão.

## 14. Qualidade, distribuição e limites

A revisão 26 registrou 72 verificações específicas e 165 de regressão aprovadas no Play de cópias isoladas. A ausência de componentes de script faltantes foi auditada nas quatro cenas. Esses resultados não equivalem automaticamente a testes no executável.

O registro de testes desta entrega identifica separadamente compilação, Editor, execução Windows e inspeção visual. Falhas conhecidas e riscos não cobertos permanecem no registro de bugs. Não se afirma ausência de todos os defeitos.

A distribuição oferece executável Windows compactado, GDD, testes/bugs e devlog. Os packs de terceiros exigem suas próprias licenças; uma cópia pública do código não concede direito de redistribuir seus arquivos-fonte. O roteiro de publicação e seus links ficam no índice da entrega.
