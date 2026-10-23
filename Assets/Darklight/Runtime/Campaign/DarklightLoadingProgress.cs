using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    public sealed class DarklightLoadingProgress
    {
        public float Value{get;private set;}
        public string Stage{get;private set;}="PREPARANDO PASSAGEM";
        public bool Ready{get;private set;}
        public void Reset(){Value=0;Ready=false;Stage="PREPARANDO PASSAGEM";}
        public void Report(float completed,string stage)
        {if(Ready)return;Value=Mathf.Max(Value,Mathf.Clamp(completed,0,.99f));Stage=stage;}
        public void Complete(){Value=1;Ready=true;Stage="CAMINHO ABERTO";}
    }
    public static class DarklightLoadingSequence
    {
        // Flatten nested stages so an exception cannot strand a paused campaign behind the overlay.
        public static IEnumerator Run(IEnumerator work,Action<Exception> failure)
        {
            var stack=new Stack<IEnumerator>();stack.Push(work);
            try
            {
                while(stack.Count>0)
                {
                    object current=null;bool next=false;Exception error=null;
                    try{next=stack.Peek().MoveNext();if(next)current=stack.Peek().Current;}catch(Exception e){error=e;}
                    if(error!=null){failure(error);yield break;}
                    if(!next){(stack.Pop() as IDisposable)?.Dispose();continue;}
                    if(current is IEnumerator nested)stack.Push(nested);else yield return current;
                }
            }
            finally{while(stack.Count>0)(stack.Pop() as IDisposable)?.Dispose();}
        }
    }
}
