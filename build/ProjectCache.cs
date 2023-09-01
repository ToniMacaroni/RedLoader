using System.Collections.Generic;
using Nuke.Common.ProjectModel;

public static class ProjectCache
{
    static readonly Dictionary<string, Microsoft.Build.Evaluation.Project> _projects = new();
        
    public static Microsoft.Build.Evaluation.Project GetParsed(this Project nukeProject)
    {
        if (_projects.TryGetValue(nukeProject.Name, out var project))
            return project;

        project = nukeProject.GetMSBuildProject();
        _projects.Add(nukeProject.Name, project);
        return project;
    }
    
    public static string CachedProp(this Project nukeProject, string name)
    {
        var property = nukeProject.GetParsed().GetProperty(name);
        return property?.EvaluatedValue;
    }
}