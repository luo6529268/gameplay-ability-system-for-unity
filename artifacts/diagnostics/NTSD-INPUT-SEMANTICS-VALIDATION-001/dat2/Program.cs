
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

 var parser=New("NTSD.DatParser.Lf2DatParserV2");var converter=a.GetType("NTSD.DatParser.Lf2DatConverter");var writer=New("NTSD.Simulation.Ecs.BattleCharacterActionWriter");
 foreach(var test in new[]{("nar",0,240,5),("ita",318,359,4),("ank",439,470,5)}){
 try {
 var path=Path.Combine(root,$"Assets/NTSD/Content/LoganRuntime/decoded_dat/c/{test.Item1}/{test.Item1}.dat");var parsed=Call(parser,"ParseLoganContent",File.ReadAllText(path),path);var data=New("NTSD.Animation.LF2CharacterData");var frames=(System.Collections.IList)Get(data,"frames");
 foreach(var block in (System.Collections.IEnumerable)Get(parsed,"Frames"))frames.Add(converter.GetMethod("ConvertLoganFrameData").Invoke(null,new[]{block}));
 var wrap=Activator.CreateInstance(a.GetType("NTSD.Animation.LF2CharacterDataWrapper"),new object[]{1,data});var ch=New("NTSD.Animation.LF2Objects.LF2Character");var cache=Get(ch,"FrameCache");Call(cache,"Load",wrap);var rt=Get(ch,"Runtime");var fi=Get(ch,"Frame");var src=Call(cache,"GetNativeFrameDataById",test.Item2);var proxy=Get(rt,"NativeInputProxy");var prev=(byte[])Get(proxy,"Previous");var cur=(byte[])Get(proxy,"Current");var edge=(byte[])Get(proxy,"EdgeWindow");
 Set(fi,"D",src);Set(fi,"N",test.Item2);Set(rt,"HP",500);Set(rt,"PP",500);Set(rt,"InputLocalResourceEnabled49D034",true);
 Console.WriteLine($"DAT={path} frames={frames.Count} source={test.Item2} target={test.Item3} targetMp={Get(Call(cache,"GetNativeFrameDataById",test.Item3),"mp")} targetState={Get(Call(cache,"GetNativeFrameDataById",test.Item3),"state")}");
 if(test.Item1=="nar"){
  var combo=a.GetType("NTSD.Simulation.NTSD28NativeComboStateMachine");combo.GetMethod("InitializeNativeHistory",all).Invoke(null,new[]{rt});var process=combo.GetMethod("ProcessSampledInput",all);
  foreach(int k in new[]{6,3,5}){Array.Copy(cur,prev,7);Array.Clear(cur);cur[k]=1;process.Invoke(null,new object[]{rt,false});}
  var res=Call(writer,"RouteNativeComboAction",ch);Console.WriteLine("Naruto routed frame="+Get(fi,"N"));Check((int)Get(fi,"N")==240,"real parser+combo+writer applies Naruto240");
 }else{
  Call(writer,"RouteNativeThreeButtonFields",ch);Check((int)Get(fi,"N")==test.Item2,test.Item1+" no hold => no input route");
  prev[test.Item4]=1;cur[test.Item4]=1;Call(writer,"RouteNativeThreeButtonFields",ch);int expected=test.Item1=="ank"?443:359;Console.WriteLine(test.Item1+" held requested="+test.Item3+" actualFrame="+Get(fi,"N"));Check((int)Get(fi,"N")==expected,test.Item1+" held route applied incl encoded-state redirect");
  Set(fi,"D",src);Set(fi,"N",test.Item2);Array.Clear(edge);prev[test.Item4]=1;cur[test.Item4]=0;Call(writer,"RouteNativeThreeButtonFields",ch);Check((int)Get(fi,"N")==expected,test.Item1+" prior-held/current-up sample still satisfies hold field");
  Set(fi,"D",src);Set(fi,"N",test.Item2);Array.Clear(prev);Array.Clear(cur);Call(writer,"RouteNativeThreeButtonFields",ch);Check((int)Get(fi,"N")==test.Item2,test.Item1+" next fully released sample cannot hold-route");
  prev[test.Item4]=1;Set(rt,"InputActionLock130",1);Call(writer,"RouteNativeThreeButtonFields",ch);Check((int)Get(fi,"N")==test.Item2,test.Item1+" action lock rejects route");Set(rt,"InputActionLock130",0);
  if(test.Item1=="ank"){
   Set(rt,"HP",150);prev[5]=1;Call(writer,"RouteNativeThreeButtonFields",ch);Check((int)Get(fi,"N")==470,"Anko HP150 does not redirect encoded1150443");
   Set(rt,"HP",500);Set(rt,"PP",0);var result=Call(writer,"ApplyNativeInputAction",ch,353);Console.WriteLine("Anko353 lowPP failure="+Get(result,"Failure"));Check(!(bool)Get(result,"Applied"),"real DAT Anko353 mp75 rejected with PP0");
  }
 }
 }catch(Exception ex){Console.WriteLine("DAT BLOCKED "+test.Item1+" "+ex);}
 }
 Console.WriteLine("DAT EXECUTION COMPLETE");
 }
}
