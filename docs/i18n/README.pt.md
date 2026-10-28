[English](../../README.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [Deutsch](README.de.md)

[Español (España)](README.es.md) · [العربية](README.ar.md) · [Bahasa Indonesia](README.id.md) · [Español (Latinoamérica)](README.es-419.md)

[Português (Brasil)](README.pt.md) · [Polski](README.pl.md) · [Русский](README.ru.md) · [简体中文](README.zh-Hans.md)

[繁體中文](README.zh-Hant.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [ไทย](README.th.md)

---

# BT3 Sparking Controls — 1.1 Beta

## 1.1 Beta — Windows / Linux

Windows 1.1 Beta: correção da detecção de luta no modo História do Decomp original, HUD com desenho recortado e menos trabalho desnecessário, melhorias de inicialização e compatibilidade, opção Bordas da tela e configurações preservadas. Linux também disponível na 1.1, com HUD recortado, reutilização da imagem X11, correção de luta no modo História e opção Bordas da tela. O suporte às variantes Windows não foi portado; partidas Linux/X11 reais ainda não foram verificadas. A queda no Mission 100 com texturas e ×8 desde a abertura ainda está em investigação; não foi confirmada como resolvida. Agradecimento especial ao Aero pelos testes detalhados e registros.

[1.1 — Changes / English](../Release-1.1.md)


## Controles e guia de instalação

Um layout de controles e HUD inspirado em Sparking! ZERO para Tenkaichi3Decomp. As animações, dano, custos de Ki, alcance, habilidades e janelas de ação continuam sendo os do BT3. O jogo e seus recursos não estão incluídos.

## Novidades da versão 1.0.6 Beta

- Windows e Linux: os quatro menus ficam à esquerda dentro da área de cada jogador, com tamanho estável e ajuste para 4:3, 16:9, 16:10 e ultrawide. Windows: compatibilidade adaptativa e detecção automática do Decomp original, Char Package e Tag Team; dados dos personagens personalizados e até quatro controles locais. O pacote Linux 1.0.6 inclui o novo layout, mas mantém o resolver e a biblioteca nativa anteriores, testados com v0.1.37/v0.1.38.1; não inclui suporte às variantes Windows. No Tag Team com quatro telas, os avisos nativos de boost permanecem sem sobreposição adicional. O próprio Char Package desativa personagens personalizados online. Testes automatizados não substituem partidas reais; Linux nativo e rollback online não foram validados. Não há garantia para qualquer versão futura nem contra todo desync.

[CHANGELOG](../../CHANGELOG.md)

## Downloads e demonstração

[**Windows x64 ZIP**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1-beta/BT3-SparkingControls-1.1-Beta-Windows.zip) · [**Linux x64 ZIP (1.1)**](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1-beta/BT3-SparkingControls-1.1-Beta-Linux-x64.zip) · [**1.1 Beta**](https://github.com/vini2227/BT3-SparkingControls/releases/tag/v1.1-beta)

[▶ Gameplay demo (recorded for 1.0 Beta) — MP4, 167 MiB](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.0-beta/BT3-SparkingControls-1.0-Beta-Gameplay-Demo.mp4) · [TXT](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1-beta/BT3-SparkingControls-Controls-and-Setup-Guide.txt) · [SHA256](https://github.com/vini2227/BT3-SparkingControls/releases/download/v1.1-beta/SHA256SUMS.txt)

## Instalação

### Windows x64

Extraia o ZIP Windows, execute `BT3-SparkingControls.exe`, clique em **Browse / Procurar**, selecione `Tenkaichi3Decomp.exe`, escolha idioma e aparência e clique em **Save and start / Salvar e iniciar**. Sempre inicie o jogo pelo auxiliar. O EXE Windows inclui os recursos do auxiliar e pode ser enviado sozinho para uso normal; o destinatário precisa ter o jogo. O ZIP inclui também o código e a licença. Não requer modloader, recompilação ou substituição de arquivos do jogo.

`%LOCALAPPDATA%\BT3-SparkingControls`

### Linux x64

Extraia o ZIP Linux x64 e mantenha a pasta inteira, incluindo o EXE gerenciado, `libbt3sparking.so`, perfis e assets. Instale Mono com Windows Forms/System.Web.Extensions, libgdiplus e fontes para seu idioma. Use X11 com composição ou XWayland funcional. No terminal dessa pasta, execute `bash start.sh`, selecione o executável **nativo Linux** e clique em Salvar e iniciar. O EXE gerenciado usa Mono; Wine/Proton não são necessários. Wayland nativo não é suportado. A biblioteca foi compilada para glibc 2.35; outro sistema pode exigir recompilação com `bash build.sh`. Não abra outra cópia do jogo separadamente.

`$XDG_DATA_HOME/BT3-SparkingControls/linux` · `~/.local/share/BT3-SparkingControls/linux`

`linux-runtime` · `linux-online-v0.1.37` → `linux`

```bash
bash start.sh --check /path/to/Tenkaichi3Decomp
```

## Controles

□ = quadrado; △ = triângulo; ○ = círculo; LS = analógico esquerdo. As setas em LS indicam o analógico, não o D-pad. A navegação normal dos menus mantém os controles originais.

| Botão / combinação | Ação |
| --- | --- |
| `□` | Ataques Rush |
| `△` | Disparos de Ki |
| `X` | Movimentação e dash nativos |
| `R2` | Carregar Ki |
| `L1` | Subir |
| `L2` | Descer |
| `R2 + L1` | Z Dash vertical para cima |
| `R2 + L2` | Z Dash vertical para baixo |
| `R1` | Defender fora dos próprios combos compatíveis |
| `R2 + □` | Superataque 1 |
| `R2 + △` | Superataque 2 |
| `R2 + ○` | Ataque supremo; exige as condições nativas |
| `R2 + D-pad ↑ / ↓` | Habilidade 1 |
| `R2 + D-pad ← / →` | Habilidade 2 |
| `○` | Postura de contra-ataque nativa |
| `R1 (combo)` | Teleporte frontal nativo durante combos compatíveis |
| `R1 + LS (combo)` | Teleporte direcional nativo durante esses combos |
| `R1 + □` | Agarrão normal |
| `R1 + LS↓ + □` | Agarrão no chão alternativo; apenas analógico |
| `LS↑ + △` | Agarrão no chão pelo comando original |
| `R3` | Emergency Blaster Wave; exige elegibilidade nativa |
| `L3` | Buscar / fixar alvo |
| `Select / View` | Mudar câmera |
| `D-pad ↑` | Manter: abrir transformações/fusões |
| `D-pad ←` | Manter: abrir troca de personagem |
| `D-pad ↓ / → (no R2)` | Bloqueados na batalha para evitar comandos acidentais |

Os especiais podem ser selecionados com R2 já pressionado. Para continuar carregando um especial, mantenha o botão de ataque correspondente; soltar só R2 não solta esse botão. Quando solicitado, □ + direção indicada envia o boost original. A sobreposição cobre apenas o triângulo e preserva a mão. As habilidades são atalhos deste auxiliar, não uma cópia exata dos controles de ZERO.

○ usa a postura nativa do BT3, com prioridade sobre ataques mantidos em combos compatíveis; não acrescenta todo o sistema Perception/Super Perception de ZERO. R1 usa os teleportes e suas exigências de Ki/janelas nativas; não garante aparecer atrás do inimigo nem ficar parado. R1 + □ tem prioridade sobre essas rotas e gera X–soltar–X: depende de alcance e janela válida. O duplo X original continua disponível. O agarrão no chão não aceita D-pad ↓; LS↑ + ○ não o ativa. R3 usa o custo de vida e condições do BT3, não o Revenge Counter baseado em Skill Count de ZERO. O R3 físico original de transformação/fusão é consumido; os cartões geram os comandos internamente.

## Transformações, fusão e troca

Mantenha D-pad ↑ e pressione o botão □/△/X/○ mostrado no cartão; solte ↑ para fechar. L1/R1 alternam entre transformações e fusões apenas quando ambas existem. Categorias vazias são ignoradas; personagens só com fusão abrem direto nela. Mantenha D-pad ← para trocar e escolha o cartão correspondente. Parceiro, forma, barras, cooldowns e estado da luta continuam exigidos pelo BT3. Os três menus usam posições consistentes, cartões vazios e barras de vida dos reservas. No online, os painéis locais ficam à esquerda mesmo sendo Player 2, sem mudar o lado lógico dos comandos.

## HUD e configurações

Mantenha R2 para ver ataques, comandos e custos. **Centered / Centralizada** equilibra centro e bordas; **Expanded / Expandida** afasta os painéis; **Classic (4:3) / Clássica** usa a área original 4:3. Em 4:3 nativo as posições são preservadas; ultrawide usa áreas seguras. É possível ocultar os ataques e ligar a moldura externa, desativada por padrão em configurações novas. Recursos HD disponíveis têm prioridade; os originais são o fallback, sem pacote de texturas incluído. O launcher tem tema escuro, botões arredondados, esfera de três estrelas, prévia e 16 idiomas com árabe RTL; nomes dos golpes/personagens vêm do jogo. **F6** alterna ataques, **F7** moldura e **F8** posição. Clique no botão do atalho para trocar, desativar ou restaurar; duplicados são rejeitados. Exigem foco no jogo e não repetem ao segurar. A opção de bloqueio impede mudanças na luta. Caminho, idioma, aparência, atalhos e alterações feitas em jogo são salvos entre aberturas e atualizações. Se mover o jogo, selecione o novo caminho.

## Compatibilidade, online e testes

Windows e Linux: os quatro menus ficam à esquerda dentro da área de cada jogador, com tamanho estável e ajuste para 4:3, 16:9, 16:10 e ultrawide. Windows: compatibilidade adaptativa e detecção automática do Decomp original, Char Package e Tag Team; dados dos personagens personalizados e até quatro controles locais. O pacote Linux 1.0.6 inclui o novo layout, mas mantém o resolver e a biblioteca nativa anteriores, testados com v0.1.37/v0.1.38.1; não inclui suporte às variantes Windows. No Tag Team com quatro telas, os avisos nativos de boost permanecem sem sobreposição adicional. O próprio Char Package desativa personagens personalizados online. Testes automatizados não substituem partidas reais; Linux nativo e rollback online não foram validados. Não há garantia para qualquer versão futura nem contra todo desync.

## Código e licença

Os dois ZIPs incluem código e licença AGPL-3.0. Fontes mantêm suas licenças. Não são incluídos jogo, texturas do jogo ou configurações pessoais. Projeto de fã não oficial; marcas pertencem aos respectivos titulares.

- [Windows — BUILD.md](../../desenvolvimento/BUILD.md)
- [Linux — README.md / build.sh](../../linux/README.md)
- [AGPL-3.0](../../LICENSE-AGPL-3.0.txt)
- [English TXT](../Controls-and-Setup-Guide.txt)
