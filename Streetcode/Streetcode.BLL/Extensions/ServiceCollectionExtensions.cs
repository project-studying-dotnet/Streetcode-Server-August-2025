using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Streetcode.BLL.MediatR;
using Streetcode.BLL.Validators;

namespace Streetcode.BLL.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddValidationPipeline(this IServiceCollection services)
        {
            // Register all validators from the current assembly
            services.AddValidatorsFromAssemblyContaining<NewsDTOValidator>();

            // Register the validation pipeline behavior
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));

            return services;
        }
    }
}
