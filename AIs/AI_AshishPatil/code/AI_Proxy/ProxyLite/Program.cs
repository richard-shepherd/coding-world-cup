using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Helper;

namespace ProxyLite
{
    class Program
    {
        static void Main(string[] args)
        {
             IFootBalStrategy s = new GameStrategy();

            while (true)
            {
                var x = Console.ReadLine();

                var response = s.Play(x);
                if ( !string.IsNullOrEmpty(response))
                {
                    Console.WriteLine(response);
                }

            }
        }
    }
}
