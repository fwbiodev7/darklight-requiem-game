using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Darklight
{
    public sealed partial class DarklightInput
    {
        const string BindingsKey="Darklight.Bindings";
        [Serializable] sealed class BindingFile { public int version; public List<BindingRecord> bindings=new(); }
        [Serializable] sealed class BindingRecord { public string map,action,defaultPath,path,interactions,processors; public int index; }
        [Serializable] sealed class LegacyFile { public List<LegacyRecord> bindings; }
        [Serializable] sealed class LegacyRecord { public string action,id,path,interactions,processors; }
        public readonly struct BindingSlot
        {
            public readonly InputAction action; public readonly int index;
            public BindingSlot(InputAction action,int index){this.action=action;this.index=index;}
            public InputBinding binding=>action.bindings[index];
        }
        public InputAction ConfigurableAction(string name)=>MenuAction(name)?Menu(name):Action(name);
        static bool MenuAction(string name)=>name=="Map"||name=="Pause"||name.StartsWith("Map",StringComparison.Ordinal);
        static bool Pad(string path)=>!string.IsNullOrEmpty(path)&&path.StartsWith("<Gamepad>",StringComparison.OrdinalIgnoreCase);
        static string Optional(string value)=>string.IsNullOrEmpty(value)||value=="null"?null:value;
        public static bool IsPadBinding(InputBinding binding)=>Pad(binding.path)||binding.groups=="Gamepad";
        void AddOptionalDeviceBindings()
        {
            // Empty slots can be assigned without stealing an existing gameplay button.
            Action("MP3").AddBinding("",groups:"Gamepad");
            Action("Journal").AddBinding("",groups:"Gamepad");
        }
        public List<BindingSlot> Slots(string name,bool pad)
        {
            var action=ConfigurableAction(name);var result=new List<BindingSlot>();
            for(int i=0;i<action.bindings.Count;i++)if(!action.bindings[i].isComposite&&IsPadBinding(action.bindings[i])==pad)result.Add(new BindingSlot(action,i));
            return result;
        }
        public string SlotLabel(BindingSlot slot)
        {
            string path=slot.binding.effectivePath;
            if(string.IsNullOrEmpty(path))return "SEM BOTAO";
            string key=path.Substring(path.LastIndexOf('/')+1);
            string label=key switch {"buttonSouth"=>"A / CRUZ","buttonWest"=>"X / QUADRADO","buttonEast"=>"B / CIRCULO","buttonNorth"=>"Y / TRIANGULO","leftShoulder"=>"LB / L1","rightShoulder"=>"RB / R1","leftTrigger"=>"LT / L2","rightTrigger"=>"RT / R2","leftStickPress"=>"LS / L3","rightStickPress"=>"RS / R3","leftStick"=>"ANALOGICO ESQ.","rightStick"=>"ANALOGICO DIR.","dpad"=>"DIRECIONAL","start"=>"MENU","select"=>"SELECT / SHARE","leftButton"=>"MOUSE ESQ.","rightButton"=>"MOUSE DIR.","middleButton"=>"MOUSE CENTRAL",_=>slot.action.GetBindingDisplayString(slot.index,InputBinding.DisplayStringOptions.DontIncludeInteractions)};
            return label.ToUpperInvariant();
        }
        public static bool CanAssign(BindingSlot slot,string path)
        {
            if(string.IsNullOrEmpty(path))return true;
            if(IsPadBinding(slot.binding)!=Pad(path))return false;
            if(path.Equals("<Keyboard>/escape",StringComparison.OrdinalIgnoreCase)||path.Equals("<Gamepad>/start",StringComparison.OrdinalIgnoreCase))return false;
            bool vector=slot.action.expectedControlType=="Vector2"&&!slot.binding.isPartOfComposite;
            bool vectorPath=path.EndsWith("/leftStick",StringComparison.OrdinalIgnoreCase)||path.EndsWith("/rightStick",StringComparison.OrdinalIgnoreCase)||path.EndsWith("/dpad",StringComparison.OrdinalIgnoreCase);
            return vector==vectorPath;
        }
        static bool SameContext(InputAction a,InputAction b)
        {
            bool globalA=a.name=="Map"||a.name=="Pause",globalB=b.name=="Map"||b.name=="Pause";
            if(globalA||globalB)return true;
            bool abyssA=a.name.StartsWith("Abyss",StringComparison.Ordinal),abyssB=b.name.StartsWith("Abyss",StringComparison.Ordinal);
            if(abyssA!=abyssB)
            {
                string original=abyssA?b.name:a.name,context=abyssA?a.name:b.name;
                if(original=="Light"||original=="Heavy"||original=="WeaponSpecial")return false;
                // These commands are consumed exclusively during mounted play.
                if(context=="AbyssDrift"&&(original=="Potion"||original=="Aerial"))return false;
            }
            return a.actionMap.name==b.actionMap.name;
        }
        static bool Overlap(string a,string b)
        {
            if(string.IsNullOrEmpty(a)||string.IsNullOrEmpty(b))return false;
            if(a.Equals(b,StringComparison.OrdinalIgnoreCase))return true;
            bool Direction(string root,string part)=>part.StartsWith(root+"/",StringComparison.OrdinalIgnoreCase)&&(part.EndsWith("/up")||part.EndsWith("/down")||part.EndsWith("/left")||part.EndsWith("/right"));
            return Direction(a,b)||Direction(b,a);
        }
        public List<BindingSlot> Conflicts(BindingSlot slot,string path)
        {
            var result=new List<BindingSlot>();
            foreach(var action in Asset)
            {
                if(action.name=="Aim"||action.name=="KeyboardActivity"||action.name=="PointerActivity"||action.name=="Navigate"||action.name=="Submit"||action.name=="Cancel"||action.name=="DialogueAdvance"||!SameContext(slot.action,action))continue;
                for(int i=0;i<action.bindings.Count;i++)if(!(action==slot.action&&i==slot.index)&&!action.bindings[i].isComposite&&Overlap(action.bindings[i].effectivePath,path))result.Add(new BindingSlot(action,i));
            }
            return result;
        }
        public bool Assign(BindingSlot slot,string path)
        {
            if(!CanAssign(slot,path)||Conflicts(slot,path).Count>0)return false;
            slot.action.ApplyBindingOverride(slot.index,path);SaveBindings();return true;
        }
        public bool CanSwap(BindingSlot slot,string path,List<BindingSlot> conflicts)
        {
            if(conflicts.Count!=1||!CanAssign(slot,path)||string.IsNullOrEmpty(slot.binding.effectivePath)||!CanAssign(conflicts[0],slot.binding.effectivePath))return false;
            var other=conflicts[0];
            foreach(var conflict in Conflicts(other,slot.binding.effectivePath))if(conflict.action!=slot.action||conflict.index!=slot.index)return false;
            return true;
        }
        public bool Swap(BindingSlot slot,string path)
        {
            var conflicts=Conflicts(slot,path);if(!CanSwap(slot,path,conflicts))return false;
            string previous=slot.binding.effectivePath;var other=conflicts[0];
            other.action.ApplyBindingOverride(other.index,previous);slot.action.ApplyBindingOverride(slot.index,path);SaveBindings();return true;
        }
        public void RestoreBindings(bool? pad=null)
        {
            foreach(var action in Asset)for(int i=0;i<action.bindings.Count;i++)if(!pad.HasValue||IsPadBinding(action.bindings[i])==pad.Value)action.RemoveBindingOverride(i);
            SaveBindings();
        }
        public bool RestoreSlot(BindingSlot slot)
        {
            if(Conflicts(slot,slot.binding.path).Count>0)return false;
            slot.action.RemoveBindingOverride(slot.index);SaveBindings();return true;
        }
        public void SaveBindings()
        {
            var file=new BindingFile{version=2};
            foreach(var action in Asset)for(int i=0;i<action.bindings.Count;i++)
            {
                var b=action.bindings[i];if(!b.hasOverrides)continue;
                file.bindings.Add(new BindingRecord{map=action.actionMap.name,action=action.name,index=i,defaultPath=b.path,path=b.overridePath,interactions=b.overrideInteractions,processors=b.overrideProcessors});
            }
            if(file.bindings.Count==0)PlayerPrefs.DeleteKey(BindingsKey);else PlayerPrefs.SetString(BindingsKey,JsonUtility.ToJson(file));PlayerPrefs.Save();
        }
        void LoadBindings()
        {
            string json=PlayerPrefs.GetString(BindingsKey,"");if(string.IsNullOrEmpty(json))return;
            try
            {
                var file=JsonUtility.FromJson<BindingFile>(json);
                if(file.version==2)
                {
                    foreach(var record in file.bindings)
                    {
                        var action=Asset.FindActionMap(record.map)?.FindAction(record.action);if(action==null)continue;
                        int index=record.index;
                        bool previousDriftDefault=action.name=="AbyssDrift"&&record.defaultPath=="<Gamepad>/rightShoulder"&&index==1&&action.bindings.Count>1&&action.bindings[1].path=="<Gamepad>/dpad/up";
                        if(index<0||index>=action.bindings.Count||action.bindings[index].path!=record.defaultPath&&!previousDriftDefault)continue;
                        action.ApplyBindingOverride(index,new InputBinding{overridePath=record.path=="null"?null:record.path,overrideInteractions=Optional(record.interactions),overrideProcessors=Optional(record.processors)});
                    }
                }
                else
                {
                    // Previous versions only exposed the first keyboard or gamepad slot.
                    var legacy=JsonUtility.FromJson<LegacyFile>(json);if(legacy?.bindings==null)return;
                    foreach(var record in legacy.bindings)
                    {
                        var action=Asset.FindAction(record.action);if(action==null)continue;
                        var slots=Slots(action.name,Pad(record.path));if(slots.Count==0)continue;
                        int index=slots[0].index;
                        for(int i=0;i<action.bindings.Count;i++)if(action.bindings[i].id.ToString()==record.id){index=i;break;}
                        action.ApplyBindingOverride(index,new InputBinding{overridePath=record.path=="null"?null:record.path,overrideInteractions=Optional(record.interactions),overrideProcessors=Optional(record.processors)});
                    }
                }
            }
            catch(Exception){Debug.LogWarning("Comandos salvos invalidos. Os padroes continuam disponiveis.");Asset.RemoveAllBindingOverrides();}
        }
    }
}
