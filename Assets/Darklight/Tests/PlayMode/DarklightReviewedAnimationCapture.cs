using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
namespace Darklight.Tests
{
    internal static class DarklightReviewedAnimationCapture
    {
        internal static void Save(string name)
        {
#if UNITY_EDITOR
            string output=UnityEditor.SessionState.GetString("Darklight.Tests.Output","");
            Assert.That(output,Is.Not.Empty,"Capture must run under protected QA");
            string folder=Path.Combine(Path.GetDirectoryName(output),"captures-reviewed");Directory.CreateDirectory(folder);
            var capture=Object.FindFirstObjectByType<DarklightDeliveryCapture>();Assert.That(capture,Is.Not.Null);
            var lab=DarklightCombatLab.Instance;
            typeof(DarklightPixelHUD).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic)?.Invoke(lab.PixelHUD,null);
            Canvas.ForceUpdateCanvases();
            typeof(DarklightDeliveryCapture).GetMethod("Capture",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(capture,new object[]{Path.Combine(folder,name+".png")});
            var body=lab.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();var block=new MaterialPropertyBlock();body.GetPropertyBlock(block);
            File.WriteAllText(Path.Combine(folder,name+".json"),JsonUtility.ToJson(new Record{name=body.sprite.name,ppu=body.sprite.pixelsPerUnit,form=block.GetFloat("_VampireForm"),erase=block.GetFloat("_EraseHead"),scale=body.transform.localScale.x},true));
#endif
        }
        [System.Serializable] sealed class Record{public string name;public float ppu,form,erase,scale;}
    }
}
