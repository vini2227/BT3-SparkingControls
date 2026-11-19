using System;using System.Reflection;
static class SafeFadeRunner {
 [STAThread] static int Main(string[] args){try{typeof(MenuFadeFixture).GetMethod("Main",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{args});return 0;}catch(Exception ex){Console.WriteLine(ex);return 1;}}
}
