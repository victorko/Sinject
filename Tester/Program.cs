using Sinject.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Stubs.TryInject(typeof(Victim), new VictimStub());

            var v = new Victim();
            var res = v.CallFoo("sss", new object());
            var res2 = v.Bar;
            v.Bar = "dddd";
            var res3 = Victim.Baz(new object());
        }

        private class VictimStub
        {
            public int Foo(Victim self, string s, object o)
            {
                return 20;
            }

            public string get_Bar(Victim self)
            {
                return "sss";
            }

            public void set_Bar(Victim self, string value)
            {
            }

            public bool Baz(object o)
            {
                return false;
            }
        }
    }
}
