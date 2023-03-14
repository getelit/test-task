using System;
using System.ServiceModel;

namespace MVHost
{
    internal class Host
    {
        static void Main(string[] args)
        {
            using (var host = new ServiceHost(typeof(wcf_MV.ServiceMV)))
            {
                host.Open();
                Console.WriteLine("Хост запущен!");
                Console.ReadKey();
            }
        }
    }
}
