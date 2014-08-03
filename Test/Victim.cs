using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class Victim
    {
        private int Foo(string s, object o)
        {
            return 10;
        }

        public int CallFoo(string s, object o)
        {
            return Foo(s, o);
        }

        public string Bar
        {
            get { return "aaa"; }
            set { }
        }

        public static bool Baz(object o)
        {
            return true;
        }
    }
}
