﻿using HotChocolate;
using HotChocolate.Resolvers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace AppAny.HotChocolate.FluentValidation
{
	public static class ValidationFieldMiddleware
	{
		public static FieldDelegate Use(FieldDelegate next)
		{
			return async middlewareContext =>
			{
				var passedArguments = middlewareContext.Selection.SyntaxNode.Arguments;

				if (passedArguments is { Count: > 0 })
				{
					var inputFields = middlewareContext.Field.Arguments;

					var options = middlewareContext.Schema.Services!
						.GetRequiredService<IOptions<InputValidationOptions>>().Value;

					// TODO: Validate only passed arguments
					for (var fieldIndex = 0; fieldIndex < inputFields.Count; fieldIndex++)
					{
						var inputField = inputFields[fieldIndex];

						var inputFieldOptions = inputField.ContextData.TryGetInputFieldOptions();

						if (inputFieldOptions is null)
						{
							continue;
						}

						var skipValidation = inputFieldOptions.SkipValidation ?? options.SkipValidation;

						if (await skipValidation.Invoke(new SkipValidationContext(middlewareContext, inputField)))
						{
							continue;
						}

						var argument = middlewareContext.ArgumentValue<object?>(inputField.Name);

						if (argument is null)
						{
							continue;
						}

						var errorMappers = inputFieldOptions.ErrorMappers ?? options.ErrorMappers;
						var inputValidatorProviders = inputFieldOptions.InputValidatorProviders ?? options.InputValidatorProviders;

						for (var providerIndex = 0; providerIndex < inputValidatorProviders.Count; providerIndex++)
						{
							var inputValidatorProvider = inputValidatorProviders[providerIndex];

							var inputValidator = inputValidatorProvider.Invoke(new InputValidatorProviderContext(
								middlewareContext,
								inputField));

							var validationResult = await inputValidator.Invoke(argument, middlewareContext.RequestAborted);

							if (validationResult?.IsValid is null or true)
							{
								continue;
							}

							for (var errorIndex = 0; errorIndex < validationResult.Errors.Count; errorIndex++)
							{
								var validationFailure = validationResult.Errors[errorIndex];

								var errorBuilder = ErrorBuilder.New();

								for (var errorMapperIndex = 0; errorMapperIndex < errorMappers.Count; errorMapperIndex++)
								{
									var errorMapper = errorMappers[errorMapperIndex];

									errorMapper.Invoke(errorBuilder, new ErrorMappingContext(
										middlewareContext,
										inputField,
										validationResult,
										validationFailure));
								}

								middlewareContext.ReportError(errorBuilder.Build());
							}
						}
					}
				}

				if (middlewareContext.HasErrors is false)
				{
					await next(middlewareContext).ConfigureAwait(false);
				}
			};
		}
	}
}
