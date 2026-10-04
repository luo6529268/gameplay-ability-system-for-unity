using System;using System.Collections.Generic;using System.Reflection;using NTSD.UI;
namespace UnityEngine { public class AddComponentMenu:Attribute {public AddComponentMenu(string s){}} }
namespace UnityEngine.EventSystems { public class PointerEventData {public enum InputButton {Left,Right,Middle} public InputButton button;public int pointerId;} }
namespace UnityEngine.UI {
 using UnityEngine.EventSystems;
 public class Button {
  protected enum SelectionState {Normal,Highlighted,Pressed,Selected,Disabled}
  public bool Active=true,VisualPressed;bool interactable=true,down,inside;public int Clicks;public Action Selecting;
  public bool IsActive()=>Active;public bool IsInteractable()=>interactable;
  protected SelectionState currentSelectionState=>!interactable?SelectionState.Disabled:down?SelectionState.Pressed:inside?SelectionState.Highlighted:SelectionState.Normal;
  public void SetInteractable(bool value){interactable=value;DoStateTransition(currentSelectionState,false);}
  public virtual void OnPointerDown(PointerEventData e){if(e.button!=PointerEventData.InputButton.Left)return;Selecting?.Invoke();down=true;Evaluate();}
  public virtual void OnPointerUp(PointerEventData e){if(e.button!=PointerEventData.InputButton.Left)return;down=false;Evaluate();}
  public void OnPointerEnter(PointerEventData e){inside=true;Evaluate();}
  public virtual void OnPointerExit(PointerEventData e){inside=false;Evaluate();}
  void Evaluate(){if(Active&&interactable)DoStateTransition(currentSelectionState,false);}
  protected virtual void DoStateTransition(SelectionState state,bool instant){VisualPressed=state==SelectionState.Pressed;}
  protected virtual void InstantClearState(){inside=false;down=false;VisualPressed=false;}
  protected virtual void OnDisable(){InstantClearState();} protected virtual void OnDestroy(){}
  public void OnPointerClick(PointerEventData e){if(e.button==PointerEventData.InputButton.Left&&Active&&interactable)Clicks++;}
  public void OnSubmit(){if(!Active||!interactable)return;Clicks++;DoStateTransition(SelectionState.Pressed,false);}
  public void FinishSubmit(){DoStateTransition(currentSelectionState,false);}
 }
}
class Probe:NTSDButton {
 public readonly List<bool> Events=new(); public Probe(){PressedStateChanged+=(s,p)=>Events.Add(p);}
 public void Disable(){Active=false;OnDisable();}public void Destroy(){OnDestroy();}
 public void Focus(bool v)=>typeof(NTSDButton).GetMethod("OnApplicationFocus",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(this,new object[]{v});
 public void Pause(bool v)=>typeof(NTSDButton).GetMethod("OnApplicationPause",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(this,new object[]{v});
 public void Clear()=>InstantClearState();
}
class Program {
 static int count;static UnityEngine.EventSystems.PointerEventData P(int id,UnityEngine.EventSystems.PointerEventData.InputButton b=0)=>new(){pointerId=id,button=b};
 static void Check(bool v,string s){if(!v)throw new Exception(s);Console.WriteLine("PASS "+s);count++;}
 static void Held(Probe b,bool held,string s)=>Check(b.IsPointerPressed==held&&b.VisualPressed==held,s);
 static void Main(){
 foreach(bool reverse in new[]{false,true}){var b=new Probe();b.OnPointerDown(P(1));b.OnPointerDown(P(2));Check(b.Events.Count==1,"first down only "+reverse);b.OnPointerUp(P(reverse?1:2));Held(b,true,"intermediate up "+reverse);b.OnPointerUp(P(reverse?2:1));Held(b,false,"final up "+reverse);Check(b.Events.Count==2,"two edges "+reverse);}
 {var b=new Probe();b.OnPointerDown(P(1));b.OnPointerDown(P(2));b.OnPointerExit(P(1));Held(b,true,"one exits other holds");b.OnPointerUp(P(1));b.OnPointerExit(P(99));Held(b,true,"late up/unknown exit preserves other");b.OnPointerExit(P(2));Held(b,false,"last exit releases base and custom");b.OnPointerEnter(P(2));Held(b,false,"reenter does not press");b.OnPointerUp(P(2));Check(b.Events.Count==2,"exit/up dedupe");}
 {var b=new Probe();b.OnPointerDown(P(1));b.OnPointerDown(P(1));b.OnPointerUp(P(99));Held(b,true,"duplicate down and unknown up");b.OnPointerUp(P(1));b.OnPointerUp(P(1));Check(b.Events.Count==2,"duplicate up no event");}
 foreach(string mode in new[]{"disable","destroy","focus","pause","interactable","clear"}){var b=new Probe();b.OnPointerDown(P(1));b.OnPointerDown(P(2));void Clear(){switch(mode){case "disable":b.Disable();break;case "destroy":b.Destroy();break;case "focus":b.Focus(false);break;case "pause":b.Pause(true);break;case "interactable":b.SetInteractable(false);break;default:b.Clear();break;}}Clear();Clear();Held(b,false,mode+" clears");b.OnPointerUp(P(1));b.OnPointerExit(P(2));Check(b.Events.Count==2,mode+" one release and stale callbacks ignored");if(mode!="destroy"&&mode!="clear"){b.OnPointerDown(P(3));Held(b,false,mode+" rejects input while blocked");}b.Active=true;b.Focus(true);b.Pause(false);b.SetInteractable(true);b.OnPointerDown(P(1));Held(b,true,mode+" fresh gesture resumes");b.OnPointerUp(P(1));Check(b.Events.Count==4,mode+" resumed edges");}
 {var b=new Probe();b.OnPointerDown(P(-1));foreach(var key in new[]{UnityEngine.EventSystems.PointerEventData.InputButton.Right,UnityEngine.EventSystems.PointerEventData.InputButton.Middle}){b.OnPointerDown(P(-1,key));b.OnPointerUp(P(-1,key));b.OnPointerExit(P(-1,key));}Held(b,true,"nonleft does not clear left");b.OnPointerUp(P(-1));b.OnPointerClick(P(-1));b.OnPointerClick(P(-1,UnityEngine.EventSystems.PointerEventData.InputButton.Right));b.OnSubmit();Check(b.Clicks==2&&!b.IsPointerPressed,"inherited click and submit");b.FinishSubmit();Held(b,false,"submit visual finishes");b.OnPointerDown(P(1));b.OnSubmit();b.FinishSubmit();Held(b,true,"submit finish preserves active pointer");}
 {var b=new Probe();b.Selecting=()=>b.SetInteractable(false);b.OnPointerDown(P(1));Held(b,false,"selection callback disabling cannot revive press");Check(b.Events.Count==0,"invalidated down no events");}
 {var b=new Probe();b.OnPointerDown(P(1));b.Focus(false);b.Pause(true);b.Focus(true);b.OnPointerDown(P(2));Held(b,false,"focus regain while paused remains blocked");b.Pause(false);b.OnPointerDown(P(2));Held(b,true,"combined pause focus recovery");}
 Console.WriteLine("TOTAL "+count+" PASS. Actual NTSDButton source; uGUI dependency substitutes, not Unity EventSystem/Play.");
 }
}
