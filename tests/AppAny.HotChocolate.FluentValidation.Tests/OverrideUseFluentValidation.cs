using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Validators;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AppAny.HotChocolate.FluentValidation.Tests
{
	public class OverrideUseFluentValidation
	{
		[Fact]
		public async Task Should_UseOnlyDefaultErrorMapper()
		{
			var executor = await TestSetup.CreateRequestExecutor(builder =>
				builder.AddFluentValidation(opt => opt.UseErrorMappers(ValidationDefaults.ErrorMappers.Details))
					.AddMutationType(new TestMutation(field => field.Argument("input",
						arg => arg.Type<NonNullType<TestPersonInputType>>().UseFluentValidation(fv =>
						{
							fv.UseErrorMappers(ValidationDefaults.ErrorMappers.Default);
						}))))
					.Services.AddTransient<IValidator<TestPersonInput>, NotEmptyNameValidator>());

			var result = Assert.IsType<QueryResult>(
				await executor.ExecuteAsync(TestSetup.Mutations.WithEmptyName));

			result.AssertNullResult();

			var error = Assert.Single(result.Errors);

			Assert.Equal(ValidationDefaults.Code, error.Code);
			Assert.Equal(NotEmptyNameValidator.Message, error.Message);

			Assert.Collection(error.Extensions,
				code =>
				{
					Assert.Equal(ValidationDefaults.ExtensionKeys.CodeKey, code.Key);
					Assert.Equal(ValidationDefaults.Code, code.Value);
				});
		}

		[Fact]
		public async Task Should_UseDefaultAndExtensionsErrorMapper()
		{
			var executor = await TestSetup.CreateRequestExecutor(builder =>
				builder.AddFluentValidation(opt => opt.UseErrorMappers(ValidationDefaults.ErrorMappers.Default))
					.AddMutationType(new TestMutation(field => field.Argument("input",
						arg => arg.Type<NonNullType<TestPersonInputType>>().UseFluentValidation(fv =>
						{
							fv.UseErrorMappers(ValidationDefaults.ErrorMappers.Default, ValidationDefaults.ErrorMappers.Details);
						}))))
					.Services.AddTransient<IValidator<TestPersonInput>, NotEmptyNameValidator>());

			var result = Assert.IsType<QueryResult>(
				await executor.ExecuteAsync(TestSetup.Mutations.WithEmptyName));

			result.AssertNullResult();

			var error = Assert.Single(result.Errors);

			Assert.Equal(ValidationDefaults.Code, error.Code);
			Assert.Equal(NotEmptyNameValidator.Message, error.Message);

			Assert.Collection(error.Extensions,
				code =>
				{
					Assert.Equal(ValidationDefaults.ExtensionKeys.CodeKey, code.Key);
					Assert.Equal(ValidationDefaults.Code, code.Value);
				},
				validator =>
				{
					Assert.Equal(ValidationDefaults.ExtensionKeys.ValidatorKey, validator.Key);
					Assert.Equal(nameof(NotEmptyValidator), validator.Value);
				},
				argument =>
				{
					Assert.Equal(ValidationDefaults.ExtensionKeys.ArgumentKey, argument.Key);
					Assert.Equal(new NameString("input"), argument.Value);
				},
				property =>
				{
					Assert.Equal(ValidationDefaults.ExtensionKeys.PropertyKey, property.Key);
					Assert.Equal("Name", property.Value);
				},
				severity =>
				{
					Assert.Equal(ValidationDefaults.ExtensionKeys.SeverityKey, severity.Key);
					Assert.Equal(Severity.Error, severity.Value);
				},
				attemptedValue =>
				{
					Assert.Equal(ValidationDefaults.ExtensionKeys.AttemptedValueKey, attemptedValue.Key);
					Assert.Equal("", attemptedValue.Value);
				});
		}

		[Fact]
		public async Task Should_UseCustomValidator()
		{
			var executor = await TestSetup.CreateRequestExecutor(builder =>
				builder.AddFluentValidation(opt => opt.UseErrorMappers(ValidationDefaults.ErrorMappers.Default))
					.AddMutationType(new TestMutation(field => field.Argument("input",
						arg => arg.Type<NonNullType<TestPersonInputType>>().UseFluentValidation(fv =>
						{
							fv.UseValidator<NotEmptyNameValidator>();
						}))))
					.Services.AddTransient<NotEmptyNameValidator>());

			var result = Assert.IsType<QueryResult>(
				await executor.ExecuteAsync(TestSetup.Mutations.WithEmptyName));

			result.AssertNullResult();

			var error = Assert.Single(result.Errors);

			Assert.Equal(ValidationDefaults.Code, error.Code);
			Assert.Equal(NotEmptyNameValidator.Message, error.Message);

			Assert.Collection(error.Extensions,
				code =>
				{
					Assert.Equal(ValidationDefaults.ExtensionKeys.CodeKey, code.Key);
					Assert.Equal(ValidationDefaults.Code, code.Value);
				});
		}

		[Fact]
		public async Task Should_UseCustomValidatorProvider()
		{
			var executor = await TestSetup.CreateRequestExecutor(builder =>
				builder.AddFluentValidation(opt => opt.UseErrorMappers(ValidationDefaults.ErrorMappers.Default))
					.AddMutationType(new TestMutation(field => field.Argument("input",
						arg => arg.Type<NonNullType<TestPersonInputType>>().UseFluentValidation(fv =>
						{
							fv.UseInputValidatorProviders(context =>
								ValidationDefaults.InputValidators.FromValidator(
									context.MiddlewareContext.Services.GetRequiredService<NotEmptyNameValidator>()));
						}))))
					.Services.AddTransient<NotEmptyNameValidator>());

			var result = Assert.IsType<QueryResult>(
				await executor.ExecuteAsync(TestSetup.Mutations.WithEmptyName));

			result.AssertNullResult();

			var error = Assert.Single(result.Errors);

			Assert.Equal(ValidationDefaults.Code, error.Code);
			Assert.Equal(NotEmptyNameValidator.Message, error.Message);

			Assert.Collection(error.Extensions,
				code =>
				{
					Assert.Equal(ValidationDefaults.ExtensionKeys.CodeKey, code.Key);
					Assert.Equal(ValidationDefaults.Code, code.Value);
				});
		}

		[Fact]
		public async Task Should_UseMultipleCustomValidators()
		{
			var executor = await TestSetup.CreateRequestExecutor(builder =>
				builder.AddFluentValidation(opt => opt.UseErrorMappers(ValidationDefaults.ErrorMappers.Default))
					.AddMutationType(new TestMutation(field => field.Argument("input",
						arg => arg.Type<NonNullType<TestPersonInputType>>().UseFluentValidation(fv =>
						{
							fv.UseValidator<NotEmptyNameValidator>()
								.UseValidator<NotEmptyAddressValidator>();
						}))))
					.Services.AddTransient<NotEmptyNameValidator>().AddTransient<NotEmptyAddressValidator>());

			var result = Assert.IsType<QueryResult>(
				await executor.ExecuteAsync(TestSetup.Mutations.WithEmptyNameAndAddress));

			result.AssertNullResult();

			Assert.Collection(result.Errors,
				name =>
				{
					Assert.Equal(ValidationDefaults.Code, name.Code);
					Assert.Equal(NotEmptyNameValidator.Message, name.Message);

					Assert.Collection(name.Extensions,
						code =>
						{
							Assert.Equal(ValidationDefaults.ExtensionKeys.CodeKey, code.Key);
							Assert.Equal(ValidationDefaults.Code, code.Value);
						});
				},
				address =>
				{
					Assert.Equal(ValidationDefaults.Code, address.Code);
					Assert.Equal(NotEmptyAddressValidator.Message, address.Message);

					Assert.Collection(address.Extensions,
						code =>
						{
							Assert.Equal(ValidationDefaults.ExtensionKeys.CodeKey, code.Key);
							Assert.Equal(ValidationDefaults.Code, code.Value);
						});
				});
		}

		[Fact]
		public async Task Should_UseMultipleCustomValidators_ExplicitInputType()
		{
			var executor = await TestSetup.CreateRequestExecutor(builder =>
				builder.AddFluentValidation(opt => opt.UseErrorMappers(ValidationDefaults.ErrorMappers.Default))
					.AddMutationType(new TestMutation(field => field.Argument("input",
						arg => arg.Type<NonNullType<TestPersonInputType>>().UseFluentValidation(fv =>
						{
							fv.UseValidator<TestPersonInput, NotEmptyNameValidator>()
								.UseValidator<TestPersonInput, NotEmptyAddressValidator>();
						}))))
					.Services.AddTransient<NotEmptyNameValidator>().AddTransient<NotEmptyAddressValidator>());

			var result = Assert.IsType<QueryResult>(
				await executor.ExecuteAsync(TestSetup.Mutations.WithEmptyNameAndAddress));

			result.AssertNullResult();

			Assert.Collection(result.Errors,
				name =>
				{
					Assert.Equal(ValidationDefaults.Code, name.Code);
					Assert.Equal(NotEmptyNameValidator.Message, name.Message);

					Assert.Collection(name.Extensions,
						code =>
						{
							Assert.Equal(ValidationDefaults.ExtensionKeys.CodeKey, code.Key);
							Assert.Equal(ValidationDefaults.Code, code.Value);
						});
				},
				address =>
				{
					Assert.Equal(ValidationDefaults.Code, address.Code);
					Assert.Equal(NotEmptyAddressValidator.Message, address.Message);

					Assert.Collection(address.Extensions,
						code =>
						{
							Assert.Equal(ValidationDefaults.ExtensionKeys.CodeKey, code.Key);
							Assert.Equal(ValidationDefaults.Code, code.Value);
						});
				});
		}

		[Fact]
		public async Task Should_UseMultipleCustomValidators_WithValidationStrategy_IgnoreAddress()
		{
			var executor = await TestSetup.CreateRequestExecutor(builder =>
				builder.AddFluentValidation(opt => opt.UseErrorMappers(ValidationDefaults.ErrorMappers.Default))
					.AddMutationType(new TestMutation(field => field.Argument("input",
						arg => arg.Type<NonNullType<TestPersonInputType>>().UseFluentValidation(fv =>
						{
							fv.UseValidator<NotEmptyNameValidator>()
								.UseValidator<TestPersonInput, NotEmptyAddressValidator>(strategy =>
									// Validates address, but includes only name
									strategy.IncludeProperties(input => input.Name));
						}))))
					.Services.AddTransient<NotEmptyNameValidator>().AddTransient<NotEmptyAddressValidator>());

			var result = Assert.IsType<QueryResult>(
				await executor.ExecuteAsync(TestSetup.Mutations.WithEmptyNameAndAddress));

			result.AssertNullResult();

			Assert.Collection(result.Errors,
				name =>
				{
					Assert.Equal(ValidationDefaults.Code, name.Code);
					Assert.Equal(NotEmptyNameValidator.Message, name.Message);

					Assert.Collection(name.Extensions,
						code =>
						{
							Assert.Equal(ValidationDefaults.ExtensionKeys.CodeKey, code.Key);
							Assert.Equal(ValidationDefaults.Code, code.Value);
						});
				});
		}

		[Fact]
		public async Task Should_UseMultipleCustomValidators_SameProperty()
		{
			var executor = await TestSetup.CreateRequestExecutor(builder =>
				builder.AddFluentValidation(opt => opt
						.UseErrorMappers(ValidationDefaults.ErrorMappers.Default))
					.AddMutationType(new TestMutation(field => field.Argument("input",
						arg => arg.Type<NonNullType<TestPersonInputType>>().UseFluentValidation(fv =>
						{
							fv.UseValidator<NotEmptyNameValidator>()
								.UseValidator<NotEmptyNameValidator>();
						}))))
					.Services.AddTransient<NotEmptyNameValidator>().AddTransient<NotEmptyAddressValidator>());

			var result = Assert.IsType<QueryResult>(
				await executor.ExecuteAsync(TestSetup.Mutations.WithEmptyName));

			result.AssertNullResult();

			Assert.Collection(result.Errors,
				name =>
				{
					Assert.Equal(ValidationDefaults.Code, name.Code);
					Assert.Equal(NotEmptyNameValidator.Message, name.Message);

					Assert.Collection(name.Extensions,
						code =>
						{
							Assert.Equal(ValidationDefaults.ExtensionKeys.CodeKey, code.Key);
							Assert.Equal(ValidationDefaults.Code, code.Value);
						});
				},
				name =>
				{
					Assert.Equal(ValidationDefaults.Code, name.Code);
					Assert.Equal(NotEmptyNameValidator.Message, name.Message);

					Assert.Collection(name.Extensions,
						code =>
						{
							Assert.Equal(ValidationDefaults.ExtensionKeys.CodeKey, code.Key);
							Assert.Equal(ValidationDefaults.Code, code.Value);
						});
				});
		}

		[Fact]
		public async Task Should_UseSingleCustomValidator_DoubleProperty()
		{
			var executor = await TestSetup.CreateRequestExecutor(builder =>
				builder.AddFluentValidation(opt => opt.UseErrorMappers(ValidationDefaults.ErrorMappers.Default))
					.AddMutationType(new TestMutation(field => field.Argument("input",
						arg => arg.Type<NonNullType<TestPersonInputType>>().UseFluentValidation(fv =>
						{
							fv.UseValidator<DoubleNotEmptyNameValidator>();
						}))))
					.Services.AddTransient<DoubleNotEmptyNameValidator>());

			var result = Assert.IsType<QueryResult>(
				await executor.ExecuteAsync(TestSetup.Mutations.WithEmptyName));

			result.AssertNullResult();

			Assert.Collection(result.Errors,
				name =>
				{
					Assert.Equal(ValidationDefaults.Code, name.Code);
					Assert.Equal(DoubleNotEmptyNameValidator.Message1, name.Message);

					Assert.Collection(name.Extensions,
						code =>
						{
							Assert.Equal(ValidationDefaults.ExtensionKeys.CodeKey, code.Key);
							Assert.Equal(ValidationDefaults.Code, code.Value);
						});
				},
				name =>
				{
					Assert.Equal(ValidationDefaults.Code, name.Code);
					Assert.Equal(DoubleNotEmptyNameValidator.Message2, name.Message);

					Assert.Collection(name.Extensions,
						code =>
						{
							Assert.Equal(ValidationDefaults.ExtensionKeys.CodeKey, code.Key);
							Assert.Equal(ValidationDefaults.Code, code.Value);
						});
				});
		}

		[Fact]
		public async Task Should_Execute_SkipValidation_WithCustomValidator()
		{
			var executor = await TestSetup.CreateRequestExecutor(builder =>
				builder.AddFluentValidation()
					.AddMutationType(new TestMutation(field => field.Argument("input",
						arg => arg.Type<NonNullType<TestPersonInputType>>().UseFluentValidation(opt =>
						{
							opt.SkipValidation().UseValidator<NotEmptyNameValidator>();
						}))))
					.Services.AddTransient<NotEmptyNameValidator>());

			var result = Assert.IsType<QueryResult>(
				await executor.ExecuteAsync(TestSetup.Mutations.WithEmptyName));

			var (key, value) = Assert.Single(result.Data);

			Assert.Equal("test", key);
			Assert.Equal("test", value);

			Assert.Null(result.Errors);
		}

		[Fact]
		public async Task Should_Execute_SkipValidation_WithCustomValidatorProvider()
		{
			var executor = await TestSetup.CreateRequestExecutor(builder =>
				builder.AddFluentValidation()
					.AddMutationType(new TestMutation(field => field.Argument("input",
						arg => arg.Type<NonNullType<TestPersonInputType>>().UseFluentValidation(opt =>
						{
							opt.SkipValidation().UseInputValidatorProviders(_ =>
								ValidationDefaults.InputValidators.FromValidator(new NotEmptyNameValidator()));
						}))))
					.Services.AddTransient<NotEmptyNameValidator>());

			var result = Assert.IsType<QueryResult>(
				await executor.ExecuteAsync(TestSetup.Mutations.WithEmptyName));

			var (key, value) = Assert.Single(result.Data);

			Assert.Equal("test", key);
			Assert.Equal("test", value);

			Assert.Null(result.Errors);
		}
	}
}
