using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;

namespace AppAny.HotChocolate.FluentValidation
{
	/// <summary>
	/// Used to combine <see cref="IValidator{T}"/> and <see cref="ValidationStrategy{T}"/>.
	/// To create new <see cref="InputValidator"/> use <see cref="FromValidator"/> or <see cref="FromValidatorWithStrategy{TInput}"/>
	/// </summary>
	public interface InputValidator
	{
		Task<ValidationResult> ValidateAsync(object argument, CancellationToken cancellationToken);

		/// <summary>
		/// Creates new <see cref="InputValidator"/> with default <see cref="ValidationStrategy{T}"/>
		/// </summary>
		public static InputValidator FromValidator(IValidator validator)
		{
			return FromValidatorWithStrategy<object>(validator, ValidationDefaults.ValidationStrategies.Default);
		}

		/// <summary>
		/// Creates new <see cref="InputValidator"/> with custom <see cref="ValidationStrategy{T}"/>
		/// </summary>
		public static InputValidator FromValidatorWithStrategy<TInput>(
			IValidator validator,
			Action<ValidationStrategy<TInput>> validationStrategy)
		{
			return new InputValidator<TInput>(validator, validationStrategy);
		}
	}

	internal sealed class InputValidator<TInput> : InputValidator
	{
		private readonly IValidator validator;
		private readonly Action<ValidationStrategy<TInput>> validationStrategy;

		public InputValidator(IValidator validator, Action<ValidationStrategy<TInput>> validationStrategy)
		{
			this.validator = validator;
			this.validationStrategy = validationStrategy;
		}

		public Task<ValidationResult> ValidateAsync(object argument, CancellationToken cancellationToken)
		{
			var validationContext = ValidationContext<TInput>.CreateWithOptions(
				(TInput)argument,
				validationStrategy);

			return validator.ValidateAsync(validationContext, cancellationToken);
		}
	}
}
