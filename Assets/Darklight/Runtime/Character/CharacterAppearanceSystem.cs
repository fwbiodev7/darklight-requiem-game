using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Darklight
{
    // Appearance follows the view's selected frame. It never changes CombatModel or physics.
    public sealed class CharacterAppearanceSystem : MonoBehaviour
    {
        public CharacterAppearanceProfile profile;
        public int Stage {get;private set;}
        public int Physique=>Current?.physique??0;
        readonly Dictionary<Sprite,AppearanceAttachment> bindings=new();
        SpriteRenderer hair, beard, shoulders;
        MaterialPropertyBlock block;
        int skin,direction,form;
        AppearanceStage Current=>profile!=null&&profile.stages?.Length>0?profile.stages[Mathf.Clamp(Stage,0,profile.stages.Length-1)]:null;
        public void Restore(DarklightProgress state)
        {
            if(profile==null||state==null)return;
            state.appearanceChapter=Mathf.Max(state.appearanceChapter,state.chapter);
            int chapter=Mathf.Clamp(state.appearanceChapter,0,profile.chapterDays.Length-1);
            state.narrativeDays=Mathf.Max(state.narrativeDays,profile.chapterDays[chapter]);
            Stage=Mathf.Clamp(Mathf.Max(state.appearanceStage,profile.StageForDays(state.narrativeDays)),0,profile.stages.Length-1);
            state.appearanceStage=Stage;
        }
        public void PreviewStage(int stage)=>Stage=profile==null?0:Mathf.Clamp(stage,0,profile.stages.Length-1);
        public Sprite ResolveBody(Sprite original)
        {
            if(Current==null||profile.bodyVariants==null)return original;
            int height=profile.allowHeightVariants?Current.heightVariant:0;
            foreach(var variant in profile.bodyVariants)
                if(variant.source==original&&variant.physique==Current.physique&&variant.heightVariant==height&&variant.replacement!=null)return variant.replacement;
            return original;
        }
        void BuildBindings()
        {
            if(bindings.Count>0||profile?.attachments==null)return;
            foreach(var item in profile.attachments)if(item.source!=null)bindings[item.source]=item;
        }
        AppearanceAttachment Attachment(Sprite source)
        {
            BuildBindings();return source!=null&&bindings.TryGetValue(source,out var value)?value:null;
        }
        void EnsureLayers(SpriteRenderer display)
        {
            if(hair!=null)return;
            SpriteRenderer Create(string name)
            {
                var layer=new GameObject(name).AddComponent<SpriteRenderer>();layer.transform.SetParent(display.transform,false);
                layer.sharedMaterial=display.sharedMaterial;layer.gameObject.layer=display.gameObject.layer;layer.enabled=false;return layer;
            }
            shoulders=Create("Aparencia_Ombros");hair=Create("Aparencia_Cabelo");beard=Create("Aparencia_Cavanhaque");
        }
        Sprite HairSprite()=>Current?.hair>0&&profile.hair?.Length==32?profile.hair[(Current.hair-1)*8+direction]:null;
        Sprite BeardSprite()
        {
            int column=direction==0?0:direction==1?1:direction==2?2:direction==6?3:direction==7?4:-1;
            return Current?.facialHair>0&&column>=0&&profile.facialHair?.Length==20?profile.facialHair[(Current.facialHair-1)*5+column]:null;
        }
        Sprite ShoulderSprite()=>Current?.physique>0&&profile.shoulders?.Length==16?profile.shoulders[skin*8+direction]:null;
        public void Present(SpriteRenderer display,int facing,int selectedSkin,int darkForm,CombatModel model)
        {
            if(display==null||profile==null)return;
            EnsureLayers(display);direction=facing;skin=Mathf.Clamp(selectedSkin,0,1);form=darkForm;
            var attachment=Attachment(display.sprite);
            if(attachment==null||Current==null){hair.enabled=beard.enabled=shoulders.enabled=false;return;}
            var hairFrame=HairSprite();
            float clock=model?.Clock??0;
            bool moving=model!=null&&(model.Motion.sqrMagnitude>.01f||model.State==CombatState.Attack||model.State==CombatState.Dodge);
            if(hairFrame!=null&&profile.windHair?.Length==32&&(int)(clock*(moving?9f:2f))%2==1)hairFrame=profile.windHair[(Current.hair-1)*8+direction];
            SetLayer(hair,hairFrame,attachment.crown,attachment.headWidth,display,2,form);
            SetLayer(beard,BeardSprite(),attachment.upperLip,attachment.headWidth*.31f,display,3,form);
            if(beard.enabled){var tint=beard.color;tint.a*=Current.facialHair==1?.35f:Current.facialHair==2?.45f:.55f;beard.color=tint;}
            // Caps extend the existing silhouette, drawn behind the original arms and equipment.
            SetLayer(shoulders,ShoulderSprite(),attachment.shoulders,attachment.shoulderWidth,display,-1,form);
            if(shoulders.enabled)shoulders.color=display.color;
            if(hair.enabled&&model!=null)
            {
                // Authored wind poses keep the crown fixed; no sprite rotation or UV warping.
                // The crown remains fixed. Frame-specific attachment data follows crouches and attacks.
                hair.transform.localRotation=Quaternion.identity;
            }
        }
        void SetLayer(SpriteRenderer layer,Sprite sprite,Vector2 point,float width,SpriteRenderer source,int order,int darkForm)
        {
            layer.enabled=source.enabled&&sprite!=null;if(!layer.enabled)return;
            layer.sprite=sprite;layer.sortingLayerID=source.sortingLayerID;layer.sortingOrder=source.sortingOrder+order;
            float scale=width/Mathf.Max(.001f,sprite.bounds.size.x);
            layer.transform.localScale=Vector3.one*scale;layer.transform.localPosition=point;
            layer.color=source.color;block??=new MaterialPropertyBlock();block.Clear();block.SetFloat("_DarkForm",darkForm);
            block.SetFloat("_HeadThreshold",sprite.bounds.min.y-.1f);
            if(layer==hair||layer==shoulders)
            {
                var body=source.sprite;var texture=body.texture;var rect=body.textureRect;
                block.SetTexture("_OcclusionTex",texture);block.SetFloat("_LayerOcclusion",layer==hair?1:2);
                block.SetVector("_OcclusionScale",new Vector4(scale*body.pixelsPerUnit/texture.width,scale*body.pixelsPerUnit/texture.height,0,0));
                block.SetVector("_OcclusionOffset",new Vector4((point.x*body.pixelsPerUnit+body.pivot.x+rect.x)/texture.width,(point.y*body.pixelsPerUnit+body.pivot.y+rect.y)/texture.height,0,0));
                block.SetVector("_OcclusionRect",new Vector4(rect.xMin/texture.width,rect.yMin/texture.height,rect.xMax/texture.width,rect.yMax/texture.height));
                block.SetVector("_OcclusionStep",new Vector4(body.pixelsPerUnit/48f/texture.width,body.pixelsPerUnit/48f/texture.height,0,0));
                var attach=Attachment(body);block.SetFloat("_OcclusionChin",(attach.upperLip.y*body.pixelsPerUnit+body.pivot.y+rect.y)/texture.height-.015f);
            }
            layer.SetPropertyBlock(block);
        }
        public void UpdatePortrait(Image portrait)
        {
            if(portrait==null)return;
            var source=portrait.sprite;var attachment=Attachment(source);int savedDirection=direction;direction=0;
            void Layer(string name,Sprite sprite,Vector2 point,float width,bool behind)
            {
                var child=portrait.transform.Find(name);Image image;
                if(child==null){image=new GameObject(name,typeof(RectTransform),typeof(Image)).GetComponent<Image>();image.transform.SetParent(portrait.transform,false);image.raycastTarget=false;}else image=child.GetComponent<Image>();
                image.enabled=portrait.enabled&&portrait.gameObject.activeSelf&&attachment!=null&&sprite!=null;
                if(!image.enabled)return;
                image.sprite=sprite;image.preserveAspect=false;image.color=name.EndsWith("Rosto")?new Color(1,1,1,Current.facialHair==1?.35f:Current.facialHair==2?.45f:.55f):Color.white;
                var rect=portrait.rectTransform.rect;float pixelScale=Mathf.Min(rect.width/source.bounds.size.x,rect.height/source.bounds.size.y);
                var rt=image.rectTransform;rt.anchorMin=rt.anchorMax=Vector2.one*.5f;rt.pivot=sprite.pivot/sprite.rect.size;
                rt.sizeDelta=(Vector2)sprite.bounds.size*(width/sprite.bounds.size.x)*pixelScale;
                rt.anchoredPosition=(point-(Vector2)source.bounds.center)*pixelScale;
                // UI cannot place a child behind its parent's Image. Only hair/facial layers are used here.
            }
            Layer("AparenciaRetratoCabelo",HairSprite(),attachment?.crown??Vector2.zero,attachment?.headWidth??0,false);
            Layer("AparenciaRetratoRosto",BeardSprite(),attachment?.upperLip??Vector2.zero,(attachment?.headWidth??0)*.31f,false);
            direction=savedDirection;
        }
    }
}
