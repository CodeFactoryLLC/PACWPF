using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory;
using CodeFactory.DotNet.CSharp;
using CodeFactory.VisualStudio;

namespace CodeFactoryExtensions.Common.Automation
{
    /// <summary>
    /// Extension method class that supports the <see cref="VsCSharpSource"/> model.
    /// </summary>
    public static class VsCSharpSourceExtensions
    {
        /// <summary>
        /// Will get the list of project references for the project that hosts this source code.
        /// </summary>
        /// <param name="source">The source code to get the references from.</param>
        /// <returns>The references or an empty list.</returns>
        public static async Task<IReadOnlyList<VsReference>> GetHostProjectReferencesAsync(this VsCSharpSource source)
        {
            IReadOnlyList<VsReference> result = ImmutableList<VsReference>.Empty;
            if (source == null) return result;

            if (!source.IsLoaded) return result;

            var project = await source.GetHostingProjectAsync();

            if (project == null) return result;

            result = await project.GetProjectReferencesAsync();

            return result;
        }

        /// <summary>
        /// Loads the parent of the source code document.
        /// </summary>
        /// <param name="source">Target source code.</param>
        /// <param name="noDocument">Flag that will determine if the parent is a document to keep searching for parent.</param>
        /// <returns>The parent model.</returns>
        public static async Task<VsModel> GetParentAsync(this VsCSharpSource source, bool noDocument = true)
        {
            if (source == null) return null;
            if (!source.IsLoaded) return null;
            var document = await source.LoadDocumentModelAsync();
            VsCSharpSource sourceDocument = null;
            VsModel parent = null;

            if(!noDocument) return await document.GetParentAsync();

            bool found = false;
            while (!found)
            {
                if (sourceDocument != null) parent = await sourceDocument.GetParentAsync(true);
                else parent = await document.GetParentAsync();

                if(parent == null) break;

                switch (parent.ModelType)
                {
                    case VisualStudioModelType.Solution:
                        found = true;
                        break;
                    case VisualStudioModelType.SolutionFolder:
                        found = true;
                        break;
                    case VisualStudioModelType.Project:
                        found = true;
                        break;
                    case VisualStudioModelType.ProjectFolder:
                        found = true;
                        break;

                    case VisualStudioModelType.Document:

                        document = parent as VsDocument;
                        if(document == null) throw new CodeFactoryException("Cannot load the document information, cannot get the parent model.");
                        sourceDocument = null;
                        
                        break;

                    case VisualStudioModelType.CSharpSource:

                        sourceDocument = parent as VsCSharpSource;
                        if (sourceDocument == null) throw new CodeFactoryException("Cannot load the document information, cannot get the parent model.");
                        document = null;
                        break;

                    default:
                        found = false;
                        break;
                }
                
            }

            return parent;
        }

        /// <summary>
        /// Gets the parent for the current C# source code model.
        /// </summary>
        /// <param name="source">Source model to load.</param>
        /// <returns>Tuple containing the location and target model. If the model is null then the parent is outside of a project implementation, or could not be loaded.</returns>
        public static async Task<(bool IsProject, VsModel ParentModel)> GetCSharpSourceDocumentParentAsync(this VsCSharpSource source)
        {
            if (source == null) return (false,null);

            var parent = await source.GetParentAsync();

            if(parent.ModelType == VisualStudioModelType.Project) return (true,parent);

            return parent.ModelType == VisualStudioModelType.ProjectFolder ? (false, parent) : (false, null);
        }

        /// <summary>
        /// Adds a new C# document to the parent project or folder of the current c# document.
        /// </summary>
        /// <param name="source">C# source document.</param>
        /// <param name="sourceCode">The source code to be added to the new code file.</param>
        /// <param name="targetFileName">The target file name of the new document.</param>
        /// <returns></returns>
        public static async Task<CsSource> AddCSharpCodeFileToParentAsync(this VsCSharpSource source, string sourceCode,
            string targetFileName)
        {
            if (source == null) throw new CodeFactoryException("No visual studio c# source was provided cannot add C# code file.");
            if(string.IsNullOrEmpty(targetFileName)) throw new CodeFactoryException("No filename was provided cannot add the C# code file.");

            var parent = await source.GetCSharpSourceDocumentParentAsync();

            if(parent.ParentModel == null) throw new CodeFactoryException("No project or project folder was found, cannot add the C# code file.");


            VsDocument document = null;

            if (parent.IsProject)
            {
                var project = parent.ParentModel as VsProject;

                if(project == null) throw new CodeFactoryException("Could load the project information, cannot add the C# code file.");

                document = await project.AddDocumentAsync(targetFileName, sourceCode);
            }
            else
            {
                var projectFolder = parent.ParentModel as VsProjectFolder;

                if (projectFolder == null) throw new CodeFactoryException("Could load the project folder information, cannot add the C# code file.");

                document = await projectFolder.AddDocumentAsync(targetFileName, sourceCode);
            }

            var csDocument = await document.GetCSharpSourceModelAsync();

            return csDocument;
        }

        /// <summary>
        /// Extension method designed to look in the parent project folder, or project of the current source document and find a target code file in that location.
        /// </summary>
        /// <param name="source">The source model.</param>
        /// <param name="targetFilePath">The fully qualified path to the target model.</param>
        /// <returns>The loaded source or null if the source could not be loaded.</returns>
        public static async Task<CsSource> GetCsSourceDocumentFromParent(this VsCSharpSource source, string targetFilePath)
        {
            if (source == null) return null;
            if (string.IsNullOrEmpty(targetFilePath)) return null;

            var parentData = await source.GetCSharpSourceDocumentParentAsync();

            var parent = parentData.ParentModel;

            if(parent == null) throw new CodeFactoryException("Source document is not hosted in a project or project folder.");

            VsCSharpSource sourceDocument = null;
            if (!parentData.IsProject)
            {

                var parentFolder = parent as VsProjectFolder;

                if (parentFolder == null) throw new CodeFactoryException("Cannot access the parent of the source code document");

                var children = await parentFolder.GetChildrenAsync(false, true);

                sourceDocument = children.Where(c => c.ModelType == VisualStudioModelType.CSharpSource)
                    .Cast<VsCSharpSource>()
                    .FirstOrDefault(s => s.SourceCode.SourceDocument == targetFilePath);
            }
            else
            {
                var parentProject = parent as VsProject;

                if (parentProject == null) throw new CodeFactoryException("Cannot access the parent of the source code document, cannot update subscription");

                var children = await parentProject.GetChildrenAsync(false, true);

                sourceDocument = children.Where(c => c.ModelType == VisualStudioModelType.CSharpSource)
                    .Cast<VsCSharpSource>()
                    .FirstOrDefault(s => s.SourceCode.SourceDocument == targetFilePath); ;
            }

            return sourceDocument?.SourceCode;

        }
    }
}
