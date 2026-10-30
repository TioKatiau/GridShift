# Fluxo de pedestres e veículos — 25/10/2026

Complementa a primeira correção do cruzamento. O teste anterior não cobria a disputa entre vários pedestres caminhando pela mesma calçada nem veículos de 14 m entre sinais próximos.

## Comportamento alterado

- Pedestres escolhem uma direção de desvio antes de encostarem uns nos outros, com preferência pelo lado direito. Cada passo continua sujeito às verificações de apoio na calçada e obstáculos sólidos.
- Um ponto intermediário ocupado pode ser contornado quando o caminho até o ponto seguinte está inteiramente livre e apoiado. Uma fila cuja saída esteja bloqueada aguarda afastada da faixa e permite o avanço de outra travessia livre.
- O ponto de saída de uma travessia fica protegido enquanto outro pedestre está atravessando. Quem está na calçada pode se afastar desse ponto, mas não avançar sobre a saída reservada.
- Novas travessias começam durante uma janela de quatro segundos na fase de vermelho dos veículos. Cada cruzamento só admite pedestres após o veículo que o reservou terminar de sair. Depois que a janela fecha, os grupos já admitidos terminam de atravessar antes do próximo verde; não entram novos grupos indefinidamente.
- O tempo verde começa completo após a faixa ficar livre. Na SampleScene, passou de 10 para 16 segundos.
- A margem de liberação da reserva não soma novamente a distância usada para seguir outro carro. Isso permite que caminhões longos liberem o cruzamento anterior antes de parar no próximo sinal. A traseira inteira ainda precisa ultrapassar o limite de 12 m, com tolerância adicional de 10 cm; esse limite já fica além da área física da faixa.
- População-base de veículos da SampleScene: **15 → 24**; limite: **30 → 40**. A quantidade efetiva continua respeitando horário, preferência de densidade e disponibilidade de posições seguras para gerar veículos.

As únicas alterações na cena foram esses dois valores de população e o tempo verde. Não foi necessário reconstruir a cidade ou alterar as calçadas.

## Testes

Validação em cópia isolada do projeto, Unity 6000.0.83f1. Evidências em `Docs/Validacao8/FluxoCidade/`.

- Calçada de 3,2 m, 12 pedestres em sentidos opostos: **12/12 chegaram**, sem sair da superfície de apoio; menor distância entre centros: **0,661 m**, superior ao diâmetro de 0,56 m dos colliders.
- Veículo de 14 m entre cruzamentos separados por 40 m: respeita vermelho e retoma no verde. A configuração antiga reproduz o bloqueio.
- Dez testes existentes de trânsito e teste de reserva de grupo: aprovados.
- Teste da fase de sinais: mantém vermelho enquanto há pedestre na faixa e inicia o próximo verde com contador zerado.

Esses testes verificam os cenários descritos; filas temporárias nos sinais e redução da população em horários de menor movimento continuam previstas na simulação.

No editor aberto, pare o Play Mode, aguarde a compilação e inicie novamente para recriar os agentes e carregar os novos valores da cena.
