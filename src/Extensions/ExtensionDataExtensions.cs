using HotChocolate;
using System.Collections.Generic;

namespace AppAny.HotChocolate.FluentValidation
{
	internal static class ExtensionDataExtensions
	{
		public static ObjectFieldValidationOptions GetOrCreateObjectFieldOptions(this ExtensionData extensionData)
		{
			var options = extensionData.TryGetObjectFieldOptions();

			if (options is null)
			{
				options = new ObjectFieldValidationOptions();
				extensionData.Add(ValidationDefaults.ObjectFieldOptionsKey, options);
			}

			return options;
		}

		public static ObjectFieldValidationOptions GetObjectFieldOptions(
			this IReadOnlyDictionary<string, object?> contextData)
		{
			return (ObjectFieldValidationOptions)contextData[ValidationDefaults.ObjectFieldOptionsKey]!;
		}

		public static ObjectFieldValidationOptions? TryGetObjectFieldOptions(
			this IReadOnlyDictionary<string, object?> contextData)
		{
			return contextData.TryGetValue(ValidationDefaults.ObjectFieldOptionsKey, out var data)
				? (ObjectFieldValidationOptions)data!
				: null;
		}

		public static ArgumentValidationOptions GetOrCreateArgumentOptions(this ExtensionData extensionData)
		{
			var options = extensionData.TryGetArgumentOptions();

			if (options is null)
			{
				options = new ArgumentValidationOptions();
				extensionData.Add(ValidationDefaults.ArgumentOptionsKey, options);
			}

			return options;
		}

		public static ArgumentValidationOptions GetArgumentOptions(this IReadOnlyDictionary<string, object?> contextData)
		{
			return (ArgumentValidationOptions)contextData[ValidationDefaults.ArgumentOptionsKey]!;
		}

		public static ArgumentValidationOptions? TryGetArgumentOptions(
			this IReadOnlyDictionary<string, object?> contextData)
		{
			return contextData.TryGetValue(ValidationDefaults.ArgumentOptionsKey, out var data)
				? (ArgumentValidationOptions)data!
				: null;
		}

		public static bool ShouldValidateArgument(this IReadOnlyDictionary<string, object?> contextData)
		{
			return contextData.TryGetArgumentOptions() is not null;
		}

		public static ValidationOptions GetValidationOptions(this IDictionary<string, object?> contextData)
		{
			return (ValidationOptions)contextData[ValidationDefaults.ValidationOptionsKey]!;
		}
	}
}
