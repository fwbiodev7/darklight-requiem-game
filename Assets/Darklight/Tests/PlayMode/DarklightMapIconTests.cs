using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightMapIconTests
    {
        [UnityTest]public IEnumerator OpenMapHasDistinctOutlinedIconsAndReadableLegend()
        {
            DarklightAutomation.Silent=true;AudioListener.volume=0;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;game.PreviewChapter(7);yield return null;lab.SetPaused(true);game.Atlas.RefreshNow();
            var symbols=(Texture2D[])typeof(DarklightAtlas).GetField("artSymbols",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game.Atlas);
            for(int n=0;n<symbols.Length;n++){var t=symbols[n];Assert.That(t.width,Is.GreaterThan(32));Assert.That(t.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(t,Is.SameAs(Resources.Load<Texture2D>("Darklight/Minimap/Icons/"+(DarklightMapSymbol)n)));}
            Assert.That(game.Atlas.TryOpen(),Is.True);yield return new WaitForSecondsRealtime(.2f);
            var root=lab.PixelHUD.transform.Find("HUD640x360/Menus/MapaDaFase");
            Assert.That(root.Find("MolduraDoAtlas").GetComponent<UnityEngine.UI.Image>().sprite,Is.SameAs(game.uiArt[0]));Assert.That(root.Find("PainelDaJornada").GetComponent<UnityEngine.UI.Image>().sprite,Is.SameAs(game.uiArt[0]));
            for(int n=0;n<7;n++)
            {
                var label=root.Find("Legenda"+n).GetComponent<UnityEngine.UI.Text>();var icon=root.Find("LegendaIcone"+n).GetComponent<UnityEngine.UI.RawImage>();
                Assert.That(label.fontSize,Is.EqualTo(5));Assert.That(icon.rectTransform.sizeDelta,Is.EqualTo(new Vector2(14,14)));Assert.That(icon.texture,Is.SameAs(symbols[n]));Assert.That(icon.color,Is.EqualTo(Color.white));
                Assert.That(label.rectTransform.anchoredPosition.x,Is.GreaterThan(icon.rectTransform.anchoredPosition.x+14));
                if(n>0)Assert.That(((RectTransform)root.Find("LegendaIcone"+(n-1))).anchoredPosition.y-icon.rectTransform.anchoredPosition.y,Is.EqualTo(15));
            }
            string folder="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/minimapa-reforma-20261015/authorial/map-screen-r2/captures/";Directory.CreateDirectory(folder);DarklightNativeCapture.Screen(folder+"mapa-aberto-icones.png");
        }
    }
}
