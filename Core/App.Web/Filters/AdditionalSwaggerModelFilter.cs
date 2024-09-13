using Lens.Core.Lib.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Lens.Core.App.Web.Filters
{
    /// <summary>
    /// Adds extra models in the Swagger documentation that are not referenced in the controllers. This way you can generate your frontend classes
    /// and have all the models at your disposal, so that you can generate all necessary models for the frontend (using nswag)
    /// </summary>
    internal class AdditionalSwaggerModelFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var excludes = new[] { "Microsoft", "System"};
            var types = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !excludes.Contains(a.FullName?[..a.FullName.IndexOf('.')]))
                    .SelectMany(a => a.GetTypes()
                    .Where(type => type.IsDefined(typeof(SwaggerAdditionalModelAttribute), false))
                    .Select(type => new { Type = type, Attribute = type.GetCustomAttribute<SwaggerAdditionalModelAttribute>() })
                    .Where(x => x?.Attribute?.GroupNames.Contains(context.DocumentName) ?? false));


            foreach (var modelType in types)
            {
                var schema = new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>()
                };

                var properties = modelType.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var property in properties)
                {
                    var propertySchema = context.SchemaGenerator.GenerateSchema(property.PropertyType, context.SchemaRepository);
                    schema.Properties[property.Name] = propertySchema;
                }

                swaggerDoc.Components.Schemas[modelType.Type.Name] = schema;
            }
        }
    }
}
