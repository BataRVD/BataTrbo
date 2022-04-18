using System;
using System.ComponentModel;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

public class CommaDelimitedArrayModelBinder : IModelBinder
{
    /// <summary>
    /// Borrowed from https://stackoverflow.com/a/19107738
    /// </summary>
    /// <param name="actionContext"></param>
    /// <param name="bindingContext"></param>
    /// <returns></returns>
    public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
    {
        var key = bindingContext.ModelName;
        var val = bindingContext.ValueProvider.GetValue(key);
        if (val != null)
        {
            var s = val.AttemptedValue;
            if (s != null)
            {
                var elementType = bindingContext.ModelType.GetElementType();
                var converter = TypeDescriptor.GetConverter(elementType);
                var values = Array.ConvertAll(s.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries),
                    x => { return converter.ConvertFromString(x != null ? x.Trim() : x); });

                var typedValues = Array.CreateInstance(elementType, values.Length);

                values.CopyTo(typedValues, 0);

                bindingContext.Model = typedValues;
            }
            else
            {
                // change this line to null if you prefer nulls to empty arrays 
                bindingContext.Model = Array.CreateInstance(bindingContext.ModelType.GetElementType(), 0);
            }
            return true;
        }
        return false;
    }
}