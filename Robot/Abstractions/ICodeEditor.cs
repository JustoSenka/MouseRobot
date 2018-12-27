using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robot.Abstractions
{
    public interface ICodeEditor
    {
        bool StartEditor(string solutionPath);
        bool FocusFile(string filePath);
    }
}
