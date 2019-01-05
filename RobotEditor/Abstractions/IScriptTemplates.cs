namespace RobotEditor.Abstractions
{
    public interface IScriptTemplates
    {
        string[] TemplateNames { get; }

        string GetTemplateFileName(string name);
        string GetTemplate(string name);
    }
}
