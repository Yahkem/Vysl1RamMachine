﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Vysl1RamMachine
{
    public class ControlUnit
    {
        private int iteration = 1;
        private bool halt = false;
        private readonly int lineNumberModifier;
        private const int secondsUntilExecutionBreak = 8;

        /// <summary>
        /// Hodnota oznacujici prazdny znak (pro JHASH). Predelavat register na int? by bylo uz moc prekopavani :D
        /// </summary>
        public const int HASH_VALUE = int.MaxValue;

        public int MaxOperationsBeforeHalt { get; set; } = 9999999;

        public Tape<int> InputTape { get; set; } = new Tape<int>();
        public Tape<double> OutputTape { get; set; } = new Tape<double>();

        public Dictionary<int, Operation> ProgramTape { get; set; }
        public int ProgramCounter { get; set; } = 0;

        public Dictionary<double, double> Register { get; set; } = new Dictionary<double, double>();

        public List<string> ExecutionHistory { get; set; } = new List<string>();
        
        public ControlUnit(string inputLine, IList<Operation> operations, bool areLinesNumberedFromZero = true)
        {
            lineNumberModifier = areLinesNumberedFromZero ? 0 : -1;

            InputTape.TapeContent = inputLine.ToCharArray()
                .Select(c => (c == '#') ? HASH_VALUE : (int)char.GetNumericValue(c))
                .ToList();

            int i = -1;
            ProgramTape = operations.ToDictionary(op => ++i);
        }

        public string Run()
        {
            string result = "";
            Stopwatch sw = Stopwatch.StartNew();

            while (!halt)
            {
                Operation opToInterpret;
                try
                {
                    opToInterpret = ProgramTape[ProgramCounter];

                    InterpretOperation(opToInterpret);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                if (sw.Elapsed.Seconds > secondsUntilExecutionBreak)
                    throw new Exception("RAM interrupted program execution - it took too much time (possible infinite loop or invalid instruction).");

                if (iteration == MaxOperationsBeforeHalt)
                    throw new Exception("RAM interrupted program execution - too many operations (possible infinite loop).");

                ++iteration;
            }

            halt = false;

            result = string.Join("", OutputTape.TapeContent.Select(i => i.ToString("0.####")));

            return result;
        }

        private void InterpretOperation(Operation op)
        {
            double val = 0d;
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
                    ProgramCounter = (int)(op.OperationValue + lineNumberModifier); // indexy jsou od 0, radky muzou byt od 1
                    break;
                case Instruction.JZERO:
                    if (Register[0] == 0)
                        ProgramCounter = (int)(op.OperationValue + lineNumberModifier); // indexy jsou od 0, radky muzou byt od 1
                    else
                        ++ProgramCounter;

                    break;
                case Instruction.JGTZ:
                    if (Register[0] > 0)
                        ProgramCounter = (int)(op.OperationValue + lineNumberModifier); // indexy jsou od 0, radky muzou byt od 1
                    else
                        ++ProgramCounter;

                    break;
                case Instruction.HALT:
                    this.halt = true;
                    break;
                case Instruction.JHASH:
                    if (Register[0] == HASH_VALUE)
                        ProgramCounter = (int)(op.OperationValue + lineNumberModifier); // indexy jsou od 0, radky muzou byt od 1
                    else
                        ++ProgramCounter;

                    break;
                default:
                    throw new InvalidOperationException("wtf?");
            }

            ExecutionHistory.Add($"{iteration}. { op.ToString()}");
            Register.ToList().ForEach(kv => ExecutionHistory.Add($"\tR[{kv.Key}]={kv.Value}"));
            ExecutionHistory.Add(string.Empty);
        }
    }
}
