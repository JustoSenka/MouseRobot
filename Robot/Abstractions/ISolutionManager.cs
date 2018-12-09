namespace Robot.Abstractions
{
    public interface ISolutionManager
    {
        void GenerateNewProject();
        void GenerateNewSolution(bool forceGenerate = false);
    }
}
