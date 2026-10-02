using System;
using NTSD.UI.Menu;
int checks=0;
void Check(bool result,string reason) { checks++; if(!result) throw new Exception(reason); }
foreach(int direction in new[]{-1,1})
{
 var m=new MenuCarouselMotion();m.Reset(7,0);
 for(int step=1;step<=700;step++)
 {
  m.Move(direction);
  Check(m.SelectedIndex==MenuCarouselMotion.WrapIndex(step*direction,7),"signed selected index");
  for(int f=0;f<120;f++)
  {
   double[] before=new double[7];
   for(int i=0;i<7;i++)before[i]=m.Offset(i);
   m.Advance(1.0/60,0.085);
   for(int i=0;i<7;i++)
    if(Math.Abs(before[i])<3 && Math.Abs(m.Offset(i))<3)
     Check(Math.Abs(m.Offset(i)-before[i])<0.3,"visible seam jump");
  }
  Check(m.IsSettled,"snap not settled");
 }
 Check(m.SelectedIndex==0,"full turn");
}
var rapid=new MenuCarouselMotion();rapid.Reset(7,0);
for(int i=1;i<=10001;i++)
{
 rapid.Move(1);Check(rapid.SelectedIndex==i%7,"rapid index loss");
 rapid.Advance(0.001,0.085);Check(rapid.Position>=0&&rapid.Position<7,"unbounded phase");
}
rapid.BeginDrag();rapid.Drag(700.4);int beforeDrag=rapid.SelectedIndex;
rapid.Move(1);Check(rapid.SelectedIndex==beforeDrag,"drag navigation conflict");
rapid.EndDrag();rapid.FinishSnap();Check(rapid.IsSettled,"drag snap");
rapid.Reset(7,0);rapid.Select(6);Check(rapid.Target==-1,"nearest wrap");
rapid.FinishSnap();Check(rapid.SelectedIndex==6&&rapid.Offset(6)==0,"center");
rapid.Reset(7,0);rapid.BeginDrag();rapid.Drag(-700.7);rapid.EndDrag();rapid.FinishSnap();
Check(rapid.SelectedIndex==6,"negative drag rounds");
Console.WriteLine("PASS "+checks+" production-motion assertions; 100 forward and 100 reverse turns, 10001 queued inputs, drag/snap/nearest wrap.");
