
using System;using System.Collections.Generic;using NTSD.UI;
namespace UnityEngine {public class AddComponentMenu:Attribute {public AddComponentMenu(string s){}}}
namespace UnityEngine.EventSystems {public class PointerEventData {public enum InputButton{Left,Right,Middle}public InputButton button;public int pointerId;}}
namespace UnityEngine.UI {
 using UnityEngine.EventSystems;
 public class Button {
  public bool Active=true,Interactable=true,VisualPressed;public int Clicks;
  public bool IsActive()=>Active;public bool IsInteractable()=>Interactable;
  public virtual void OnPointerDown(PointerEventData e){if(e.button==PointerEventData.InputButton.Left)VisualPressed=true;}
  public virtual void OnPointerUp(PointerEventData e){if(e.button==PointerEventData.InputButton.Left)VisualPressed=false;}
  public virtual void OnPointerExit(PointerEventData e){}
  protected virtual void InstantClearState(){VisualPressed=false;}
  protected virtual void OnDisable(){InstantClearState();}
  public void FocusLost(){if(VisualPressed&&Active&&Interactable)InstantClearState();}
  public void OnPointerClick(PointerEventData e){if(e.button==PointerEventData.InputButton.Left&&Active&&Interactable)Clicks++;}
  public void OnSubmit(){if(Active&&Interactable)Clicks++;}
 }
}
class Probe:NTSDButton {public void Disable()=>OnDisable();public void Clear()=>InstantClearState();public List<bool> Hooks=new List<bool>();protected override void OnPressedStateChanged(bool p){Hooks.Add(p);}}
class Program {
 static int count;static void Check(bool v,string name){if(!v)throw new Exception(name);count++;Console.WriteLine("PASS "+name);}
 static void Main(){var b=new Probe();var events=new List<bool>();b.PressedStateChanged+=(s,p)=>events.Add(p);
 var left=new UnityEngine.EventSystems.PointerEventData();var right=new UnityEngine.EventSystems.PointerEventData{button=UnityEngine.EventSystems.PointerEventData.InputButton.Right};var middle=new UnityEngine.EventSystems.PointerEventData{button=UnityEngine.EventSystems.PointerEventData.InputButton.Middle};
 b.OnPointerDown(right);b.OnPointerDown(middle);Check(!b.IsPointerPressed&&events.Count==0&&!b.VisualPressed,"non-left down ignored");
 b.OnPointerDown(left);b.OnPointerUp(right);b.OnPointerUp(middle);Check(b.IsPointerPressed&&b.VisualPressed&&events.Count==1,"non-left up cannot release left hold");
 b.OnPointerDown(left);Check(events.Count==1,"duplicate press suppressed");b.OnPointerUp(left);Check(!b.IsPointerPressed&&!b.VisualPressed&&events.Count==2,"left release");
 b.OnPointerDown(left);b.OnPointerExit(left);b.OnPointerUp(left);Check(!b.IsPointerPressed&&events.Count==4,"exit releases once");
 b.OnPointerDown(left);b.FocusLost();Check(!b.IsPointerPressed&&!b.VisualPressed&&events.Count==6,"base focus clearing also clears custom state");
 b.OnPointerDown(left);b.Disable();b.Clear();b.OnPointerUp(left);Check(!b.IsPointerPressed&&events.Count==8,"disable repeated clear and later up emit one release");
 b.OnPointerClick(left);b.OnPointerClick(right);b.OnSubmit();Check(b.Clicks==2,"inherited click/submit retained");
 Check(events.Count==b.Hooks.Count,"hook/event edge counts agree");var multi=new Probe();var p1=new UnityEngine.EventSystems.PointerEventData{pointerId=1};var p2=new UnityEngine.EventSystems.PointerEventData{pointerId=2};multi.OnPointerDown(p1);multi.OnPointerDown(p2);multi.OnPointerUp(p2);Console.WriteLine("OBSERVATION p1 down, p2 down, p2 up: IsPointerPressed="+multi.IsPointerPressed+" while p1 has not released; no new policy selected.");
 Console.WriteLine("TOTAL "+count+" PASS; actual NTSDButton source, stub uGUI base; not real EventSystem test.");
 }
}
