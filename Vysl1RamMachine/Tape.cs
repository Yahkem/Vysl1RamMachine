using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vysl1RamMachine
{
    public class Tape<T>
    {
        public List<T> TapeContent { get; set; } = new List<T>();
        public int Index { get; set; } = 0;
    }
}
