using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace JabraAssignment.Binding;

// Same rule as DateTimeOffsetJsonConverter, for values from the query string (e.g. ?from=...)
public class DateTimeOffsetModelBinder : IModelBinder
{
	public Task BindModelAsync(ModelBindingContext bindingContext)
	{
		var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
		if (value == ValueProviderResult.None || string.IsNullOrEmpty(value.FirstValue))
		{
			return Task.CompletedTask;
		}

		bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);

		if (DateTimeOffsetJsonConverter.TryParse(value.FirstValue, out var result))
		{
			bindingContext.Result = ModelBindingResult.Success(result);
		}
		else
		{
			bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, DateTimeOffsetJsonConverter.ErrorMessage);
		}

		return Task.CompletedTask;
	}
}

public class DateTimeOffsetModelBinderProvider : IModelBinderProvider
{
	public IModelBinder? GetBinder(ModelBinderProviderContext context) =>
		context.Metadata.UnderlyingOrModelType == typeof(DateTimeOffset)
			? new BinderTypeModelBinder(typeof(DateTimeOffsetModelBinder))
			: null;
}
