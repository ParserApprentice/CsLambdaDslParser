using System;
namespace A
{
	public delegate void TestDel1(int a);
	public delegate void TestDel2();
	public class B101<T>:B100
		where T:new()
	{
		
	}
	
	[Author(1,"ok",LongName="OKOK12345")]
	public class B100
	{	
		//event
		
		public event EventHandler Ev3{
			add{}
			remove{}		
		}		
		public event EventHandler Ev1,Ev2;
		//fields
		public int Field1=0,Field2=1;		
		//methods
		public void M(){
			var a =1+ 2*3;
			var b = new { x = 20, y = 30 };
			var c = 2;
			if (true) { return; } else { return; }
		} 
	}
	public class AuthorAttribute:Attribute
	{
		public AuthorAttribute(int no,string shortname){}
		public string LongName{get;set;}
	}
}