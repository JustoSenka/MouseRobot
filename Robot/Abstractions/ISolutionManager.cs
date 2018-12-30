namespace Robot.Abstractions
{
    public interface ISolutionManager
    {
        string CSharpSolutionPath { get; }
        string CSharpProjectPath { get; }

        string CSharpSolutionName { get; }
        string CSharpProjectName { get; }

        void GenerateNewProject();
        void GenerateNewSolution();
    }
}
