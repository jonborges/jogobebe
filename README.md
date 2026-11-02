# Jogo Bebê — protótipo da cena da RUA (Unity)

Terror com humor. A abertura usa um único trecho reto do mapa: a protagonista começa na calçada, a Rebeca fica a ~40 m à frente, passam carros nas duas direções da pista e o pai do menino chega de carro para oferecer o trabalho de babá.

## Como rodar (2 minutos)

1. Unity 2021 LTS ou mais novo (testado na lógica para Built-in; também funciona em URP).
2. Crie um projeto 3D e **copie a pasta `Assets` deste zip por cima** da do projeto.
3. **Project Settings > Player > Other Settings > Active Input Handling = "Both"** (os scripts usam `Input` clássico; o Unity 6 vem com o Input System novo por padrão).
4. Aguarde o Unity instalar o importador e importar os modelos glTF; os prefabs de MAP, WOMAN e REBECA são criados automaticamente (os modelos podem ficar em `Assets/Resources/Models/<NOME>/` ou em `Assets/Models/<NOME>/`; se faltar algum, os outros continuam funcionando).
5. Menu **Jogo Bebe > Criar cena Rua**. Ele cria `Assets/Scenes/Rua.unity` e põe no Build Settings.
6. Play.

## Controles

WASD / analógico andar · Shift correr · Mouse câmera · **E** falar/avançar diálogo · **1/2** escolher resposta · Esc solta o cursor (clique na tela para prender de novo)

## Fluxo da cena

1. Fade-in: "23:47 — A rua está morta."
2. Vá até a Rebeca (~40 m à frente, encostada na parede, sob o neon rosa) e aperte E. Ela fica parada, não vira para você.
3. Depois da conversa, ~7 s depois (ou 150 s se você nem falar com ela) o carro do pai chega por trás, desacelera e para ao lado da calçada.
4. Chegue perto da janela, E, e responda: aceitar ou pedir adiantado.
5. Fade → "CAPÍTULO 1 — A Casa do Pai". Se existir uma cena chamada `Casa` no Build Settings, ela carrega; senão mostra a tela de fim.

## O que tem em cada script (`Assets/Scripts`)

| Script | Função |
|---|---|
| `GameBootstrap` | Monta rua, prédios com janelas acesas, postes, neon, câmera, protagonista, Rebeca e sistemas |
| `StoryDirector` | Roteiro da cena e diálogos (edite o texto aqui) |
| `PS2Render` | Renderiza em Full HD (1080p) com suavização de bordas |
| `PlayerController` | Terceira pessoa: aceleração, corpo que vira suave, balanço de passos, câmera suavizada com colisão |
| `TalkNPC` | Interação com E |
| `FatherCar` | Chegada e parada do carro do pai |
| `Traffic` | Carros e motos que passam sem parar (motos ~22%), mão direita, mantêm distância entre si |
| `GameUI` | HUD de objetivo, diálogo com escolhas, fade, cartões de título |
| `CharacterFactory`, `Gfx` | Personagens (modelo importado ou placeholder blocky) + cache de materiais |
| `Vehicles` | Carros/motos: usa os modelos de `Resources/Models/Cars` e `Bikes`; sem eles, placeholders de cubos |

## Modelos MAP e WOMAN

O projeto usa Unity glTFast para importar os modelos em `Assets/Models/`. Depois da importação, os prefabs são criados automaticamente. O menu **Jogo Bebe > Importar modelos glTF** também permite recriá-los manualmente. O mapa entra no lugar da rua de cubos; WOMAN e REBECA assumem os papéis da protagonista e da NPC.

Os prefabs gerados ficam em `Assets/Resources/Models/Imported/`. O importador preserva os arquivos `.bin` e as texturas nos caminhos originais. O jogo também acha o glTF direto em `Resources/Models/<NOME>/scene` se o prefab ainda não existir.

### Onde fica a rua no mapa

O MAP (Procedural City 5, ~240 x 250 m) é uma grade de quarteirões. A cena usa a rua norte-sul em X = 0, medida na geometria do modelo (as constantes estão em `StreetRoute`, dentro de `GameBootstrap.cs`):

- pista de X = -7,5 a +7,5; calçadas de 7,5 a 15,5 dos dois lados; paredes dos prédios em |X| = 15,5
- trecho reto entre as faixas de pedestres: Z = -52,5 a -7,5. Protagonista começa em Z = -50, Rebeca fica em Z = -11 (~39 m)
- ao norte a rua termina numa praça (T); ao sul ela segue até a borda do mapa, por onde o tráfego chega
- mão direita: sentido +Z anda em X = +2 (lado da calçada da protagonista), sentido -Z em X = -2; o carro do pai encosta em X = 5,4

A colisão é feita com caixas simples (chão, paredes dos prédios, limites das pontas e postes), sem MeshCollider nas malhas do mapa. Se mudar de trecho da rua, só ajuste `StreetRoute` e a posição dos postes/neon em `BuildCorridor`.

### Carros e motos do tráfego (CC0)

Qualquer `.glb`/`.gltf`/prefab em `Assets/Resources/Models/Cars` entra sozinho no tráfego (escala ~3,9 m, centralizado e apoiado no chão automaticamente; frente = +Z). A primeira moto de `Assets/Resources/Models/Bikes` substitui a moto de cubos. Já vêm 4 caminhões e 1 moto do Kenney "Starter Kit Racing" (CC0, https://github.com/KenneyNL/Starter-Kit-Racing). Para sedans, táxi, van e polícia (também Kenney, CC0), baixe o "Car Kit" em https://kenney.nl/assets/car-kit e copie os `.glb` para `Cars/`. Se algum vier com a frente virada, ajuste `Vehicles.ImportedCarYaw`.

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
