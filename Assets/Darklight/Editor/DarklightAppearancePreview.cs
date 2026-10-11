using UnityEditor;
using UnityEngine;

namespace Darklight.Editor
{
    public sealed class DarklightAppearancePreview : EditorWindow
    {
        int stage,skin,direction,frame,weapon;GameObject preview;
        [MenuItem("Darklight/Aparencia de Fabio")]
        static void Open()=>GetWindow<DarklightAppearancePreview>("Aparencia de Fabio");
        void OnGUI()
        {
            EditorGUILayout.LabelField("Previa visual sem Play Mode",EditorStyles.boldLabel);
            stage=EditorGUILayout.IntSlider("Estagio",stage,0,4);skin=EditorGUILayout.Popup("Skin",skin,new[]{"Requiem","Rock"});
            weapon=EditorGUILayout.Popup("Arma",weapon,new[]{"Katana","Presas do Veu","Laco da Ruptura"});
            direction=EditorGUILayout.Popup("Direcao",direction,new[]{"S","SW","W","NW","N","NE","E","SE"});frame=EditorGUILayout.IntSlider("Pose da arma",frame,0,7);
            if(GUILayout.Button("Atualizar previa na cena"))Refresh();
            if(GUILayout.Button("Remover previa"))Remove();
            EditorGUILayout.HelpBox("A previa nao altera o save, cronologia, combate, audio ou assets. Altura exige variante desenhada com o mesmo PPU e apoio dos pes.",MessageType.Info);
        }
        void Refresh()
        {
            Remove();var view=FindAnyObjectByType<DarklightCharacterView>();var campaign=FindAnyObjectByType<DarklightCampaign>();if(view==null||campaign==null)return;
            var profile=AssetDatabase.LoadAssetAtPath<CharacterAppearanceProfile>("Assets/Darklight/Data/Appearance.asset");
            preview=new GameObject("PreviaAparenciaNaoSalvar"){hideFlags=HideFlags.DontSave};preview.transform.position=view.transform.position+Vector3.right*2;
            var sprite=new GameObject("Corpo").AddComponent<SpriteRenderer>();sprite.transform.SetParent(preview.transform,false);sprite.sharedMaterial=campaign.pixelMaterial;
            sprite.sprite=skin==0?view.idle[direction]:campaign.rockIdle[direction];
            if(weapon>0){var id=weapon==1?"PresasDoVeu":"LacoDaRuptura";var data=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Darklight/Data/Weapons/"+id+".asset");sprite.sprite=data.Poses(skin)[direction*8+frame];}
            sprite.transform.localScale=Vector3.one*2f/sprite.sprite.bounds.size.y;sprite.sortingOrder=200;
            var system=preview.AddComponent<CharacterAppearanceSystem>();system.profile=profile;system.PreviewStage(stage);system.Present(sprite,direction,skin,0,null);
            Selection.activeGameObject=preview;SceneView.lastActiveSceneView?.FrameSelected();SceneView.RepaintAll();
        }
        void Remove(){if(preview!=null)DestroyImmediate(preview);}
        void OnDisable()=>Remove();
    }
}
