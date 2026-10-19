# Auditoria das pendências do jogo — 18/10/2026

Esta auditoria separa o que já existe, a evidência histórica e o que ainda precisa de validação. A revisão atual das artes e efeitos permanece uma prévia externa; não equivale a uma nova build integrada.

## Pontes do mapa 5

A correção de profundidade já está implementada: `DarklightBridgeVisibility` protege a visibilidade do herói nas três pontes, por meio dos shaders PixelSprite e RemasterSpriteLit. As capturas históricas `ponte-antes-unity.png` e `ponte-corrigida-unity.png`, em `outputs/validacao-pendencias-20261017`, mostram a remoção das faixas do cenário que encobriam o personagem.

O teste de profundidade passou em `delivery-playmode.xml` e novamente em `technical-final-20261018.xml`, iniciado em 18/10 às 12h33, horário de Brasília. A execução inicial de `bridge-depth.xml` havia falhado antes dessas correções; não representa o último resultado. Nenhum desses testes foi repetido nesta auditoria.

A cobertura confirma a ativação nos três corredores e compara o resultado renderizado na primeira ponte. Existe também um teste de travessia física bidirecional, mas sua execução não foi localizada nesses registros. Permanecem abertas a conferência interativa completa nas três pontes e a validação de combate com todas as armas/formas após a integração das artes novas.

## Áudio

O catálogo ativo contém 132 efeitos: 84 WAV e 48 OGG. O diretório Music mantém os cinco MP3 fornecidos. Nesta auditoria silenciosa, os 49 registros antigos de procedência e os 82 arquivos da expansão procedural corresponderam aos arquivos e hashes registrados. O efeito preparado do Crawler e seus dois arquivos de origem preservados também corresponderam aos hashes. Não foram encontrados arquivos `.meta` ausentes nem prefixos sem amostra entre os 36 prefixos literais conferidos no código.

Já existem chamadas de dano do herói, contato das armas, preparação/ataque/dano/morte dos inimigos, passos e pousos. Há nove materiais de passos, 15 identidades de inimigos com duas amostras cada, dez assinaturas de chefes e dez perfis de mistura ambiental. O catálogo oferece seis camadas longas e cinco efeitos ocasionais; os perfis reutilizam recursos e não representam dez gravações ambientais exclusivas.

Como evidência histórica, nove testes EditMode de infraestrutura/importação de áudio passaram em 17/10. Três testes de controles de volume também passaram no conjunto técnico de 18/10. Não houve nova execução desses testes, download ou reprodução de áudio nesta auditoria.

Permanecem pendentes a escuta e a mistura finais, a repetição percebida, os loops, o custo de reprodução real e a sincronização com as animações que ainda serão integradas. O atlas externo `Enemy sounds` continua preservado, sem seleção de trechos para gameplay. As identidades sintéticas variam duas texturas por pitch; sua adequação como alerta, dano e morte exige escuta. O Cavaleiro ainda reutiliza sons metálicos genéricos e precisa de um conjunto dedicado completo para motor, montagem, derrapagem e sobrecarga. A reprodução continua suspensa até pedido explícito do usuário.

## Quarto golpe das adagas — teste desta sessão

O perfil de sessão agora acrescenta um quarto golpe somente quando a fonte de adagas contém três golpes. A execução dupla usa dois contatos, cada um com metade do dano e da postura do terceiro golpe. Katana e corrente mantêm seus três golpes; uma fonte futura de adagas já com quatro golpes não recebe outro golpe adicional nem a configuração especial do fallback.

Quatro testes Unity 6000.6.0f1 EditMode passaram nesta sessão, em projeto mínimo isolado, às 21h44 de 18/10: encadeamento por inputs e reinício no quinto comando; dois contatos únicos e total de dano/postura; imutabilidade dos originais; preservação de um quarto golpe já autorado. Foram usados os quatro scripts reais de runtime e o teste focado, sem cena, Play, áudio ou saves do jogo. Evidências locais deste pacote: `QA-ADAGAS-EDITMODE.xml` e `QA-ADAGAS-EDITMODE.json`.

Esse teste valida o modelo e os perfis, não combate físico ou a apresentação na build. A animação específica do quarto golpe ainda não foi integrada. O perfil registra hitstop de 0,028 s; o resolver existente usa 0,012 s por contato nos ataques com múltiplos pulsos. Essa diferença permanece registrada para a integração.

## Correções de apresentação no runtime desta sessão

Os rastros agora seguem a cor da transformação, incluindo a corrente vermelha e o branco/prateado. A emissão dos cortes usa a janela real de contato e seu progresso, sem continuar desenhando o corte durante a recuperação. A corrente procedural deixa de sobrepor uma segunda arma quando a fonte já contém o personagem completo com sua corrente.

A aura e a explosão de transformação usam os limites da pose neutra original da skin como referência. Trocar para um ataque com arma ou moto muito maior deixa de aumentar a altura e o centro desses efeitos. Isso não altera a escala dos sprites do personagem e não corrige anatomia desenhada. Os dois recursos neutros foram localizados e suas medidas verificadas.

O runtime completo foi compilado com as referências atuais do Unity, sem erros. Essas correções ainda precisam de conferência visual e física dentro do jogo após a integração; não houve execução de gameplay nem nova build nesta sessão.

## Morte, reversão e Game Over

A leitura do fluxo atual confirma: morte interrompe a forma ativa, a reversão visual usa tempo não escalado, o menu permite retornar pelo selo e o carregamento tenta o salvamento principal, seu backup e o estado de sessão. O retorno aplica um ponto seguro, restaura equipamento/vida e mantém a forma normal. Não foi encontrado um erro concreto novo nessa leitura.

O teste existente `DeathMenuPauseDoesNotFreezeTheUnscaledTransition` foi executado isoladamente contra o runtime atual e passou (1 teste, 0 falhas). Ele não abriu o jogo ou arquivos de save e não valida o fluxo completo. Os quatro testes de PlayMode para salvamento principal, antigo, backup e ausência de arquivos não foram executados nesta revisão; continuam pendentes para a validação integrada. Os oito arquivos protegidos de morte/reversão permanecem com os hashes originais.
