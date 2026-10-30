# Correção do cruzamento da loja Clothing — 25/10/2026

Local das imagens: cruzamento próximo de `(244.1, -4.6, 261.4)` na SampleScene.

## Causas reproduzidas

- Pedestres disputavam os mesmos pontos da esquina e podiam bloquear a saída de outra travessia. A aproximação direta também podia encontrar o poste antes de alcançar a faixa.
- Nas quadras de 40 m, o veículo podia alcançar o segundo sinal vermelho ainda reservando o cruzamento anterior. Quando o sinal abria, essa reserva impedia a entrada no próximo cruzamento.

## Alterações

- A aproximação dos pedestres segue os nós da calçada, com posições de espera separadas e afastadas das saídas das faixas. Uma aproximação bloqueada é recalculada.
- A reserva da travessia só começa quando o pedestre chega ao ponto de partida e encontra passagem livre. A admissão usa um estado explícito para não confundir duas travessias consecutivas.
- Pequenos grupos no mesmo sentido podem atravessar juntos. O cruzamento continua reservado até o último integrante sair; fluxos opostos não entram simultaneamente. Pedestres e veículos aguardando têm alternância de acesso.
- O veículo libera o cruzamento depois que toda a traseira ultrapassa a área da faixa (12 m do centro), somada à margem de segurança existente. A verificação de espaço disponível na saída continua ativa.

Arquivos de execução alterados: `PedestrianAgent.cs`, `PedestrianManager.cs`, `Traffic/TrafficJunction.cs` e `Traffic/TrafficCar.cs`. O teste reproduzível está em `Editor/GridShiftCrossingRegression.cs`.

## Validação executada

Executada em uma cópia do projeto com Unity 6000.0.83f1. Os quatro arquivos de execução coincidem por SHA-256 com os arquivos testados. SampleScene e Office não foram alteradas nesta correção.

- Cruzamento real, 240 segundos simulados, oito pedestres nos dois sentidos das duas faixas: **8/8 atravessaram**, 17 travessias concluídas; veículos das duas direções obtiveram acesso (2 horizontais e 6 verticais distintos). Maior ocupação contínua por pedestres: 19,4 s.
- Dois cruzamentos separados por 40 m: a reserva antiga de 20 m reproduziu o bloqueio no segundo verde; com a correção, o veículo respeitou o vermelho e retomou no verde.
- Grupo de pedestres: sentido oposto recusado e carros bloqueados até a saída do último integrante.
- Dez testes existentes de trânsito: aprovados.
- Desvios de poste e jogador: concluídos sem teleporte, colisão ou saída da calçada no cenário de regressão.
- Regressão na cidade expandida: mapa e controles aprovados, pedestres e vários veículos progrediram; uma entrega ao bairro novo foi concluída e paga uma vez.

Evidências em `Docs/Validacao8/CorrecaoCruzamento/`. Esses resultados cobrem os cenários executados; não representam garantia de ausência de congestionamentos em qualquer combinação de tráfego.

Para usar no editor aberto, pare o Play Mode, aguarde a compilação e inicie novamente. Isso recria os agentes com o estado corrigido.
