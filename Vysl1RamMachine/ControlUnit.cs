using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vysl1RamMachine
{
    public class ControlUnit
    {
        private bool halt = false;
        private int lineNumberModifier;

        /// <summary>
        /// Hodnota oznacujici prazdny znak (pro JHASH). Predelavat register na int? by bylo uz moc prekopavani :D
        /// </summary>
        public const int HASH_VALUE = int.MaxValue;

        public Tape InputTape { get; set; } = new Tape();
        public Tape OutputTape { get; set; } = new Tape();

        public Dictionary<int, Operation> ProgramTape { get; set; }
        public int ProgramCounter { get; set; } = 0;

        public Dictionary<int, int> Register { get; set; } = new Dictionary<int, int>();
        
        public ControlUnit(string inputLine, IList<Operation> operations, bool areLinesNumberedFromZero = true)
        {
            this.lineNumberModifier = areLinesNumberedFromZero ? 0 : -1;

            InputTape.TapeContent = inputLine.ToCharArray()
                .Select(c => (c=='#') ? HASH_VALUE : (int)char.GetNumericValue(c))
                .ToList();

            int i = -1;
            ProgramTape = operations.ToDictionary(op => ++i);
        }

        public string Run()
        {
            string result = "";

            while (!halt)
            {
                var opToInterpret = ProgramTape[ProgramCounter];

                InterpretOperation(opToInterpret);
            }

            halt = false;
            result = string.Join("", OutputTape.TapeContent.Select(i => i.ToString()));
            
            return result;
        }

        private void InterpretOperation(Operation op)
        {
            int val = 0;
            switch (op.Instruction)
            {
                case Instruction.READ:
                    Register[0] = InputTape.TapeContent[InputTape.Index];
                    ++InputTape.Index;
                    ++ProgramCounter;
                    break;
                case Instruction.WRITE:
                    OutputTape.TapeContent.Add(Register[0]);
                    ++OutputTape.Index;
                    ++ProgramCounter;
                    break;
                case Instruction.LOAD:
                    switch (op.OperandType)
                    {
                        case OperandType.Value:
                            Register[0] = Register[op.OperationValue];
                            break;
                        case OperandType.Reference:
                            Register[0] = Register[Register[op.OperationValue]];
                            break;
                        case OperandType.Constant:
                            Register[0] = op.OperationValue;
                            break;
                    }

                    ++ProgramCounter;

                    break;
                case Instruction.STORE:
                    switch (op.OperandType)
                    {
                        case OperandType.Value:
                            Register[op.OperationValue] = Register[0];
                            break;
                        case OperandType.Reference:
                            Register[Register[op.OperationValue]] = Register[0];
                            break;
                    }

                    ++ProgramCounter;

                    break;
                case Instruction.ADD:
                    switch (op.OperandType)
                    {
                        case OperandType.Value:
                            val = Register[op.OperationValue];
                            break;
                        case OperandType.Reference:
                            val = Register[Register[op.OperationValue]];
                            break;
                        case OperandType.Constant:
                            val = op.OperationValue;
                            break;
                    }

                    Register[0] += val;
                    ++ProgramCounter;

                    break;
                case Instruction.SUB:
                    switch (op.OperandType)
                    {
                        case OperandType.Value:
                            val = Register[op.OperationValue];
                            break;
                        case OperandType.Reference:
                            val = Register[Register[op.OperationValue]];
                            break;
                        case OperandType.Constant:
                            val = op.OperationValue;
                            break;
                    }

                    Register[0] -= val;
                    ++ProgramCounter;
                    
                    break;
                case Instruction.MUL:
                    switch (op.OperandType)
                    {
                        case OperandType.Value:
                            val = Register[op.OperationValue];
                            break;
                        case OperandType.Reference:
                            val = Register[Register[op.OperationValue]];
                            break;
                        case OperandType.Constant:
                            val = op.OperationValue;
                            break;
                    }

                    Register[0] *= val;
                    ++ProgramCounter;

                    break;
                case Instruction.DIV:
                    switch (op.OperandType)
                    {
                        case OperandType.Value:
                            val = Register[op.OperationValue];
                            break;
                        case OperandType.Reference:
                            val = Register[Register[op.OperationValue]];
                            break;
                        case OperandType.Constant:
                            val = op.OperationValue;
                            break;
                    }

                    if (val == 0)
                        throw new DivideByZeroException("DIV op by 0");

                    Register[0] /= val;
                    ++ProgramCounter;

                    break;
                case Instruction.JUMP:
                    ProgramCounter = op.OperationValue + lineNumberModifier; // indexy jsou od 0, radky muzou byt od 1
                    break;
                case Instruction.JZERO:
                    if (Register[0] == 0)
                        ProgramCounter = op.OperationValue + lineNumberModifier; // indexy jsou od 0, radky muzou byt od 1
                    else
                        ++ProgramCounter;

                    break;
                case Instruction.JGTZ:
                    if (Register[0] > 0)
                        ProgramCounter = op.OperationValue + lineNumberModifier; // indexy jsou od 0, radky muzou byt od 1
                    else
                        ++ProgramCounter;

                    break;
                case Instruction.HALT:
                    this.halt = true;
                    break;
                case Instruction.JHASH:
                    if (Register[0] == HASH_VALUE)
                        ProgramCounter = op.OperationValue + lineNumberModifier; // indexy jsou od 0, radky muzou byt od 1
                    else
                        ++ProgramCounter;

                    break;
                default:
                    throw new InvalidOperationException("wtf?");
            }
        }
    }
}
