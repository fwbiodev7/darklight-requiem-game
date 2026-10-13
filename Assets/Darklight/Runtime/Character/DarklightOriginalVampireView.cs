using UnityEngine;
namespace Darklight
{
    public sealed class DarklightOriginalVampireView:MonoBehaviour
    {
        DarklightOriginalVampireCatalog catalog;
        SpriteRenderer aura;
        MaterialPropertyBlock bodyBlock,auraBlock;
        bool legacyCleaned;
        public bool HasAgeHead=>false;
        public Sprite PresentedHead=>null;
        public Sprite PresentedAura=>aura!=null&&aura.enabled?aura.sprite:null;
        public void Present(SpriteRenderer body,int skin,int age,int direction,int form,float clock,bool dead,bool wholeCharacter=false,bool crouched=false)
        {
            bodyBlock??=new MaterialPropertyBlock();auraBlock??=new MaterialPropertyBlock();
            catalog??=Resources.Load<DarklightOriginalVampireCatalog>("Darklight/OriginalVampireCatalog");
            if(!legacyCleaned){var legacy=body.transform.Find("IdadeOriginal");if(legacy!=null)legacy.gameObject.SetActive(false);legacyCleaned=true;}
            var frame=catalog?.Find(body.sprite);
            var completeMask=DarklightWholeCharacterBank.Mask(body.sprite);wholeCharacter|=completeMask!=null||frame!=null;
            var mask=completeMask??(wholeCharacter?DarklightUserSpriteBank.Mask(body.sprite):frame?.mask)??Texture2D.blackTexture;
            body.GetPropertyBlock(bodyBlock);bodyBlock.SetFloat("_DarkForm",0);bodyBlock.SetFloat("_Physique",0);bodyBlock.SetFloat("_LayerOcclusion",0);bodyBlock.SetFloat("_VampireForm",wholeCharacter||frame!=null?form:0);bodyBlock.SetFloat("_WholeSprite",wholeCharacter?1:0);bodyBlock.SetFloat("_AlphaClip",DarklightUserSpriteBank.Mask(body.sprite)!=null?.1f:.65f);bodyBlock.SetFloat("_EraseHead",0);bodyBlock.SetTexture("_VampireMask",mask);body.SetPropertyBlock(bodyBlock);
            if(aura==null){aura=new GameObject("AuraVampiraOriginal").AddComponent<SpriteRenderer>();aura.transform.SetParent(body.transform,false);aura.sharedMaterial=body.sharedMaterial;}
            var auras=catalog?.aura??DarklightUserSpriteBank.FormAuras;aura.enabled=form>0&&!dead&&auras.Length==12;
            if(aura.enabled)
            {
                int phase=DarklightCampaign.Instance?.ReducedFlashes==true?0:(int)(clock*8)%4;aura.sprite=auras[(Mathf.Clamp(form,1,3)-1)*4+phase];
                aura.transform.localPosition=Vector3.up*(wholeCharacter&&crouched?.76f:1.03f);aura.transform.localScale=Vector3.one*(2.4f/aura.sprite.bounds.size.y);
                aura.sortingLayerID=body.sortingLayerID;aura.sortingOrder=body.sortingOrder-2;
                auraBlock.Clear();auraBlock.SetFloat("_AlphaClip",.18f);aura.SetPropertyBlock(auraBlock);aura.color=Color.white;
            }
        }
    }
}
