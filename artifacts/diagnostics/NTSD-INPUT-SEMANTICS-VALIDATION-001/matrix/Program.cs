
using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;
class Program {
 static Assembly a;static BindingFlags all=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
 static object New(string n)=>Activator.CreateInstance(a.GetType(n,true),true);
 static object Call(object o,string n,params object[] args)=>o.GetType().GetMethods(all).Single(m=>m.Name==n&&m.GetParameters().Length==args.Length).Invoke(o,args);
 static object Get(object o,string n)=>o.GetType().GetField(n,all)?.GetValue(o)??o.GetType().GetProperty(n,all).GetValue(o);
 static void Set(object o,string n,object v){var f=o.GetType().GetField(n,all);if(f!=null)f.SetValue(o,v);else o.GetType().GetProperty(n,all).SetValue(o,v);}
 static object Act(string s)=>Enum.Parse(a.GetType("NTSD.App.BattleInputAction"),s);
 static void UI(object o,string s,bool p)=>Call(o,"NTSD.App.IPlayerActionInputSink.SetActionPressed",Act(s),p);
 static void Device(object o,string s,bool p)=>Call(o,"Set"+s+"ActionPressed",p);
 static object Held(object o)=>Call(o,"NTSD.Simulation.ILocalFrameInputSource.CaptureHeldSimulationButtons");
 static void Check(bool ok,string s){if(!ok)throw new Exception(s);Console.WriteLine("PASS "+s);}
 static void Main(){
 var root=Directory.GetCurrentDirectory();var dirs=new[]{Path.Combine(root,"artifacts/diagnostics/NTSD-BUTTON-PRESS-CONSISTENCY-001/build"),Path.Combine(root,"Library/ScriptAssemblies"),@"D:\Unity\HubEditor\2022.3.62f3\Editor\Data\Managed\UnityEngine",@"D:\Unity\HubEditor\2022.3.62f3\Editor\Data\Managed"};
 AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{var n=new AssemblyName(e.Name).Name+".dll";foreach(var d in dirs){var p=Path.Combine(d,n);if(File.Exists(p))return Assembly.LoadFrom(p);}return null;};
 a=Assembly.LoadFrom(Path.Combine(dirs[0],"Assembly-CSharp.dll"));Console.WriteLine("Actual managed assembly: "+a.Location);
 var c=New("NTSD.Game.CharacterInputModule");Console.WriteLine("created actual CharacterInputModule");
 UI(c,"Attack",true);Console.WriteLine("down held="+Held(c));UI(c,"Attack",false);Console.WriteLine("up held="+Held(c));Check(Convert.ToUInt64(Held(c))==0,"down/up before sample ends held=None");
 var buffer=Get(c,"InputBuffer");object[] args={1,null};var dequeued=(bool)buffer.GetType().GetMethod("TryDequeueAll").Invoke(buffer,args);Console.WriteLine("raw buffered events="+Get(args[1],"Count"));Check(dequeued&&(int)Get(args[1],"Count")==2,"raw down/up were enqueued, disappearance is after event production");
 Device(c,"Attack",true);UI(c,"Attack",true);UI(c,"Attack",false);Check(Convert.ToUInt64(Held(c))!=0,"UI release preserves keyboard hold");Device(c,"Attack",false);Check(Convert.ToUInt64(Held(c))==0,"both sources released");UI(c,"Attack",true);Device(c,"Attack",true);Device(c,"Attack",false);Check(Convert.ToUInt64(Held(c))!=0,"keyboard release preserves UI hold");UI(c,"Attack",false);
 var runtime=New("NTSD.Simulation.NTSDEntityRuntime");Console.WriteLine("created actual NTSDEntityRuntime");var combo=a.GetType("NTSD.Simulation.NTSD28NativeComboStateMachine",true);combo.GetMethod("InitializeNativeHistory",all).Invoke(null,new[]{runtime});var proxy=Get(runtime,"NativeInputProxy");var current=(byte[])Get(proxy,"Current");var previous=(byte[])Get(proxy,"Previous");var edges=(byte[])Get(proxy,"EdgeWindow");var process=combo.GetMethod("ProcessSampledInput",all);
 for(int tick=1;tick<=8;tick++){Array.Copy(current,previous,7);current[4]=(byte)(tick<=6?1:0);var mask=process.Invoke(null,new object[]{runtime,false});Console.WriteLine($"tick={tick} prevA={previous[4]} currentA={current[4]} edgeA={edges[0]} risingMask={mask}");if(tick==1)Check(edges[0]==5,"first down produces edge window5");if(tick==6)Check(edges[0]==0,"holding does not refresh rising window");if(tick==7)Check(Convert.ToUInt32(mask)==0,"release does not produce rising edge");}
 
 foreach(var test in new[]{("same-render-interval",10.0,12.0,false),("different-render-same-logic",10.0,20.0,false),("cross-33ms",32.0,34.0,true),("after33-before33.333",33.1,33.2,false),("short-straddles66",65.9,66.1,true)}){
  var cc=New("NTSD.Game.CharacterInputModule");var rr=New("NTSD.Simulation.NTSDEntityRuntime");combo.GetMethod("InitializeNativeHistory",all).Invoke(null,new[]{rr});var pp=Get(rr,"NativeInputProxy");var cur=(byte[])Get(pp,"Current");var prev=(byte[])Get(pp,"Previous");var ee=(byte[])Get(pp,"EdgeWindow");bool downDone=false,upDone=false,detected=false;
  Console.WriteLine($"CASE {test.Item1} down={test.Item2}ms up={test.Item3}ms render=16.6667ms nominal; logic=0,33,66,99ms");
  for(int tick=0;tick<4;tick++) {double time=tick*33.0;if(!downDone&&test.Item2<time){UI(cc,"Attack",true);downDone=true;}if(!upDone&&test.Item3<time){UI(cc,"Attack",false);upDone=true;}
   var held=Held(cc);Array.Copy(cur,prev,7);cur[4]=(byte)(held.ToString().Contains("Jump")?1:0);var rising=Convert.ToUInt32(process.Invoke(null,new object[]{rr,false}));detected|=rising!=0;Console.WriteLine($" t={time} tick={tick} prevA={prev[4]} heldA={cur[4]} pressedA={(rising&16)!=0} releasedA={prev[4]!=0&&cur[4]==0} edgeA={ee[0]}");
  }Check(detected==test.Item4,"sample detection matches "+test.Item1);
 }
 var cc2=New("NTSD.Game.CharacterInputModule");UI(cc2,"Attack",true);UI(cc2,"Attack",false);var bb=Get(cc2,"InputBuffer");Call(bb,"DiscardTick",1);object[] emptyArgs={1,null};Check(!(bool)bb.GetType().GetMethod("TryDequeueAll").Invoke(bb,emptyArgs),"canonical BeforeSimTick DiscardTick removes queued down/up");
 var state=New("NTSD.Input.NTSDInputStateModule");var bb2=Get(New("NTSD.Game.CharacterInputModule"),"InputBuffer");

 var keyType=a.GetType("NTSD.Input.FuncKeyMask");var jumpKey=Enum.Parse(keyType,"jump");Call(bb2,"EnqueueForTick",1,jumpKey,true);Call(bb2,"EnqueueForTick",1,jumpKey,false);Call(state,"UpdateFromBuffer",bb2,1,null);Check(!(bool)Get(state,"Jump"),"actual input state collapses same-tick direct down/up to released");
 Call(bb2,"EnqueueForTick",2,jumpKey,true);Call(bb2,"EnqueueCompletePacketKeyForTick",2,jumpKey,false);Call(state,"UpdateFromBuffer",bb2,2,null);Check(!(bool)Get(state,"Jump"),"actual input state complete packet overrides direct down event");
 var r2=New("NTSD.Simulation.NTSDEntityRuntime");combo.GetMethod("InitializeNativeHistory",all).Invoke(null,new[]{r2});var p2=Get(r2,"NativeInputProxy");var c2=(byte[])Get(p2,"Current");var v2=(byte[])Get(p2,"Previous");
 foreach(int key in new[]{6,3,5}){Array.Copy(c2,v2,7);Array.Clear(c2);c2[key]=1;process.Invoke(null,new object[]{r2,false});Console.WriteLine("D-right-J states="+string.Join(",",(byte[])Get(p2,"ComboState")));}
 var frame=New("NTSD.Animation.LF2FrameData");Set(frame,"hit_Fj",240);object[] routeArgs={frame,r2,null};var selector=a.GetType("NTSD.Simulation.NTSD28NativeComboRouteSelector").GetMethod("TrySelect",all);Check((bool)selector.Invoke(null,routeArgs)&&(int)Get(routeArgs[2],"RequestedAction")==240,"actual combo route D-right-J selects Naruto DAT240");
 var r3=New("NTSD.Simulation.NTSDEntityRuntime");combo.GetMethod("InitializeNativeHistory",all).Invoke(null,new[]{r3});var p3=Get(r3,"NativeInputProxy");((byte[])Get(p3,"Current"))[6]=1;process.Invoke(null,new object[]{r3,false});object[] noRoute={frame,r3,null};Check(!(bool)selector.Invoke(null,noRoute),"D without direction/J does not select rasengan");
 try {var ch=New("NTSD.Animation.LF2Objects.LF2Character");Console.WriteLine("ACTUAL CHARACTER CREATED");Console.WriteLine("FrameType="+Get(ch,"Frame").GetType());Console.WriteLine("Cache="+Get(ch,"FrameCache"));} catch(Exception ex){Console.WriteLine("CHARACTER HOST LIMIT "+ex.ToString());}
 Console.WriteLine("MATRIX COMPLETE");

 }
}
