# GridShift 14.0 — início da Fase 1

Data: 01/11/2026. Esta entrega é incremental e não representa conclusão das cinco fases.

## Recuperação e preservação

- Branch de trabalho: `codex/gridshift-14-phase1`, criada a partir de `main` em `fdfcefb`.
- Tag anterior às alterações: `gridshift-14-recovery-20261101`.
- A alteração preexistente em LiberationSans SDF - Fallback.asset foi preservada e não integra esta implementação. Uma cópia também foi guardada fora do repositório, em `.codex/backups/gridshift14-20261101`.
- Nenhum save pessoal, cena ou prefab foi substituído. Sem push.

## Auditoria inicial do código e das evidências existentes

Empresa, entregas, frota, finanças, missões, upgrades e persistência já têm gerenciadores reais. Tablet e computador usam GridShiftNativeOperations e compartilham os mesmos gerenciadores; não há motivo para introduzir outra fonte de dados ou navegador.

As cenas habilitadas são MainMenu, SampleScene e Office. A campanha contém quatro atos e a missão secundária Parceiros do bairro. A conclusão exige a entrega vinculada e confirmação de recompensa; os testes V12 exercitam essa distinção e impedem pagamento repetido. Essas evidências anteriores não equivalem a uma nova sessão completa nesta versão.

Os pedestres usam PedestrianGraph e movimento cinemático, não NavMeshAgent. Já existem filas de travessia, desvio local, tentativas de recálculo, pooling e seleção de destinos. Substituir essa navegação agora duplicaria mecanismos e poderia quebrar a coordenação com TrafficJunction.

Problemas encontrados por inspeção: o movimento verificava colisões somente na posição final; obstáculos finos entre as posições não eram varridos. Desvios e aproximações de faixa informavam a velocidade pretendida ao Animator, sem medir deslocamento; a distância percorrida também não incluía consistentemente esses ramos.

O relatório `Docs/Validacao12/city-before-queue-fix.txt` termina em FAILURE: somente cinco de oito agentes atravessaram. Não foi encontrado nessa pasta um relatório posterior de sucesso para a cidade completa. Os relatórios isolados de fila têm sucesso, mas usam cenários diferentes e não provam que essa falha foi resolvida. A Fase 1 permanece aberta até nova regressão real.

Os saves v3 já têm cinco slots por perfil, gravação temporária/substituição, backup, validação e leitura de formatos anteriores. O áudio 13 já possui biblioteca, mixer, canais e testes; sua documentação registra ausência de novo build e de avaliação auditiva final.

Assets reaproveitáveis encontrados: controller Pedestrian, clipes Idle A/B, Walk A/B, Run A/B, Jump, Greet e animações faciais, inclusive Talk. Não foram identificados nesta inspeção clipes corporais específicos de sentar, trabalhar no computador ou manusear documentos. A compatibilidade de cada rig ainda exige verificação visual.

O manifest não contém Chromium/UWB. Há ferramentas de editor e pacotes opcionais; sua ausência de uso em todo o projeto ainda não foi comprovada e a remoção não foi realizada. A cópia de validação revelou diferença entre versões declaradas no manifest e as efetivamente disponíveis em Library/PackageCache. Não modificar o PackageCache nem tratar uma execução com pacotes distintos como equivalência completa do projeto.

## Primeiras correções implementadas

- PedestrianManager.CanStep combina verificação do destino e CapsuleCast ao longo do deslocamento, mantendo paredes, veículos e outros obstáculos físicos.
- Caminhada, desvio lateral e aproximação de travessia usam essa mesma verificação.
- Animação, velocidade observável e distância percorrida usam o deslocamento horizontal efetivo do Rigidbody entre passos físicos.
- Não há teletransporte, remoção de obstáculos ou alteração das regras de semáforo.

## Arquivos

Alterados: Assets/Scripts/PedestrianAgent.cs e Assets/Scripts/PedestrianManager.cs.

Criado: Assets/Scripts/Editor/GridShiftV14Checks.cs, com regressões físicas isoladas.

Cenas modificadas: nenhuma. Prefabs modificados: nenhum. Novas mecânicas: nenhuma; este lote corrige a navegação existente.

## Validação e próximos passos

Os resultados executados deste lote são registrados em Docs/Validacao14. A verificação de código e os testes físicos isolados não substituem uma sessão jogável de novo jogo, Carlos, despacho, entrega, pagamento, save e load.

Execuções concluídas nesta entrega:

- `regression.txt`: SUCCESS, 15 verificações de movimento físico e armazenamento real. Inclui parede fina, destino ocupado, triggers, deslocamento real, velocidade zero ao bloquear, gravação v3, backup, recuperação de JSON inválido, rejeição de versão futura e leitura v1.
- `traffic.txt`: SUCCESS, fila de cinco veículos incluindo caminhão. Quatro cruzaram no mesmo verde de 16 segundos, folga mínima de 2,152 m, sem interpenetração; vermelho, pedestre e saída ocupada continuam bloqueando corretamente.
- `audio-assets.txt`: SUCCESS, 63 arquivos conferidos por SHA-256 e por referência de GUID/ID na biblioteca atual. Não é teste de escuta nem valida os sliders em runtime.
- Scripts atuais compilados no Unity 6000.0.83f1, DX11, sem `-nographics`, em projeto isolado contendo os scripts reais e fixtures físicas. URP 17.0.4, Unity UI 2.0.0 e Input System 1.20.0 foram referenciados a partir dos pacotes locais. O manifest e o PackageCache originais não foram editados.
- As tentativas iniciais com Unity 6000.3 e resolução de pacotes falharam antes dos testes por incompatibilidades de dependências. A execução final usa o editor compatível com os pacotes disponíveis. Não houve build final ou teste da cidade completa neste lote.

Prioridade seguinte: executar a regressão de multidão e travessias na cidade atual, investigar qualquer bloqueio restante com motivo e tempo de espera, e repetir a sessão completa V12 e os testes de áudio em cópia integral com versões de pacotes documentadas. Somente então encerrar Fase 1 e avançar para Urban Flow 2.0.

Permanecem pendentes as rotinas e relações de NPCs, escritório evolutivo, depósito, contratos recorrentes, novos eventos, conquistas, personalização adicional, interface 2.0 e build final das fases seguintes.
