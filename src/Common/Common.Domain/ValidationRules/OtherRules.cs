using FluentValidation;

namespace Common.Domain.ValidationRules;

public static class OtherRules
{
    public static IRuleBuilderOptions<T, TProperty> NotDefault<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder
    )
    {
#pragma warning disable CS8619
        return ruleBuilder
            .NotEqual(default(TProperty))
            .WithMessage(
                $"{{PropertyName}} must be not equal to default value of {typeof(TProperty).Name}"
            );
#pragma warning restore CS8619
    }
}
