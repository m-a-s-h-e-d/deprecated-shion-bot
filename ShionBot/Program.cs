using System;
using System.Threading.Tasks;

namespace ShionBot
{
    class Program
    {
        public static async Task Main(string[] args)
            => await Startup.RunAsync(args);
    }
}
