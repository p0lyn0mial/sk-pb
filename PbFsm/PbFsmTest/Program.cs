using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PbFsmTest.Fsm;

namespace PbFsmTest
{
    class Program
    {
        static void Main(string[] args)
        {
            PhotoBoothFsm fsm = new PhotoBoothFsm();
            fsm.Start();

            Console.ReadKey();
            fsm.Stop();
        }
    }
}
