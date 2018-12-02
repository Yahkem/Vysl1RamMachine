using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vysl1RamMachine
{
    public class Operation
    {
        public bool IsValid { get; set; } = true;
        public string Description { get; set; } = null;
        public Instruction Instruction { get; set; }
        public OperandType OperandType { get; set; } = OperandType.None;
        public double OperationValue { get; set; } = -1;

        public Operation(string operationLine)
        {
            string trimmedLine = operationLine?.Trim();
            const char commentSymptom = '#';
            const char lineNumberSymptom = '.';

            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                Description = "Line is empty";
                IsValid = false;
                return;
            }
            else if (trimmedLine[0] == commentSymptom)
            {
                Description = $"Line is a comment: {trimmedLine}";
                IsValid = false;
                return;
            }

            // postara se o "manualni" cislovani radku - "1. READ" etc.
            
            if (trimmedLine.Contains(lineNumberSymptom))
            {
                int indexOfSymptom = trimmedLine.IndexOf(lineNumberSymptom);
                trimmedLine = trimmedLine.Substring(indexOfSymptom+1).TrimStart();
            }
            if (trimmedLine.Contains(commentSymptom))
            {
                int indexOfSymptom = trimmedLine.IndexOf(commentSymptom);
                trimmedLine = trimmedLine.Substring(0, indexOfSymptom).TrimEnd();
            }

            string[] operationParts = trimmedLine.Split(' ');

            bool isParseSuccessful = Enum.TryParse(operationParts[0].Trim(), out Instruction instruction);

            if (!isParseSuccessful)
            {
                Description = $"Couldn't parse the first part (instruction)";
                IsValid = false;
                return;
            }

            this.Instruction = instruction;
            
            if (Instruction != Instruction.READ &&
                Instruction != Instruction.WRITE &&
                Instruction != Instruction.HALT)
            {
                // has operand
                if (operationParts.Count() == 1)
                {
                    IsValid = false;
                    Description = $"Instruction should have an operand";
                    return;
                }

                string operandPart = operationParts[1]?.Trim();
                
                char firstChar = operandPart[0];

                OperandType = 
                    (firstChar == '*') ? OperandType.Reference :
                    (firstChar == '=') ? OperandType.Constant :
                    OperandType.Value;

                int startSubstringIndex = OperandType == OperandType.Value ? 0 : 1;
                string partToParse = operandPart.Substring(startSubstringIndex).Replace(',', '.');
                var culture = CultureInfo.InvariantCulture;

                isParseSuccessful = double.TryParse(partToParse, NumberStyles.Number, culture, out double operationValue);
                if (isParseSuccessful)
                {
                    OperationValue = operationValue;
                }
                else
                {
                    Description = $"Couldn't parse the second part (operand)";
                    IsValid = false;
                }
            }

            ValidateParsedCombination();
        }

        public override string ToString() => Description;

        private void ValidateParsedCombination()
        {
            if (
                ((Instruction == Instruction.READ ||
                Instruction == Instruction.WRITE ||
                Instruction == Instruction.HALT)
                && OperandType != OperandType.None) ||

                ((Instruction == Instruction.LOAD ||
                Instruction == Instruction.STORE ||
                Instruction == Instruction.ADD ||
                Instruction == Instruction.SUB ||
                Instruction == Instruction.MUL ||
                Instruction == Instruction.DIV)
                && OperandType == OperandType.None) ||

                (Instruction == Instruction.STORE
                && OperandType == OperandType.Constant) ||

                ((Instruction == Instruction.JUMP ||
                Instruction == Instruction.JZERO ||
                Instruction == Instruction.JGTZ ||
                Instruction == Instruction.JHASH)
                && (OperandType != OperandType.Value))
                )
            {
                Description = $"Invalid operation: ";
                IsValid = false;
            }

            Description = (Description ?? "") + $"{Instruction.ToString()}" + 
                ((Instruction == Instruction.WRITE || Instruction == Instruction.READ || Instruction == Instruction.HALT) ? 
                string.Empty : 
                $" {OperationValue} by {OperandType.ToString()}");
        }
    }
}
