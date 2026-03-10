using System.ComponentModel.DataAnnotations;

namespace EasyPicPay.Web.Validators;

public class NotSameAsAttribute : ValidationAttribute
{
    private readonly string _otherProperty;

    public NotSameAsAttribute(string otherProperty)
    {
        _otherProperty = otherProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var otherPropertyInfo = validationContext.ObjectType.GetProperty(_otherProperty);
        
        if (otherPropertyInfo == null)
            return new ValidationResult($"Propriedade {_otherProperty} não encontrada");

        var otherValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance);

        if (Equals(value, otherValue))
            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} não pode ser igual a {_otherProperty}");

        return ValidationResult.Success;
    }
}