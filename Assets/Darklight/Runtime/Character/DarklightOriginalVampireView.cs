using UnityEngine;
namespace Darklight
{
    public sealed class DarklightOriginalVampireView:MonoBehaviour
    {
        DarklightOriginalVampireCatalog catalog;

        MaterialPropertyBlock bodyBlock;
        bool legacyCleaned;
        DarklightSmallTransformationAura smallAura;
        DarklightTransformationBurst burst;
        readonly DarklightDeathReversion deathReversion=new();
        public bool HasAgeHead=>false;
        public Sprite PresentedHead=>null;
        public Sprite PresentedAura=>smallAura!=null?smallAura.PresentedFrame:null;
        public void Present(SpriteRenderer body,int skin,int age,int direction,int form,float clock,bool dead,bool wholeCharacter=false,bool crouched=false,bool allowDeathTransition=true)
        {
            int visualForm=allowDeathTransition?deathReversion.Observe(form,dead,Time.unscaledTime):dead?0:form;
            bodyBlock??=new MaterialPropertyBlock();
            catalog??=Resources.Load<DarklightOriginalVampireCatalog>("Darklight/OriginalVampireCatalog");
            if(!legacyCleaned){var legacy=body.transform.Find("IdadeOriginal");if(legacy!=null)legacy.gameObject.SetActive(false);legacyCleaned=true;}
            var normal=body.sprite;
            body.GetPropertyBlock(bodyBlock);bodyBlock.SetFloat("_RuinFists",DarklightRuinFistsBank.IsRuin(normal)?1:0);body.SetPropertyBlock(bodyBlock);
            bool authored=DarklightTransformedBank.TryResolve(normal,visualForm,out var transformed,out var transformedMask,out int transformedOwner);
            var frame=catalog?.Find(normal);
            var completeMask=TodayCharacterBank.Mask(body.sprite)??DarklightWholeCharacterBank.Mask(body.sprite);wholeCharacter|=completeMask!=null||frame!=null;
            var mask=completeMask??(wholeCharacter?DarklightUserSpriteBank.Mask(body.sprite):frame?.mask)??Texture2D.blackTexture;
            body.GetPropertyBlock(bodyBlock);bodyBlock.SetFloat("_DarkForm",0);bodyBlock.SetFloat("_Physique",0);bodyBlock.SetFloat("_LayerOcclusion",0);bodyBlock.SetFloat("_VampireForm",wholeCharacter||frame!=null?visualForm:0);bodyBlock.SetFloat("_WholeSprite",wholeCharacter?1:0);bodyBlock.SetFloat("_AlphaClip",DarklightUserSpriteBank.Mask(body.sprite)!=null?.1f:.65f);bodyBlock.SetFloat("_EraseHead",0);bodyBlock.SetTexture("_VampireMask",mask);bodyBlock.SetFloat("_WholeFrameId",TodayCharacterBank.WholeFrameId(body.sprite));body.SetPropertyBlock(bodyBlock);
            if(authored){body.sprite=transformed;bodyBlock.SetFloat("_VampireForm",DarklightRuinFistsBank.UsesMaterialPalette(normal,visualForm)?visualForm:0);bodyBlock.SetFloat("_WholeSprite",1);bodyBlock.SetFloat("_WholeFrameId",transformedOwner);bodyBlock.SetTexture("_VampireMask",transformedMask);body.SetPropertyBlock(bodyBlock);}
            if(smallAura==null){var old=body.transform.Find("AuraVampiraOriginal");if(old!=null)old.gameObject.SetActive(false);var obj=new GameObject("AuraPequenaContinua");obj.transform.SetParent(body.transform,false);smallAura=obj.AddComponent<DarklightSmallTransformationAura>();}
            smallAura.Present(body,visualForm,clock,dead);
            if(burst==null){var obj=new GameObject("TransicaoDoDespertar");obj.transform.SetParent(body.transform,false);burst=obj.AddComponent<DarklightTransformationBurst>();}
            var game=DarklightCampaign.Instance;
            bool deathExit=allowDeathTransition&&dead&&deathReversion.Active;
            burst.Present(body,deathExit?deathReversion.SourceForm:game?.TransformationForm??form,
                deathExit?deathReversion.Progress:game?.TransformationProgress??1,
                deathExit?false:game?.EnteringTransformation??true,
                deathExit||!dead&&game?.Transforming==true);
        }
    }
}

