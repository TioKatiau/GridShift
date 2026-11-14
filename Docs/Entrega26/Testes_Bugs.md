# GridShift — Registro de testes e bugs

Versão 26.0 · 14 de novembro de 2026

## 1. Critério de evidência

Este documento separa inspeção de código, compilação, testes no Editor, testes do executável e revisão visual. Uma correção implementada não comprova que todos os cenários possíveis foram testados. Os resultados históricos não são contados como execuções novas.

## 2. Ambientes

Projeto principal: Unity 6000.0.83f1, Windows. Compilação C# de runtime e ferramentas de Editor com retorno zero, conforme Docs/Validacao26/compilacao.txt e compilacao-editor.txt.

Integração: cópias isoladas com Unity 6000.3.26f1 e perfis temporários. As quatro cenas são MainMenu, SampleScene, FactoryInterior e Apartment. Nenhuma partida pessoal foi apagada pelo roteiro de verificação.

## 3. Execuções registradas da revisão 26

T26-01 — Compilação de runtime: APROVADO, retorno zero.

T26-02 — Compilação de Editor: APROVADO, retorno zero.

T26-03 — Integração específica: APROVADO, 72 verificações e RESULT SUCCESS em Docs/Validacao26/runtime/runtime.txt. Inclui casa, clima, interruptores, banho, entrada/saída, persistência da caixa e duas entregas.

T26-04 — Regressão: APROVADO, 165 verificações e RESULT SUCCESS em Docs/Validacao26/regressao/runtime.txt. Inclui slots, tutorial, mira real, pedidos, condução, câmera, clima e ragdoll. Algumas mensagens do roteiro dizem build/executável; nessa rodada ele foi executado no Editor.

T26-05 — Objetos editáveis: APROVADO nos cenários automatizados. O interruptor movido antes da inicialização conservou sua posição. As ações da casa foram reutilizadas. O modelo do HUD renomeado e reposicionado foi aplicado à interface funcional. A van não duplicou o piso de carga.

T26-06 — Auditoria: APROVADO, zero componentes de script ausentes nas quatro cenas. O arquivo missing-scripts.txt fica vazio quando não há ocorrências.

T26-07 — Capturas: casa de dia/noite, chuva externa, banho e encomendas foram produzidas; as capturas da casa e da banheira foram inspecionadas. Não representa inspeção de toda a cidade.

T26-08 — Executável Windows desta entrega: resultado adicional registrado em Docs/Entrega26/Resultado_Build.md após a geração e execução. Não confundir os 237 checks anteriores com a build nova.

## 4. Bugs tratados

GS-01 · Alta · Caixa desaparecia ao sair da Factory. Reprodução: preparar um segundo pedido e sair para a cidade. Correção: separar apresentação da caixa do avatar antes de descarregar a cena e preservá-la no gerenciador existente. Regressão: identidade, instância e visibilidade da caixa verificadas em duas entregas. Estado: corrigido nos cenários testados.

GS-02 · Média · Minimap de interior desenhava rota em coordenadas da cidade. Correção: suprimir trajeto de ruas nos interiores e reconstruir o destino real ao voltar. Estado: aprovado nos testes específicos.

GS-03 · Alta · Interações indisponíveis depois de retornar à cena. Correção: reatribuir comandos e registrar ações existentes, preservando alvos de foco. Estado: aprovado nos fluxos cobertos; outros objetos exigem inspeção manual.

GS-04 · Média · Vidro da banheira girava sobre o banheiro. Correção: posição local interpolada para o deslizamento, mantendo rotação. Reprodução: usar E para entrar na banheira. Estado: o teste detectou deslocamento e ausência de giro durante a abertura.

GS-05 · Média · Gavetas e armários abriam com geometria incorreta; porta levava ao vazio. Correção: bloquear os casos incompatíveis do kit e manter a porta de transição real. Estado: verificação automatizada dos bloqueios aprovada.

GS-06 · Alta · Carro prendia em marcações e apresentava dano em piso livre. Tratamentos incrementais: remover colisões decorativas de marcações, suavizar contato inferior e não incluir faróis no cálculo do volume de colisão. Estado: direção e contato aprovados nos trechos do roteiro; todas as ruas ainda não foram percorridas manualmente.

GS-07 · Média · Pedestre recuperava em cima da van. Tratamento: recuperação física segura e liberação do ragdoll. Estado: roteiro de regressão aprovou a criação de juntas e a liberação da física temporária; situações de tráfego denso permanecem para exploração manual.

GS-08 · Média · Tutorial voltava em partidas já concluídas. Tratamento: conclusão e etapa persistidas no slot, sem reinício por troca de cena. Estado: carregamento de slots com tutorial e estado de interior aprovado no roteiro.

GS-09 · Média · Objetos criados apenas no Play dificultavam edição e duplicavam geometria. Correção: referências de cena para objetos fixos e fontes editáveis para elementos dinâmicos. Estado: testes de posição, vínculos e duplicação aprovados nos cenários específicos.

## 5. Ocorrências abertas e limitações

GS-10 · Baixa · Character 04 tem transições AnyState sem condições ou Exit Time; o Unity informa que serão ignoradas. Não foi alterado indiscriminadamente o Animator compartilhado do pack. Estado: aberto para revisão da configuração e da animação percebida no jogo.

GS-11 · Ferramenta · UnityEditor.Search registrou ArgumentOutOfRangeException na indexação da cópia de validação. O stack pertence ao Editor; não demonstra uma falha do executável. Estado: registrar e reproduzir em ambiente limpo se afetar o trabalho.

GS-12 · Cobertura · Não existe benchmark comparável de FPS, memória ou tempo de carregamento desta revisão. Otimizações estruturais foram feitas, mas ganho numérico não foi comprovado.

GS-13 · Cobertura · Teste exploratório manual de todas as ruas, NPCs, menus, combinações de aparência e compras ainda não está completo. Estado: pendente.

GS-14 · Design · O rótulo Fome apresenta reserva de alimento crescente ao comer. Pode causar interpretação inversa. Estado: documentado para futura decisão de UX.

## 6. Próxima sessão manual sugerida

Criar personagem e salvar; concluir tutorial; preparar dois pedidos; sair e voltar da fábrica com a caixa; dirigir por cruzamentos e faixas; usar luzes e setas; entrar na casa; alterar roupa; comprar comida e esperar entrega; cozinhar; entrar e sair dos dois banhos; dormir; alternar interruptores; recarregar e conferir cada estado.

Registrar para cada problema: identificador, data, versão, cena, passos, resultado esperado, resultado observado, frequência, imagem/log, prioridade e estado. Usar perfil descartável ao testar morte permanente.

## 7. Resultado do executável novo

A build Windows 26 foi gerada com sucesso e zero erros, incluindo as quatro cenas e nenhum componente de script ausente. O executável concluiu 165 verificações com RESULT SUCCESS usando perfil temporário. Evidências: build.txt, Resultado_Build.md e executavel/runtime.txt. Este resultado foi obtido nesta entrega, separadamente dos testes no Editor.

