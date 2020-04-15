using System.ServiceProcess;

namespace MuTest.Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var ServicesToRun = new ServiceBase[]
            {
                new MuTestService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
