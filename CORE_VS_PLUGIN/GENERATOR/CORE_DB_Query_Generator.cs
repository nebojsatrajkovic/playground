using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CORE_VS_PLUGIN.GENERATOR
{
    public static class CORE_DB_Query_Generator
    {
        public static void GenerateQuery(Project project, CORE_DB_QUERY_XML_Template xmlTemplate, Dictionary<string, string> parameters)
        {
            var pathTokens = parameters["File_Path"].Split('\\').ToList();
            pathTokens = pathTokens.Take(pathTokens.Count - 2).ToList();

            var containingFolder = string.Join("\\", pathTokens);

            #region sql

            var sqlDirectory = $"{containingFolder}\\SQL";

            var sqlFilePath = $"{sqlDirectory}\\{xmlTemplate.Meta.MethodClassName}.sql";

            Directory.CreateDirectory(sqlDirectory);

            File.WriteAllText(sqlFilePath, xmlTemplate.SQL);

            CopyToProject(project, sqlFilePath).SetBuildAction(CORE_DB_ProjectItem_BuildAction.EmbeddedResource);

            #endregion sql

            string classTemplate;
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("CORE_VS_PLUGIN.GENERATOR.Templates.Query.DB_QUERY_TEMPLATE.txt")))
            {
                classTemplate = reader.ReadToEnd();
            }

            var projectNameToken = pathTokens.First(x => x.Equals(project.Name));
            var index = pathTokens.IndexOf(projectNameToken);
            var commandLocationTokens = pathTokens.Skip(index).Take(pathTokens.Count - index).ToList();
            commandLocationTokens.Add($"{xmlTemplate.Meta.MethodClassName}.sql");
            var commandLocation = string.Join(".", commandLocationTokens);

            // base data
            {
                classTemplate = classTemplate.Replace("${NAMESPACE}", xmlTemplate.Meta.MethodNamespace);
                classTemplate = classTemplate.Replace("${CLASS_NAME}", xmlTemplate.Meta.MethodClassName);
                classTemplate = classTemplate.Replace("${RESULT_TYPE}", xmlTemplate.Result.ResultClass.Name);
                classTemplate = classTemplate.Replace("${PARAMETER_TYPE}", xmlTemplate.Parameter.ClassName);
                classTemplate = classTemplate.Replace("${COMMAND_LOCATION}", commandLocation);
            }

            // classes

            {
                var customClasses = GenerateClasses(xmlTemplate);

                if (customClasses?.Any() == true)
                {
                    var combinedString = string.Join($"{Environment.NewLine}{Environment.NewLine}", customClasses);

                    classTemplate = classTemplate.Replace("${CUSTOM_CLASSES}", combinedString);
                }
            }

            var classFilePath = $"{containingFolder}\\{xmlTemplate.Meta.MethodClassName}.cs";

            File.WriteAllText(classFilePath, classTemplate);

            CopyToProject(project, classFilePath);
        }

        public static ProjectItem CopyToProject(Project project, string fileLocation)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var projectItem = project.ProjectItems.AddFromFile(fileLocation);

            return projectItem;
        }

        public static ProjectItem SetBuildAction(this ProjectItem item, CORE_DB_ProjectItem_BuildAction buildAction)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var action = string.Empty;

            switch (buildAction)
            {
                case CORE_DB_ProjectItem_BuildAction.EmbeddedResource:
                    action = "EmbeddedResource";
                    break;
            }

            item.Properties.Item("ItemType").Value = action;

            return item;
        }

        public enum CORE_DB_ProjectItem_BuildAction
        {
            EmbeddedResource
        }

        public static List<string> GenerateClasses(CORE_DB_QUERY_XML_Template xmlTemplate)
        {
            var returnValue = new List<string>();

            var rawClassProperties = xmlTemplate.Result.ResultClass.ClassMember.SelectMany(x => x.GetAllClassMembers()).Where(x => !x.IsClass && !x.IsArray).ToList();

            returnValue.AddRange(GenerateClass($"{xmlTemplate.Result.ResultClass.Name}_raw", rawClassProperties, true));

            if (xmlTemplate.Parameter?.ClassMember?.Any() == true)
            {
                returnValue.AddRange(GenerateClass(xmlTemplate.Parameter.ClassName, xmlTemplate.Parameter.ClassMember));
            }

            if (xmlTemplate.Result?.ResultClass?.ClassMember?.Any() == true)
            {
                returnValue.AddRange(GenerateClass(xmlTemplate.Result.ResultClass.Name, xmlTemplate.Result.ResultClass.ClassMember));
            }

            return returnValue;
        }

        public static void RawDataConverterGenerator(CORE_DB_QUERY_XML_Template xmlTemplate)
        {
            var resultClass = xmlTemplate.Result.ResultClass;

            if (!string.IsNullOrEmpty(resultClass.GroupBy))
            {

            }
        }

        public static List<string> GenerateClass(string className, List<CORE_DB_QUERY_XML_ClassMember> members, bool isRaw = false)
        {
            var customClasses = members.SelectMany(x => x.GetAllClassMembers()).Where(x => x.IsClass).ToList();

            var classes = new List<string>
            {
                GetClassString(className, members, isRaw)
            };

            customClasses.ForEach(x => classes.Add(GetClassString(x.Type, x.ClassMembers, isRaw)));

            return classes;
        }

        private static string GetClassString(string className, List<CORE_DB_QUERY_XML_ClassMember> members, bool isRaw = false)
        {
            var templateFile = isRaw ? "CORE_VS_PLUGIN.GENERATOR.Templates.Query.DB_QUERY_RAW_CLASS_TEMPLATE.txt" : "CORE_VS_PLUGIN.GENERATOR.Templates.Query.DB_QUERY_CLASS_TEMPLATE.txt";

            string classTemplate;
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(templateFile)))
            {
                classTemplate = reader.ReadToEnd();
            }

            classTemplate = classTemplate.Replace("${CLASS_NAME}", className);

            var sb = new StringBuilder();

            foreach (var member in members)
            {
                if (member.IsArray)
                {
                    sb.Append($"            public List<{member.Type}> {member.Name} {{ get; set; }}");
                    sb.AppendLine();
                }
                else
                {
                    sb.Append($"            public {member.Type} {member.Name} {{ get; set; }}");
                    sb.AppendLine();
                }
            }

            classTemplate = classTemplate.Replace("${PROPERTIES}", sb.ToString());

            return classTemplate;
        }
    }
}