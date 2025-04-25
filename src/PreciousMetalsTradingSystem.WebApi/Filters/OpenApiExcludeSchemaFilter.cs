﻿using PreciousMetalsTradingSystem.Application.Common.CustomAttributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace PreciousMetalsTradingSystem.WebApi.Filters
{
    /// <summary>
    /// From:
    /// https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/482
    /// https://stackoverflow.com/questions/41005730/how-to-configure-swashbuckle-to-ignore-property-on-model
    /// </summary>
    public class OpenApiExcludeSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties == null) return;

            var excludedProperties = context.Type.GetProperties()
                .Where(t => t.GetCustomAttribute<OpenApiExcludeAttribute>() != null)
                .Select(t => t.Name)
                .ToList();

            if (excludedProperties.Any())
            {
                schema.Properties = schema.Properties
                    .Where(entry => !excludedProperties.Contains(entry.Key, StringComparer.InvariantCultureIgnoreCase))
                    .ToDictionary(entry => entry.Key, entry => entry.Value);
            }
        }
    }
}
