# GridShift 26 — edição e manutenção

## Como mover os objetos

1. Pare o Play. Abra Apartment, FactoryInterior ou SampleScene normalmente.
2. Na hierarquia, selecione o objeto e altere Transform. Os interruptores estão sob o ambiente da casa; Carlos, o acesso à fábrica e a van estão na cidade.
3. Salve a cena. Inicie Play para conferir a interação.

Não edite durante Play esperando que o Unity mantenha a alteração. Não use o menu de preparação para cada ajuste: as cenas entregues já estão preparadas.

## Banheira

No objeto com GridShiftHomeBath, Auto usa deslizamento na banheira. Sliding Offset controla a direção e distância local; Door Seconds controla a duração. O vidro conserva sua rotação. O chuveiro separado mantém a dobradiça.

## Fontes dinâmicas

O grupo MODELOS EDITÁVEIS contém fontes para pedestres, tráfego, aparência, encomenda, mãos, utensílios e partículas. Edite a fonte na própria cena. O grupo é ocultado ao iniciar e as cópias são usadas pela simulação real. Quantidade, destino, dados das encomendas e estado dos personagens continuam dinâmicos.

## Interfaces

O grupo Editable interface · nome da cena tem modelos capturados das telas reais. Ajuste layout, imagem, fonte e cores. Texto, valores e ações continuam no sistema funcional; não remova componentes de ligação. As fontes são desativadas no Play. Alguns elementos criados conforme o conteúdo continuam sendo gerados e recebem apresentação quando há um modelo correspondente.

## Correções e otimizações

- Banheira desliza em vez de girar.
- Ações preexistentes recebem seus comandos novamente sem duplicar o objeto.
- Modelos de NPC mantêm gatilho de interação habilitado na inicialização.
- Inicialização reaproveita gerenciadores, câmera da van e geometria preparada.
- Faróis decorativos não aumentam os limites usados para criar colisores.
- Cores por instância usam MaterialPropertyBlock; faróis atualizam propriedades quando o estado muda.
- Visibilidade de nomes usa consulta espaçada e câmera em cache.
- Objetos fixos deixam de ser reconstruídos a cada entrada.

## Limpeza

Consulte arquivos-arquivados.txt. Arquivos introdutórios do template e cinco ferramentas antigas sem referências foram movidos para Arquivados, fora de Assets. A operação é reversível. Nenhum save foi removido.

## Validação e limites

Compilação C# de runtime e Editor: Unity 6000.0.83f1, código de saída zero. Testes integrados: cópias isoladas com Unity 6000.3.26f1, perfis temporários. Os resultados finais estão nas subpastas runtime e regressao.

Não foi medido um ganho comparável de FPS, nem produzido/testado um novo executável. Uma revisão e testes não comprovam ausência de todos os bugs. Packs originais permanecem preservados. As cenas foram preparadas em cópia isolada; ao abrir no Editor principal, aguarde a importação e confira visualmente a casa e a cidade antes de distribuir uma build.

### Ocorrências ainda abertas

Os logs da cópia de validação registraram erro do indexador UnityEditor.Search ao iniciar e avisos de transições AnyState sem condição no asset Character 04. O primeiro pertence ao indexador do Editor; os avisos do Animator do pack merecem revisão separada antes de alterar animações compartilhadas. Eles não foram classificados como corrigidos. A aparência final das novas fontes de cena ainda precisa de conferência visual no Editor principal.

A auditoria das quatro cenas carregadas não encontrou componentes de script ausentes. O teste de edição do HUD alterou a posição e o nome do modelo na cena e confirmou aplicação na interface funcional.


### Resultado final

- 72 verificações específicas aprovadas, RESULT SUCCESS: runtime/runtime.txt.
- 165 verificações anteriores aprovadas, RESULT SUCCESS: regressao/runtime.txt.
- As mensagens antigas de regressão usam palavras como build/executável; nesta execução elas rodaram no Play de uma cópia isolada do Editor, não em um novo .exe.
- Compilação runtime e Editor em Unity 6000.0.83f1: código zero.
- Auditoria de componentes ausentes nas quatro cenas: zero (missing-scripts.txt vazio).
- Capturas da casa e do banho inspecionadas nesta revisão. Isso não substitui percorrer manualmente toda a cidade e todas as interfaces.
