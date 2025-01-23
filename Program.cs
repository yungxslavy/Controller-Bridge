using Controllers;

namespace Controller_Bridge
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //XboxToPS4 controller = new();
            PSToPS4 controller = new();
            controller.Start();
            Console.ReadLine();
        }
    }
}
