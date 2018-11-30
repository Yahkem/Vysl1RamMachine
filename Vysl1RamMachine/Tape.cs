using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vysl1RamMachine
{
    public class Tape
    {
        public List<int> TapeContent { get; set; } = new List<int>();
        public int Index { get; set; } = 0;
    }
}
