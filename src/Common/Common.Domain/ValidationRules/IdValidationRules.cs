using FluentValidation;

namespace Common.Domain.ValidationRules;

public static class IdValidationRules
{
    public static IRuleBuilderOptions<T, int> IsValidId<T>(this IRuleBuilder<T, int> ruleBuilder) =>
        ruleBuilder.GreaterThan(0);

    public static IRuleBuilderOptions<T, int?> IsValidId<T>(
        this IRuleBuilder<T, int?> ruleBuilder
    ) => ruleBuilder.GreaterThan(0);

    public static IRuleBuilderOptions<T, long> IsValidId<T>(
        this IRuleBuilder<T, long> ruleBuilder
    ) => ruleBuilder.GreaterThan(0);

    public static IRuleBuilderOptions<T, long?> IsValidId<T>(
        this IRuleBuilder<T, long?> ruleBuilder
    ) => ruleBuilder.GreaterThan(0);

    public static IRuleBuilderOptions<T, Guid> IsValidId<T>(
        this IRuleBuilder<T, Guid> ruleBuilder
    ) => ruleBuilder.NotDefault();

    public static IRuleBuilderOptions<T, Guid?> IsValidId<T>(
        this IRuleBuilder<T, Guid?> ruleBuilder
    ) => ruleBuilder.NotDefault();
}
