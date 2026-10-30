# UrbanLink Flow publicado — diagnóstico separado da Unity

Inspeção somente de leitura em 25/10/2026. Nenhuma alteração publicada.

Endereço: https://urbanlink-flow.onrender.com/

## Evidência disponível

- A primeira abertura mostrou a página “Render - Application loading”; depois a aplicação carregou. Isso separa uma espera de inicialização da hospedagem de problemas durante a navegação. Não há medição suficiente para atribuir a lentidão ao servidor.
- A página pública contém 197 elementos e carrega `vendor/lenis/lenis.min.js` e `js/motion.js`.
- Dois vídeos estavam reproduzindo: `hero-truck.mp4` (1280 × 674) e `logistics-detail.mp4` (1280 × 720).
- O JavaScript público configura Lenis com `autoRaf: true` e suavização da roda. O processamento de scroll já usa um único `requestAnimationFrame` pendente e um evento passivo; não é correto tratá-lo como um listener sem limitação.
- `updateScroll()` escreve propriedades CSS e consulta `getBoundingClientRect()` dos heróis durante a rolagem. É um candidato a medir com o profiler do navegador, não uma causa isolada comprovada.
- `syncVideo()` pausa vídeos quando o documento fica oculto, mas não verifica se cada vídeo está fora da tela. Na inspeção ambos estavam ativos na mesma página.
- O observador de mutações percorre elementos adicionados à árvore. Seu impacto no painel autenticado depende das atualizações dessa outra aplicação.

Fonte pública inspecionada: https://urbanlink-flow.onrender.com/js/motion.js

## Trabalho preparado para quando o repositório estiver disponível

1. Medir tarefas longas, eventos de entrada, layouts e frames durante scroll, abertura de menus e atualização do painel, em páginas públicas e autenticadas.
2. Usar visibilidade por elemento para interromper vídeos fora da tela e pausar animações quando a página estiver oculta, preservando a intenção visual do site.
3. Agrupar leituras de geometria antes das escritas de estilo e atualizar somente heróis visíveis.
4. Medir se Lenis melhora a experiência dentro do Chromium embarcado; oferecer rolagem nativa quando for mais estável, sem aplicar modificações globais sem teste.
5. Conferir identidade estável das linhas do painel, paginação de listas, cancelamento de requisições e frequência das consultas.
6. Avaliar cache de mídia, compressão, respostas de API e inicialização da instância Render com acesso aos logs e à configuração reais.

Faltam o repositório/pasta de origem e, para investigar backend e hospedagem, acesso aos respectivos logs e configurações. Não foram presumidos endpoints, banco de dados ou integração com a empresa salva na Unity.

## Correções já feitas na aplicação local

O DEMO servido pela Unity mantém a identidade de botões e listas e não reconstrói abas ocultas a cada telemetria. A ponte envia identificação e instruções somente quando mudam. O tablet e o computador preservam sua sessão; páginas não são recarregadas para contornar travamentos. Mídias que estavam reproduzindo são pausadas ao guardar o dispositivo e retomadas ao reabri-lo. Essas mudanças não publicam nem alteram o código do UrbanLink Flow hospedado no Render.
