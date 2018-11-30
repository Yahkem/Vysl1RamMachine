using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vysl1RamMachine
{
    public enum Instruction
    {
        READ, WRITE, LOAD, STORE, ADD, SUB, MUL, DIV, JUMP, JZERO, JGTZ, HALT, JHASH
    }
}
