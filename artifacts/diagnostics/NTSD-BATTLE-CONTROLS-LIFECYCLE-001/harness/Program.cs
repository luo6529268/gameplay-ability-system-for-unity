
using System;
using System.Collections.Generic;
using System.Reflection;
using NTSD.App;
using NTSD.UI;
using NTSD.UI.Battle;
namespace UnityEngine {
 public class MonoBehaviour { public bool isActiveAndEnabled=true; public GameObject gameObject=new GameObject(); }
 public class GameObject { public bool activeSelf=true; public void SetActive(bool v){activeSelf=v;} }
 public class SerializeField:Attribute{} public class HeaderAttribute:Attribute{public HeaderAttribute(string s){}}
 public static class Debug{public static void LogWarning(string s){}}
}
namespace UnityEngine.InputSystem {
 public class InputAction {public string name; public InputAction(string n){name=n;}}
 public class InputActionMap {public InputAction FindAction(string n,bool throwIfNotFound){return new InputAction(n);}}
}
namespace NTSD.App {
 public enum BattleInputAction{Attack,Jump,Defend}
 public class InputModule {
  public bool Available=true;public List<string> Calls=new List<string>();
  public UnityEngine.InputSystem.InputActionMap GetActionMapByPlayerID(int i){return new UnityEngine.InputSystem.InputActionMap();}
  public bool TrySetActionPressed(int i,UnityEngine.InputSystem.InputAction a,bool p){Calls.Add(i+":"+a.name+":"+p);return Available;}
 }
 public class AppManager { public static AppManager Instance=new AppManager();public InputModule InputModule=new InputModule(); }
}
namespace NTSD.UI {
 public class NTSDButton {
  public event Action<NTSDButton,bool> PressedStateChanged;
  public bool isActiveAndEnabled=true;public bool IsInteractable(){return true;}
  public int Listeners=>PressedStateChanged?.GetInvocationList().Length??0;
  public void Send(bool p){PressedStateChanged?.Invoke(this,p);}
 }
}
namespace NTSD.UI.Battle {public class BattleControlsState {public bool IsVisible;}}
class Program {
 static int checks;static void Check(bool v,string n){if(!v)throw new Exception(n);checks++;Console.WriteLine("PASS "+n);}
 static void Call(object o,string m){o.GetType().GetMethod(m,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(o,null);}
 static void Main(){
  var v=new BattleControlsView();var a=new NTSDButton();var j=new NTSDButton();var d=new NTSDButton();
  var flags=BindingFlags.NonPublic|BindingFlags.Instance;
  typeof(BattleControlsView).GetField("attackButton",flags).SetValue(v,a);
  typeof(BattleControlsView).GetField("jumpButton",flags).SetValue(v,j);
  typeof(BattleControlsView).GetField("defendButton",flags).SetValue(v,d);
  Call(v,"Awake");Call(v,"OnEnable");Call(v,"OnEnable");v.BindPlayer(1);
  var sink=AppManager.Instance.InputModule;
  Check(a.Listeners==1&&j.Listeners==1&&d.Listeners==1,"enable idempotent subscription");
  a.Send(true);a.Send(true);a.Send(false);Check(sink.Calls.Count==2,"press repeat suppressed and release routed");
  a.Send(true);v.isActiveAndEnabled=false;Call(v,"OnDisable");int count=sink.Calls.Count;
  a.Send(true);v.SetActionPressed(BattleInputAction.Attack,true);
  Check(a.Listeners==0&&sink.Calls.Count==count&&sink.Calls[count-1]=="1:Attack:False","disabled view releases and rejects callbacks/direct presses");
  v.isActiveAndEnabled=true;Call(v,"OnEnable");a.Send(true);Check(sink.Calls.Count==count+1,"reenable accepts new press");
  sink.Available=false;a.Send(false);sink.Available=true;count=sink.Calls.Count;a.Send(true);
  Check(sink.Calls.Count==count+1,"missing sink release clears local pressed state");
  count=sink.Calls.Count;v.BindPlayer(2);Check(sink.Calls[count]=="1:Attack:False","rebind releases old player before switching");
  a.Send(false);Check(sink.Calls.Count==count+1,"old pointer release not sent to new player");
  a.Send(true);Check(sink.Calls[sink.Calls.Count-1]=="2:Attack:True","new player receives next press");
  v.UnbindPlayer();count=sink.Calls.Count;a.Send(false);Check(sink.Calls[count-1]=="2:Attack:False"&&sink.Calls.Count==count,"unbind release idempotent");
  v.BindPlayer(3);j.Send(true);Call(v,"OnDestroy");count=sink.Calls.Count;Call(v,"OnDestroy");
  Check(j.Listeners==0&&sink.Calls.Count==count&&sink.Calls[count-1]=="3:Jump:False","destroy fallback releases and unsubscribes once");
  Console.WriteLine("TOTAL "+checks+" PASS; actual View source with dependency stubs, not Unity runtime.");
 }
}
