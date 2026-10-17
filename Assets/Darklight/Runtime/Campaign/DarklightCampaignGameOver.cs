using UnityEngine;

namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        bool deathMenuPending;
        public bool PresentationThreat => encounterStarted && !chapterCleared;
        float deathMenuReadyAt;

        void BeginGameOver()
        {
            if (deathMenuPending) return;
            StopPower();
            // Never replace the last valid save with the defeat state.
            deathMenuPending = true;
            deathMenuReadyAt = Time.unscaledTime + DarklightDeathReversion.Duration;
            lab.SetPaused(true);
            AudioDirector?.StopCombat();
            music.Pause();
            ambient.Pause();
        }

        bool TickGameOver()
        {
            if (!deathMenuPending) return false;
            if (Time.unscaledTime >= deathMenuReadyAt)
            {
                deathMenuPending = false;
                Open("Death");
            }
            return true;
        }

        public void ResumeLastReturnPoint()
        {
            if(screenTransitionBusy)return;
            StartCoroutine(ReturnThroughSeal());
        }

        void BuildGameOver()
        {
            heading.text = "GAME OVER";
            bodyText.text = "O VÉU TE ALCANÇOU\n\nÚLTIMO PONTO SALVO\n" + DarklightStory.Places[Progress.chapter] +
                " · SELO " + Mathf.Clamp(Progress.checkpoint + 1, 1, 4);
            Button("RETOMAR ÚLTIMO PONTO", ResumeLastReturnPoint);
            Button("CARREGAR SALVAMENTO", () => Open("LoadGames"));
            Button("MENU PRINCIPAL", () => Open("Main"));
        }

        void LayoutGameOver()
        {
            SetHUDVisible(false);
            EnsureGameOverArtwork();gameOverArtwork.gameObject.SetActive(true);gameOverOpenedAt=Time.unscaledTime;
            menuBackdrop.color = Color.clear;
            menuRoot.Find("Moldura").gameObject.SetActive(false);
            menuRoot.anchoredPosition=Vector2.zero;menuRoot.sizeDelta=new Vector2(640,360);
            heading.fontSize = 26;
            heading.color = new Color(.9f, .21f, .27f);
            heading.alignment = TextAnchor.MiddleLeft;
            heading.rectTransform.anchoredPosition = new Vector2(336, -72);
            heading.rectTransform.sizeDelta = new Vector2(274, 42);
            bodyText.alignment = TextAnchor.UpperLeft;
            bodyText.fontSize = 7;
            bodyText.rectTransform.anchoredPosition = new Vector2(340, -128);
            bodyText.rectTransform.sizeDelta = new Vector2(258, 51);
            bodyText.lineSpacing = 1.25f;
            menuFooter.text = "SETAS / LS: ESCOLHER   ENTER / A: CONFIRMAR";
            menuFooter.fontSize=4;menuFooter.rectTransform.anchoredPosition=new Vector2(336,-328);
            menuFooter.rectTransform.sizeDelta=new Vector2(284,12);
            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];
                button.rect.anchoredPosition = new Vector2(336, -(192 + i * 37));
                button.rect.sizeDelta = new Vector2(264, 30);
                button.text.rectTransform.anchoredPosition = new Vector2(8, 0);
                button.text.rectTransform.sizeDelta = new Vector2(248, 30);
                button.text.fontSize = 7;
                button.text.alignment = TextAnchor.MiddleCenter;
            }
        }

        void StyleGameOverButton(int index)
        {
            var b = buttons[index];
            b.rect.gameObject.SetActive(true);
            b.rect.anchoredPosition = new Vector2(336, -(192 + index * 37));
            b.image.material=null;
            b.image.sprite = UIArt(index == selection ? 2 : 1, extras[7]);
            b.image.color = index == selection ? new Color(1, .64f, .69f) : new Color(.65f,.61f,.67f);
            b.text.color = new Color(.96f, .9f, .86f);
        }
    }
}
