using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab2Halma.Interfaces
{
    internal interface IValidatable
    {
        public abstract static bool IsValid(IValidatable? validatable);
    }
}
