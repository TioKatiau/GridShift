# GridShift

Simulação de logística e vida cotidiana em uma cidade 3D low poly. Projeto Integrador SENAI 2026.

**A cidade não para. Sua empresa também não.**

## Jogar

[Baixar o protótipo Windows 26.0](https://github.com/TioKatiau/GridShift/releases/tag/v26.0). Extraia a pasta completa e execute GridShift.exe. Não execute diretamente dentro do ZIP.

[Jogar pelo itch.io](https://tiokatiau.itch.io/gridshift) · [Devlog publicado](https://tiokatiau.itch.io/gridshift/devlog/1778549/gridshift-26-casa-logstica-e-uma-entrega-completa). O ZIP e os três documentos também estão disponíveis na página.

W/A/S/D: movimentar ou dirigir. Mouse: câmera. E: interagir. G: tablet. F1: ajuda. Esc: pausa. Os controles adicionais podem ser consultados e remapeados no jogo.

**Morte permanente:** a morte remove o slot correspondente. Use uma partida descartável ao explorar essa regra.

## Documentos da entrega

- [GDD atualizado — PDF](Docs/Entrega26/GDD.pdf) · [versão editável](Docs/Entrega26/GDD.md)
- [Registro de testes e bugs — PDF](Docs/Entrega26/Testes_Bugs.pdf) · [versão editável](Docs/Entrega26/Testes_Bugs.md)
- [Devlog — PDF](Docs/Entrega26/Devlog.pdf) · [versão editável](Docs/Entrega26/Devlog.md)
- [Índice da entrega](Docs/Entrega26/ENTREGA.md)
- [Resultado da build Windows](Docs/Entrega26/Resultado_Build.md)
- [Como editar os objetos das cenas](Docs/Validacao26/LEIA-ME.md)

## Conteúdo do protótipo

Tutorial com Carlos; preparação, transporte e entrega de encomendas; gestão financeira, funcionários e frota; van pessoal com manutenção, combustível e iluminação; trânsito e pedestres; interações sociais; apartamento com sono, banho, cozinha, compras e personalização; clima e ciclo de dia e noite; perfis locais e cinco slots por perfil.

## Desenvolvimento

Projeto principal em Unity 6000.0.83f1, C#, URP, Input System, TextMesh Pro e Unity UI. Esta distribuição Windows foi gerada e verificada em uma cópia com Unity 6000.3.26f1. As configurações do projeto principal foram preservadas. A compilação de scripts principal e a validação de execução são descritas separadamente nos registros.

O repositório é um registro do código e das cenas do projeto. **Os arquivos-fonte do novo pack Brick Project Studio / Apartment Kit não estão incluídos na publicação.** Para editar o interior, obtenha legalmente o kit e suas dependências e importe-os mantendo seus GUIDs originais. Não substitua arquivos .meta das cenas. A build Windows disponibilizada contém os recursos para jogar sem Unity.

Recursos de terceiros continuam sujeitos às respectivas licenças. Este repositório não concede uma licença sobre os packs de arte nem autoriza redistribuir os arquivos-fonte desses recursos.

## Equipe

Anna Clara Lara Pereira — Multimídia; Geovanna Lopes Fagundes Dalla — Design Gráfico; Gustavo Henriques Barros — Programação de Jogos Digitais; Théo Henrique Barros dos Santos — Design Gráfico; Pedro do Nascimento Braga — Análise e Desenvolvimento de Sistemas.

## Qualidade

Revisão 26: 72 checks específicos e 165 regressões no Editor. Entrega Windows: 165 checks no executável, perfil temporário, RESULT SUCCESS. Há pendências documentadas de Animator, cobertura exploratória e desempenho; não se afirma ausência de todos os bugs.

