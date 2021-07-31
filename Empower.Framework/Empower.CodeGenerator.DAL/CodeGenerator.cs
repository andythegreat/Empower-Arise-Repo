using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empower.CodeGenerator.DAL
{

    [Generator]
    public class RepositoryGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Not needed for this sample
        }
        public void Execute(GeneratorExecutionContext context)
        {
            // begin creating the source we'll inject into the users compilation
            var sourceBuilder = new StringBuilder(
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Empower.DAL
{
    public interface IRepository
    {
        Empower.Model.IEmpowerContext Context { get; }
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        void Detach(object entity);
        //IQueryable Set(Type type);
        IQueryable<T> Set<T>() where T : class;
        void LoadEntity<T, U>(T entity, Expression<Func<T, U>> selector) where T : class where U : class;
        void LoadCollection<T, U>(T entity, Expression<Func<T, ICollection<U>>> selector) where T : class where U : class;
        void Save(string overrideUser = null, IDictionary<string, object> overrideRouteDataValues = null);
        void NoAuditSave(bool async);
        void ExecuteSqlCommand(string sql, params object[] parameters);
        void Update<T>(T entity, int id) where T : class;

");

            var additionalText = context.AdditionalFiles.Where(af => af.Path.Contains("EmpowerContext.cs")).SingleOrDefault();

            var fileEmpowerContext = additionalText.Path;

            var repositoryInterfaceDefinition = "";

            Task.Run(async () =>
            {
                repositoryInterfaceDefinition = await GetRepositoryInterfaceDefinition(fileEmpowerContext);
            })
            .GetAwaiter()
            .GetResult();

            //System.Diagnostics.Debugger.Launch();

            sourceBuilder.Append(repositoryInterfaceDefinition);
            sourceBuilder.Append("\t}");
            sourceBuilder.Append(@"

    public partial class Repository : IRepository
    {
");

            var repositoryInterfaceImplementation = "";

            Task.Run(async () =>
            {
                repositoryInterfaceImplementation = await GetRepositoryInterfaceImplementation(fileEmpowerContext);
            })
            .GetAwaiter()
            .GetResult();

            //System.Diagnostics.Debugger.Launch();

            sourceBuilder.Append(repositoryInterfaceImplementation);
            sourceBuilder.Append("\t}");
            sourceBuilder.Append(@"
}");
            // inject the created source into the users compilation
            context.AddSource("Repository.g", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        static async Task<string> GetRepositoryInterfaceDefinition(string filePath)
        {
            //System.Diagnostics.Debugger.Launch();

            //var file = File.ReadAllText(@"C:\_Projects\Arise\Arise-DC\Empower.Framework\Empower.Model\EmpowerContext.cs");
            var file = File.ReadAllText(filePath);

            // Parse the code into a SyntaxTree.
            var tree = CSharpSyntaxTree.ParseText(file);

            // Get the root CompilationUnitSyntax.
            var root = await tree.GetRootAsync().ConfigureAwait(false) as CompilationUnitSyntax;

            // Get the namespace declaration.
            var empowerNamespace = root.Members.Single(m => m is NamespaceDeclarationSyntax) as NamespaceDeclarationSyntax;

            // Get all class declarations inside the namespace.
            var empowerContext = empowerNamespace.Members.Where(m => m is ClassDeclarationSyntax).FirstOrDefault() as ClassDeclarationSyntax;

            var propertyClasses = empowerContext.Members.Where(m => m is PropertyDeclarationSyntax);

            var sbIqueryable = new StringBuilder();

            //System.Diagnostics.Debugger.Launch();

            foreach (var property in propertyClasses)
            {
                var tmpProperty = property as PropertyDeclarationSyntax;

                var tmpType = tmpProperty.Type as GenericNameSyntax;

                var args = tmpType.TypeArgumentList.Arguments.FirstOrDefault().ToString();

                sbIqueryable.AppendLine($"\t\tIQueryable<Model.{args}> {tmpProperty.Identifier.ValueText} {{ get; }}");
            }

            return sbIqueryable.ToString();
        }

        static async Task<string> GetRepositoryInterfaceImplementation(string filePath)
        {
            //System.Diagnostics.Debugger.Launch();

            //var file = File.ReadAllText(@"C:\_Projects\Arise\Arise-DC\Empower.Framework\Empower.Model\EmpowerContext.cs");
            var file = File.ReadAllText(filePath);

            // Parse the code into a SyntaxTree.
            var tree = CSharpSyntaxTree.ParseText(file);

            // Get the root CompilationUnitSyntax.
            var root = await tree.GetRootAsync().ConfigureAwait(false) as CompilationUnitSyntax;

            // Get the namespace declaration.
            var empowerNamespace = root.Members.Single(m => m is NamespaceDeclarationSyntax) as NamespaceDeclarationSyntax;

            // Get all class declarations inside the namespace.
            var empowerContext = empowerNamespace.Members.Where(m => m is ClassDeclarationSyntax).FirstOrDefault() as ClassDeclarationSyntax;

            var propertyClasses = empowerContext.Members.Where(m => m is PropertyDeclarationSyntax);

            var sbIqueryable = new StringBuilder();

            foreach (var property in propertyClasses)
            {
                var tmpProperty = property as PropertyDeclarationSyntax;

                var tmpType = tmpProperty.Type as GenericNameSyntax;

                var args = tmpType.TypeArgumentList.Arguments.FirstOrDefault().ToString();

                sbIqueryable.AppendLine($"\t\tpublic IQueryable<Model.{args}> {tmpProperty.Identifier.ValueText} {{ get {{ return Context.{tmpProperty.Identifier.ValueText}; }} }}");
            }

            return sbIqueryable.ToString();
        }
    }
}
