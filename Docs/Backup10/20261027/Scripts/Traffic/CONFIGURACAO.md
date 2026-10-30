# Caminhão configurado — SampleScene

Atualização 2.0 aplicada: consulte também Docs/ATUALIZACAO_2_0.md e Docs/VALIDACAO_2_0.txt para geração de veículos, reservas de cruzamentos, Office persistente e primeira pessoa.

A instância `Vehicle_Truck_separate` já está configurada e salva na SampleScene.

## Configuração aplicada

- `rota`: Rota_14_15_16_Reta, dentro de TrafficRoutes_Truck.
- Posição inicial preservada longitudinalmente; alinhamento lateral corrigido em cerca de 13 cm para o centro da faixa.
- Rigidbody cinemático, sem gravidade, interpolação e Continuous Speculative.
- BoxCollider sólido ajustado ao modelo inteiro: aproximadamente 3,63 m de largura, 3,39 m de altura e 9,10 m de comprimento. A referência `carroceria` está atribuída. O MeshCollider anterior do caminhão foi desativado para evitar formas duplicadas.
- Velocidade máxima 6 m/s; curva 2,5 m/s; aceleração 1,2 m/s²; frenagem 3 m/s²; raio mínimo de giro 7 m.
- Folga de parada 1 m à frente da carroceria; segurança 2 m; alcance de desaceleração 22 m; tolerância de waypoint 0,25 m e tolerância lateral 0,65 m.
- Layer TrafficCar e sensores incluindo os Colliders sólidos existentes. Os sensores desconsideram o contato das rodas com o asfalto e ignoram Triggers.

## Rede de circulação aplicada

O objeto TrafficRoutes_Truck contém 111 trajetórias dirigidas, cobrindo 18 cruzamentos e esquinas reais da malha viária: 34 movimentos retos, 37 conversões à direita e 40 à esquerda. As posições foram obtidas dos objetos Road Intersection, Road T_Intersection e Road Corner da cidade. Não foram adicionadas ruas.

Todas as conexões estão preenchidas explicitamente e terminam na mesma faixa em que a rota seguinte começa. As curvas são amostradas em passos próximos de 40 cm. Os trechos curtos têm limite próprio de 3 m/s, antecipado antes da troca de rota. A escolha aleatória ocorre uma vez por aproximação e fica estável durante a manobra. Os braços de rua sem uma continuação segura para o caminhão ficaram fora das opções.

As rotas que passam por semáforos têm a referência do sinal que enfrenta a direção de aproximação, linha de retenção antes da faixa e marcador de saída após a travessia. A autorização de entrada considera espaço para o veículo inteiro depois do cruzamento. Entradas legais continuam após mudança de luz.

Cinco movimentos inicialmente considerados foram excluídos por obstáculo físico ou falta de continuação após essa exclusão. Todos os caminhos mantidos passaram pela conferência da carroceria inteira contra os Colliders da cópia da cena.

TrafficLight, TrafficLightGroup e TrafficIntersection foram preservados, assim como HUD, câmera, minimapa e economia. Nenhuma configuração de instalação ou pacote do projeto principal foi alterada.

## Testes executados no Unity 6000.0.83f1

- Os 10 testes físicos da lógica passaram: vermelho/espera, verde, amarelo distante/próximo, travessia após vermelho, aceleração, obstáculo/retomada, saída ocupada, conexões variadas e curvas nos dois sentidos.
- Na cópia da SampleScene com o modelo e os Colliders reais, o caminhão parou no vermelho em aproximadamente (367,30; -4,60; 183,69), com a dianteira a 1 m da retenção, e retomou após o verde.
- As 111 trajetórias foram percorridas individualmente sem bloqueio ou erro de acompanhamento.
- Foram executados três percursos contínuos com sementes aleatórias distintas, cada um atravessando 25 cruzamentos. Foram visitadas 57 trajetórias diferentes, sem travamento.

Essas verificações usaram Physics.Simulate e os mesmos scripts numa cópia independente da cena, sem executar HUD/economia/câmera. Os percursos completos usaram sinais verdes controlados pelo teste; o ciclo visual real dos semáforos com todos os sistemas ativos ainda deve ser observado em Play Mode. A atualização 2.0 acrescentou reservas de cruzamentos para múltiplos veículos.

## Uso

Abra a SampleScene atualizada e entre em Play Mode. Se o Unity ainda exibir os campos antigos, recarregue a cena salva antes do Play. Não é necessário preencher referências manualmente neste caminhão.

Os menus GridShift > Tráfego > Validar cena e Preparar veículo selecionado continuam disponíveis para futuras alterações ou outros veículos. Para executar os testes genéricos, use uma cena vazia, sem Colliders de outras cenas carregadas.

