
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

 var unityCore=Assembly.LoadFrom(Path.Combine(dirs[2],"UnityEngine.CoreModule.dll"));var logger=unityCore.GetType("UnityEngine.Debug").GetProperty("unityLogger").GetValue(null);Set(logger,"logEnabled",false);Console.WriteLine("Only this standalone process Unity logger disabled to avoid native logging ECall; gameplay code unchanged.");
 var parser=New("NTSD.DatParser.Lf2DatParserV2");var converter=a.GetType("NTSD.DatParser.Lf2DatConverter");var writer=New("NTSD.Simulation.Ecs.BattleCharacterActionWriter");

 var path=Path.Combine(root,"Assets/NTSD/Content/LoganRuntime/decoded_dat/c/ank/ank.dat");var parsed=Call(parser,"ParseLoganContent",File.ReadAllText(path),path);var data=New("NTSD.Animation.LF2CharacterData");var frames=(System.Collections.IList)Get(data,"frames");foreach(var block in (System.Collections.IEnumerable)Get(parsed,"Frames"))frames.Add(converter.GetMethod("ConvertLoganFrameData").Invoke(null,new[]{block}));
 var wrap=Activator.CreateInstance(a.GetType("NTSD.Animation.LF2CharacterDataWrapper"),new object[]{1,data});var ch=New("NTSD.Animation.LF2Objects.LF2Character");var cache=New("NTSD.Animation.LF2FrameCache");Set(ch,"FrameCache",cache);Call(cache,"Load",wrap);var rt=Get(ch,"Runtime");var fi=Get(ch,"Frame");Set(fi,"D",Call(cache,"GetNativeFrameDataById",0));Set(fi,"N",0);Set(rt,"HP",500);Set(rt,"PP",500);Set(rt,"InputLocalResourceEnabled49D034",true);
 var tracker=New("NTSD.Simulation.BattleHudChangeTracker");var handle=Activator.CreateInstance(tracker.GetType().GetMethod("Bind",all).GetParameters()[2].ParameterType,new object[]{0,(uint)1});Call(tracker,"Bind",0,rt,handle);object[] consume={null};tracker.GetType().GetMethod("TryConsume",all).Invoke(tracker,consume);
 var result=Call(writer,"ApplyNativeInputAction",ch,353);Console.WriteLine("actual action353 applied="+Get(result,"Applied")+" PP="+Get(rt,"PP")+" legacyMP="+Get(rt,"MP"));consume[0]=null;bool dirty=(bool)tracker.GetType().GetMethod("TryConsume",all).Invoke(tracker,consume);Console.WriteLine("HUD dirty after real skill="+dirty);Check((bool)Get(result,"Applied")&&(int)Get(rt,"PP")==425,"actual Anko353 mp75 consumption");Check(!dirty,"RED confirmed: actual skill deducts PP but HUD produces no event");
 }
}
