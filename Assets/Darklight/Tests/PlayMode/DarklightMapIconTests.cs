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
            DarklightProgress.IsolateStorageForTests(Path.GetFullPath("work/gameplay-20261019/isolated-map-icons.json"));
            DarklightAutomation.Silent=true;AudioListener.volume=0;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;game.PreviewChapter(7);yield return null;lab.SetPaused(true);game.Atlas.RefreshNow();
            var symbols=(Texture2D[])typeof(DarklightAtlas).GetField("artSymbols",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game.Atlas);
            for(int n=0;n<symbols.Length;n++){var t=symbols[n];Assert.That(t.width,Is.GreaterThan(32));Assert.That(t.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(t,Is.SameAs(Resources.Load<Texture2D>("Darklight/Minimap/Icons/"+(DarklightMapSymbol)n)));}
            Assert.That(game.Atlas.TryOpen(),Is.True);yield return new WaitForSecondsRealtime(.2f);
            var root=lab.PixelHUD.transform.Find("HUD640x360/Menus/MapaDaFase");
            foreach(var panel in new[]{"MolduraDoAtlas","PainelDaJornada","MolduraDoTerritorio"})
            {
                var center=root.Find(panel).GetComponent<UnityEngine.UI.Image>();
                Assert.That(center.sprite,Is.Null,"Decorated panel bitmaps must not be stretched over the map");
                var edge=center.transform.Find("ContornoHUD").GetComponent<UnityEngine.UI.Image>();
                Assert.That(edge.sprite,Is.SameAs(Resources.Load<Sprite>("Darklight/HUD/Frame")));
                Assert.That(edge.sprite.border.x,Is.GreaterThan(0));Assert.That(edge.sprite.border.y,Is.GreaterThan(0));
                Assert.That(edge.type,Is.EqualTo(UnityEngine.UI.Image.Type.Sliced));Assert.That(edge.fillCenter,Is.False);
            }
            var mini=(RectTransform)typeof(DarklightAtlas).GetField("miniRoot",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game.Atlas);
            var binding=mini.Find("Atalho").GetComponent<UnityEngine.UI.Text>();
            Assert.That(mini.GetComponent<UnityEngine.UI.RectMask2D>(),Is.Not.Null);
            binding.text="[SELECT]";Canvas.ForceUpdateCanvases();
            Assert.That(binding.preferredWidth,Is.LessThanOrEqualTo(binding.rectTransform.rect.width),"Controller map binding needs enough horizontal space");
            var plus=(RectTransform)mini.Find("DiminuirZoom");
            Assert.That(binding.rectTransform.anchoredPosition.x+binding.rectTransform.rect.width,Is.LessThan(plus.anchoredPosition.x));
            foreach(var row in new[]{"Objetivo0","Objetivo1","Objetivo2"})Assert.That(root.Find(row).GetComponent<UnityEngine.UI.RectMask2D>(),Is.Not.Null);
            for(int n=0;n<7;n++)
            {
                var label=root.Find("Legenda"+n).GetComponent<UnityEngine.UI.Text>();var icon=root.Find("LegendaIcone"+n).GetComponent<UnityEngine.UI.RawImage>();
                Assert.That(label.fontSize,Is.EqualTo(5));Assert.That(icon.rectTransform.sizeDelta,Is.EqualTo(new Vector2(14,14)));Assert.That(icon.texture,Is.SameAs(symbols[n]));Assert.That(icon.color,Is.EqualTo(Color.white));
                Assert.That(label.rectTransform.anchoredPosition.x,Is.GreaterThan(icon.rectTransform.anchoredPosition.x+14));
                if(n>0)Assert.That(((RectTransform)root.Find("LegendaIcone"+(n-1))).anchoredPosition.y-icon.rectTransform.anchoredPosition.y,Is.EqualTo(15));
            }
            string folder=Path.GetFullPath("outputs/validacao-pendencias-20261019/icons")+Path.DirectorySeparatorChar;Directory.CreateDirectory(folder);DarklightNativeCapture.Screen(folder+"mapa-aberto-icones.png");
            game.Atlas.CloseMap();lab.SetPaused(true);binding.text="[SELECT]";Canvas.ForceUpdateCanvases();
            DarklightNativeCapture.Screen(folder+"minimapa-atalho-select.png");
        }
    }
}
