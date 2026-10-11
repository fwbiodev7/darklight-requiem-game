namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        public CharacterAppearanceSystem Appearance=>lab?.Player?.GetComponent<CharacterAppearanceSystem>();
        void RestoreAppearance()=>Appearance?.Restore(Progress);
        void RefreshAppearancePortrait()
        {
            if(portrait==null)return;
            // Other speakers must not inherit Fabio's appearance overlays.
            var actor=lab.Player.GetComponent<DarklightCharacterView>();
            if(portrait.sprite==actor.idle[0])Appearance?.UpdatePortrait(portrait);
            else foreach(var name in new[]{"AparenciaRetratoCabelo","AparenciaRetratoRosto"})
            {var child=portrait.transform.Find(name);if(child!=null)child.gameObject.GetComponent<UnityEngine.UI.Image>().enabled=false;}
        }
    }
}
