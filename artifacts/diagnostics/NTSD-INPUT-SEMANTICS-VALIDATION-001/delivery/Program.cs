
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

 var combo=a.GetType("NTSD.Simulation.NTSD28NativeComboStateMachine");var process=combo.GetMethod("ProcessSampledInput",all);
 foreach(var x in new[]{("60Hz-short-in-first-render",10.0,12.0,16.6666667,false),("60Hz-cross-render-same-sample",10.0,20.0,16.6666667,false),("60Hz-cross-actual-sample",32.0,34.0,16.6666667,true),("60Hz-cross-ideal66-only",65.9,66.1,16.6666667,false),("30Hz-cross-ideal33-only",33.1,33.2,33.3333333,false),("stalled-render-two-ticks",32.0,34.0,66.0,false),("multi-tick-hold-release",10.0,150.0,16.6666667,true)}){
  var c=New("NTSD.Game.CharacterInputModule");var r=New("NTSD.Simulation.NTSDEntityRuntime");combo.GetMethod("InitializeNativeHistory",all).Invoke(null,new[]{r});var p=Get(r,"NativeInputProxy");var cur=(byte[])Get(p,"Current");var prev=(byte[])Get(p,"Previous");var edge=(byte[])Get(p,"EdgeWindow");bool down=false,up=false,detected=false;double debt=0;int tick=0;
  Console.WriteLine($"CASE {x.Item1}; OS-event times down={x.Item2}ms up={x.Item3}ms; fixture delivers all pending callbacks before sampling at render increments={x.Item4}; logical cadence33ms, clamp66ms,max2");
  for(int render=1;render<=12;render++){double t=render*x.Item4; if(!down&&x.Item2<=t){UI(c,"Attack",true);down=true;}if(!up&&x.Item3<=t){UI(c,"Attack",false);up=true;}debt=Math.Min(66,debt+x.Item4);int burst=0;
   while(debt+0.000001>=33&&burst<2){debt-=33;burst++;tick++;Array.Copy(cur,prev,7);cur[4]=(byte)(Held(c).ToString().Contains("Jump")?1:0);uint mask=Convert.ToUInt32(process.Invoke(null,new object[]{r,false}));detected|=(mask&16)!=0;Console.WriteLine($" render#{render} time={t:F4}ms logicTick={tick} held={cur[4]} rising={(mask&16)!=0} released={prev[4]!=0&&cur[4]==0} edge={edge[0]}");}
   if(t>x.Item3+66)break;
  }Check(detected==x.Item5,"delivery model "+x.Item1);
 }
 Console.WriteLine("DELIVERY MATRIX COMPLETE. Real input and combo classes; render/OS delivery schedule is a deterministic fixture, not measured Unity hardware timing.");
 }
}
