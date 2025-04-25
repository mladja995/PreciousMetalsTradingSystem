﻿using FluentValidation;
using MediatR;
using ValidationException = PreciousMetalsTradingSystem.Application.Common.Exceptions.ValidationException;

namespace PreciousMetalsTradingSystem.Application.Common.Behaviors
{
    public class ValidationPipelineBehavior<TRequest, TResponse> 
        : IPipelineBehavior<TRequest, TResponse> 
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(
                    _validators.Select(v =>
                        v.ValidateAsync(context, cancellationToken)));

                var failures = validationResults
                    .Where(r => r.Errors.Count != 0)
                    .SelectMany(r => r.Errors)
                    .ToList();

                if (failures.Count != 0)
                {
                    throw new ValidationException(failures);
                }
            }
            return await next();
        }
    }
}
