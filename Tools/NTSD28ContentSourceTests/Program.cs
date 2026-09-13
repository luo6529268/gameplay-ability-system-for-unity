using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

internal static class Program
{
    private static int Main()
    {
        var fixture = new NTSD.Test.Editor.NTSD28B11ContentSourceEditorTests();
        int passed = 0;
        int failed = 0;
        foreach (MethodInfo method in fixture.GetType().GetMethods().OrderBy(method => method.Name))
        {
            TestCaseAttribute[] cases = method.GetCustomAttributes<TestCaseAttribute>().ToArray();
            object[][] arguments = cases.Select(item => item.Arguments).ToArray();
            if (arguments.Length == 0 && method.GetCustomAttribute<TestAttribute>() != null)
                arguments = new[] { Array.Empty<object>() };
            foreach (object[] argument in arguments)
            {
                try
                {
                    method.Invoke(fixture, argument);
                    passed++;
                    Console.WriteLine("PASS " + method.Name);
                }
                catch (TargetInvocationException error)
                {
                    failed++;
                    Console.WriteLine("FAIL " + method.Name + ": " + error.InnerException?.Message);
                }
            }
        }
        Console.WriteLine($"SOURCE_LINKED_NUNIT_ONLY passed={passed} failed={failed}");
        return failed == 0 && passed > 0 ? 0 : 1;
    }
}
