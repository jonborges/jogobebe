using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JogoBebe
{
    /// <summary>Roteiro da cena da rua: intro, conversa com a Rebeca, chegada do pai e a proposta de babá.</summary>
    public class StoryDirector : MonoBehaviour
    {
        public Transform player;
        public GameObject playerModel;
        public TalkNPC rebeca;

        const string Eu = "Protagonista"; // troque pelo nome dela quando decidir

        float t, fatherTimer = -1f;
        bool rebecaDone, fatherSpawned, ending;
        FatherCar father;

        void Start()
        {
            rebeca.onInteract = TalkRebeca;
            GameUI.Objective = "Converse com a Rebeca (na calçada, uns 40 m à frente).";
            StartCoroutine(Intro());
        }

        IEnumerator Intro()
        {
            PlayerController.Frozen = true;
            GameUI.I.fade = 1f;
            GameUI.TitleCard = "23:47\nA rua está morta.";
            yield return new WaitForSeconds(2.5f);
            GameUI.TitleCard = "";
            yield return GameUI.I.FadeTo(0f, 1.5f);
            PlayerController.Frozen = false;
        }

        void Update()
        {
            t += Time.deltaTime;
            if (fatherSpawned || ending) return;

            if (fatherTimer >= 0f)
            {
                fatherTimer -= Time.deltaTime;
                if (fatherTimer <= 0f) SpawnFather();
            }
            else if (t > 150f) SpawnFather(); // se o jogador enrolar demais, o pai aparece mesmo assim
        }

        void TalkRebeca()
        {
            if (!rebecaDone)
            {
                GameUI.I.Play(new[]
                {
                    new GameUI.Line("Rebeca", "Amiga! Finalmente alguém. Eu já ia começar a conversar com o poste."),
                    new GameUI.Line(Eu, "E o poste respondeu?"),
                    new GameUI.Line("Rebeca", "Mais que os carros. Olha só: passam, passam, e nenhum desacelera. Parece que tão fugindo de mim."),
                    new GameUI.Line(Eu, "Sexta-feira e a rua deserta. Meu aluguel vence segunda."),
                    new GameUI.Line("Rebeca", "(sem tirar os olhos do celular) Tô aqui olhando meu saldo. Tá mais magro que eu na dieta da lua."),
                    new GameUI.Line("Rebeca", "Fica de olho, vai aparecer alguma coisa. Ou alguém. Ou, no mínimo, um gato."),
                }, () =>
                {
                    rebecaDone = true;
                    fatherTimer = 7f;
                    GameUI.Objective = "Fique na calçada e espere. Alguma coisa vai aparecer.";
                });
            }
            else if (fatherSpawned)
            {
                GameUI.I.Play(new[] { new GameUI.Line("Rebeca", "Aquele carro parou pra você, mulher! Vai lá antes que ele desista!") });
            }
            else
            {
                GameUI.I.Play(new[] { new GameUI.Line("Rebeca", "Tô por aqui. Qualquer coisa, assobia.") });
            }
        }

        void SpawnFather()
        {
            fatherSpawned = true;
            TrafficSpawner.ReserveLane = true;

            var driver = CharacterFactory.Build("Pai", CharacterFactory.Pai());
            var car = Vehicles.Car(new Color(.55f, .5f, .42f), driver, true, "CarroPai");
            car.name = "CarroPai";
            // nasce longe, escondido pela neblina, e chega por tras na faixa do lado da calcada
            float startZ = Mathf.Max(player.position.z - 55f, StreetRoute.StartZ + 8f);
            car.transform.position = new Vector3(StreetRoute.NearLaneX, .05f, startZ);

            father = car.AddComponent<FatherCar>();
            father.player = player;
            father.curbX = StreetRoute.CurbStopX;
            father.onStopped = OnFatherStopped;
            GameUI.Objective = "Um carro está desacelerando na sua direção...";
        }

        void OnFatherStopped()
        {
            var go = new GameObject("JanelaMotorista");
            go.transform.SetParent(father.transform, false);
            // janela do lado da calcada (lado direito do carro, que olha para +Z)
            go.transform.localPosition = new Vector3(.55f, 1.2f, 0f);
            var tn = go.AddComponent<TalkNPC>();
            tn.player = player;
            tn.label = "Falar com o motorista";
            tn.range = 4.5f;
            tn.facePlayer = false;
            tn.onInteract = TalkFather;
            GameUI.Objective = "O motorista quer falar com você. Chegue perto do carro e aperte E.";
        }

        void TalkFather()
        {
            GameUI.I.Play(new[]
            {
                new GameUI.Line("Pai", "Ei, moça! Tem um minuto?"),
                new GameUI.Line(Eu, "Depende. Você paga por minuto ou por hora?"),
                new GameUI.Line("Pai", "Não, não é isso! Eu preciso de uma babá. Só por hoje. Me chamaram no trabalho de emergência."),
                new GameUI.Line(Eu, "Babá? Eu? Querido, eu vou traumatizar a criança."),
                new GameUI.Line("Pai", "Você foi a única que me olhou nos olhos esta noite. Pago o triplo do que você faz numa noite boa."),
                new GameUI.Line(Eu, "..."),
            }, null,
            new[] { "Tá bom, eu topo.", "Triplo? Fechado. Mas adiantado." },
            OnFatherChoice);
        }

        void OnFatherChoice(int i)
        {
            var first = i == 0
                ? new GameUI.Line("Pai", "Obrigado, obrigado! Entra no carro.")
                : new GameUI.Line("Pai", "(suspira) Combinado. Adiantado. E não deixa ele sozinho nem um segundo.");

            GameUI.I.Play(new[]
            {
                first,
                new GameUI.Line("Pai", "Ah, uma última coisa: se ele chorar, não grite. Nunca grite com ele."),
                new GameUI.Line(Eu, "Como assim?"),
                new GameUI.Line("Pai", "Nada! É só um bebê! Vamos?"),
            }, () => StartCoroutine(Finish()));
        }

        IEnumerator Finish()
        {
            ending = true;
            PlayerController.Frozen = true;
            GameUI.Objective = "";
            yield return GameUI.I.FadeTo(1f, 1.5f);
            playerModel.SetActive(false);
            GameUI.TitleCard = "CAPÍTULO 1\nA Casa do Pai";
            yield return new WaitForSeconds(3f);

            if (Application.CanStreamedLevelBeLoaded("Casa"))
                SceneManager.LoadScene("Casa");
            else
                GameUI.TitleCard = "FIM DA CENA DA RUA\n(crie a cena \"Casa\" e adicione no Build Settings)";
        }
    }
}
