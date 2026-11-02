# Jogo Bebê — protótipo da cena da RUA (Unity)

Terror com humor. A abertura usa um único trecho reto do mapa: a protagonista começa na calçada, a Rebeca fica a ~40 m à frente, passam carros nas duas direções da pista e o pai do menino chega de carro para oferecer o trabalho de babá.

## Como rodar (2 minutos)

1. Unity 2021 LTS ou mais novo (testado na lógica para Built-in; também funciona em URP).
2. Crie um projeto 3D e **copie a pasta `Assets` deste zip por cima** da do projeto.
3. **Project Settings > Player > Other Settings > Active Input Handling = "Both"** (os scripts usam `Input` clássico; o Unity 6 vem com o Input System novo por padrão).
4. Aguarde o Unity instalar o importador e importar os modelos glTF; os prefabs de MAP, WOMAN e REBECA são criados automaticamente.
5. Menu **Jogo Bebe > Criar cena Rua**. Ele cria `Assets/Scenes/Rua.unity` e põe no Build Settings.
6. Play.

## Controles

WASD andar · Shift correr · Mouse câmera · **E** falar/avançar diálogo · **1/2** escolher resposta · Esc solta o cursor

## Fluxo da cena

1. Fade-in: "23:47 — A rua está morta."
2. Vá até a Rebeca (40 m à frente, na parede, sob o neon rosa) e aperte E.
3. Depois da conversa, ~7 s depois (ou 150 s se você nem falar com ela) o carro do pai chega por trás, desacelera e para ao lado da calçada.
4. Chegue perto da janela, E, e responda: aceitar ou pedir adiantado.
5. Fade → "CAPÍTULO 1 — A Casa do Pai". Se existir uma cena chamada `Casa` no Build Settings, ela carrega; senão mostra a tela de fim.

## O que tem em cada script (`Assets/Scripts`)

| Script | Função |
|---|---|
| `GameBootstrap` | Monta rua, prédios com janelas acesas, postes, neon, câmera, protagonista, Rebeca e sistemas |
| `StoryDirector` | Roteiro da cena e diálogos (edite o texto aqui) |
| `PS2Render` | Renderiza em Full HD (1080p) com suavização de bordas |
| `PlayerController` | Terceira pessoa com colisão de câmera |
| `TalkNPC` | Interação com E |
| `FatherCar` | Chegada e parada do carro do pai |
| `Traffic` | Carros e motos que passam sem parar (motos ~22%) |
| `GameUI` | HUD de objetivo, diálogo com escolhas, fade, cartões de título |
| `CharacterFactory`, `Vehicles`, `Gfx` | Placeholders blocky + cache de materiais |

## Modelos MAP e WOMAN

O projeto usa Unity glTFast para importar os modelos em `Assets/Models/`. Depois da importação, os prefabs são criados automaticamente. O menu **Jogo Bebe > Importar modelos glTF** também permite recriá-los manualmente. O mapa entra no lugar da rua de cubos; WOMAN e REBECA assumem os papéis da protagonista e da NPC.

Os prefabs gerados ficam em `Assets/Resources/Models/Imported/`. O importador preserva os arquivos `.bin` e as texturas nos caminhos originais.

Para outros personagens ou veículos, coloque prefabs em `Assets/Resources/Models/` com exatamente estes nomes e eles substituem o placeholder automaticamente:

`Protagonista` · `Rebeca` · `Pai` · `CarroPai`

Regras: pés na origem, frente = +Z, ~1,7 m de altura para humanos, carro com ~1,8 × 4,2 m. Os conceitos (cercados, pai e carro, protagonista com machado/inhaler/cigarro, bebê) estão em `Assets/ConceptArt`.

## Antes de gerar o build para a Steam

- **Project Settings > Graphics > Always Included Shaders**: adicione `Standard`, `Unlit/Color` (e os do URP, se usar). Os scripts usam `Shader.Find` e sem isso os materiais ficam rosa no build.
- A resolução da imagem é Full HD; a qualidade visual também depende das malhas, texturas e iluminação dos modelos usados.
- Integração com Steamworks (conquistas, nuvem) fica para depois que o jogo tiver estrutura.

## Próximos passos sugeridos

1. Cena `Casa`: 5 cômodos chiques, o bebê no berço.
2. Sistema de sustos/eventos por cômodo.
3. Os três demônios na ordem: Paimon → Abadom → Anameleque, e o confronto final com o bebê.
