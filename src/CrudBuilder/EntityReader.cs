using Domain.Carts;
using System.Globalization;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CrudBuilder;
public class EntityReader
{
    public record EntityProperty(string Type, string Name, bool IsNullable);

    // Extracted list of properties from the entity
    public List<EntityProperty> Properties { get; private set; } = new();

    // Generated strings to be used by the Crud builder templates
    public string CreateCommandParameters { get; private set; } = string.Empty; // parameters for create record (exclude Id, timestamps)
    public string UpdateCommandParameters { get; private set; } = string.Empty; // parameters for update record (include Id)
    public string PropertyAssignments { get; private set; } = string.Empty; // mapper/assignment snippet

    public void ReadEntityFile()
    {
        var filePath = Path.Combine(MyPath.AbsoluteBasePath, "Domain", $"{MyPath.EntityName}s", $"{MyPath.EntityName}.cs");

        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string fileContent = reader.ReadToEnd();

                // Parse properties and build helper strings using Roslyn
                ParseProperties(fileContent);
                BuildPropertyStrings();

                Console.WriteLine($"Read entity '{MyPath.EntityName}' and found {Properties.Count} properties.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while reading entity file: {ex.Message}");
        }
    }

    /// <summary>
    /// Parses the class file content and fills the Properties list with detected public properties using Roslyn.
    /// </summary>
    /// <param name="fileContent">Full text content of the entity file.</param>
    private void ParseProperties(string fileContent)
    {
        Properties.Clear();

        var tree = CSharpSyntaxTree.ParseText(fileContent);
        var root = tree.GetCompilationUnitRoot();

        // Find target class by name
        var classDecl = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(c => c.Identifier.Text == MyPath.EntityName);

        if (classDecl == null)
            return;

        // Iterate over property declarations in the class
        var properties = classDecl.Members.OfType<PropertyDeclarationSyntax>();

        foreach (var prop in properties)
        {
            // only public properties
            if (!prop.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                continue;

            // ensure has both get and set accessors
            if (prop.AccessorList == null)
                continue;

            var hasGet = prop.AccessorList.Accessors.Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
            var hasSet = prop.AccessorList.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));
            if (!hasGet || !hasSet)
                continue;

            var typeSyntax = prop.Type;
            bool isNullable = false;
            string typeName;

            // Handle nullable reference types (e.g. string?) and Nullable<T> value types
            if (typeSyntax is NullableTypeSyntax nullableTypeSyntax)
            {
                isNullable = true;
                typeName = nullableTypeSyntax.ElementType.ToString().Trim();
            }
            else
            {
                // For cases like "string?" Roslyn can represent as NullableTypeSyntax or as IdentifierName with trailing trivia.
                // We will check the token text for a trailing '?' for safety.
                var text = typeSyntax.ToString().Trim();
                if (text.Length > 0 && text[^1] == '?')
                {
                    isNullable = true;
                    typeName = text.Substring(0, text.Length - 1).Trim();
                }
                else
                {
                    typeName = text;
                }
            }

            var propName = prop.Identifier.Text;

            Properties.Add(new EntityProperty(typeName, propName, isNullable));
        }
    }

    /// <summary>
    /// Builds commonly used strings for the CRUD templates based on extracted properties.
    /// - CreateCommandParameters: parameters for Create command record (excludes Id, CreatedAt, UpdatedAt)
    /// - UpdateCommandParameters: parameters for Update command record (includes Id first, then other props)
    /// - PropertyAssignments: a snippet that maps command properties to entity properties using assignment (entity.Property = command.Property;}
    /// </summary>
    private void BuildPropertyStrings()
    {
        // Filter out common infrastructure properties
        var skipNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Id", "CreatedAt", "UpdatedAt" };

        var createParams = Properties
            .Where(p => !skipNames.Contains(p.Name))
            .Select(p =>
            {
                var type = p.Type + (p.IsNullable ? "?" : string.Empty);
                return $"{type} {ToCamelCase(p.Name)}";
            })
            .ToList();

        CreateCommandParameters = string.Join(",\n    ", createParams);

        // Update: include Id first, then other props
        var updateParams = new List<string> { "Guid Id" };
        updateParams.AddRange(Properties
            .Where(p => !skipNames.Contains(p.Name))
            .Select(p =>
            {
                var type = p.Type + (p.IsNullable ? "?" : string.Empty);
                return $"{type} {ToCamelCase(p.Name)}";
            }));

        UpdateCommandParameters = string.Join(",\n    ", updateParams);

        // Property assignments (entity.Prop = command.Prop;) excluding Id and timestamps
        var assignments = Properties
            .Where(p => !skipNames.Contains(p.Name))
            .Select(p => $"{MyPath.EntityName.ToLower(CultureInfo.CurrentCulture)}.{p.Name} = command.{p.Name};")
            .ToList();

        PropertyAssignments = string.Join("\n        ", assignments);
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name) || name.Length == 1) return name.ToLower(CultureInfo.CurrentCulture);
        return char.ToLower(name[0], CultureInfo.CurrentCulture) + name.Substring(1);
    }

    public void CreateClassFromType()
    {
        string className = "Domain.Carts.Cart";  //MyPath.EntityName; // named class string

        Type type = Type.GetType(className);

        if (type != null)
        {
            object instance = Activator.CreateInstance(type);
            var instanceaa = instance;
        }
        else
        {
            Console.WriteLine($"Class '{className}' not found.");
        }
    }
}
