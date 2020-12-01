using System;
using TestApp.Models;

namespace TestApp.Services
{
    public static class TestService
    {
        public static void Test(string testValue)
        {
            var test = new TestModel
            {
                TestValue = testValue
            };

            WriteTesting(test);
        }

        private static void WriteTesting(TestModel testModel)
        {
            Console.WriteLine(testModel.TestValue);
        }
    }
}