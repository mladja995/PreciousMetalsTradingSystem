using PreciousMetalsTradingSystem.Application.Common.CustomAttributes;
using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Reflection;

namespace PreciousMetalsTradingSystem.WebApi.AppConfigSetup
{
    public class OptionsSetup<T> : IConfigureOptions<T> where T : class
    {
        private readonly IConfiguration _configuration;
        private readonly string _sectionName;

        public OptionsSetup(IConfiguration configuration, string sectionName)
        {
            _configuration = configuration;
            _sectionName = sectionName;
        }

        public void Configure(T options)
        {
            var section = _configuration.GetSection(_sectionName);

            if (!section.Exists())
            {
                throw new ConfigurationException($"Section '{_sectionName}' does not exist in the configuration.");
            }

            ValidateRequiredProperties(typeof(T), section, options);

            section.Bind(options);
        }

        private void ValidateRequiredProperties(Type type, IConfigurationSection section, object options)
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var optionalAttribute = property.GetCustomAttribute<OptionalAttribute>();

                if (optionalAttribute == null)
                {
                    var key = section.GetSection(property.Name);

                    if (!key.Exists() /*|| string.IsNullOrWhiteSpace(key.Value)*/)
                    {
                        if (typeof(IList).IsAssignableFrom(property.PropertyType))
                        {
                            var elementType = property.PropertyType.GetGenericArguments().FirstOrDefault();

                            if (elementType == null)
                            {
                                throw new ConfigurationException($"{_sectionName} - List property '{property.Name}' has an undefined element type.");
                            }

                            var sectionChildren = key.GetChildren();

                            if (!sectionChildren.Any())
                            {
                                throw new ConfigurationException($"{_sectionName} - List property '{property.Name}' is required but not provided or empty in the configuration.");
                            }

                            foreach (var childSection in sectionChildren)
                            {
                                var item = childSection.Get(elementType);
                                if (item == null)
                                {
                                    throw new ConfigurationException($"{_sectionName} - Item in list property '{property.Name}' is invalid or null.");
                                }

                                if (!IsSimpleType(elementType))
                                {
                                    ValidateRequiredProperties(elementType, childSection, item);
                                }
                            }
                        }
                        else
                        {
                            throw new ConfigurationException($"{_sectionName} - Property '{property.Name}' is required but not provided in the configuration.");
                        }
                    }
                }
            }
        }

        private bool IsSimpleType(Type type)
        {
            return type.IsPrimitive || type.IsEnum || type.Equals(typeof(string)) || type.Equals(typeof(decimal));
        }
    }
}
