namespace Robot.Abstractions
{
    public interface ISolutionManager
    {
        string CSharpSolutionPath { get; }
        string CSharpProjectPath { get; }

        void GenerateNewProject();
        void GenerateNewSolution();
    }
}
