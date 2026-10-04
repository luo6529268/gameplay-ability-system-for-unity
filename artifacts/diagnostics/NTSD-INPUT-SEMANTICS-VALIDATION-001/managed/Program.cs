
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
 Console.WriteLine("MANAGED PROBE COMPLETE");
 }
}
