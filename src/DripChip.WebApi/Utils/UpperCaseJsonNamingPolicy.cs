using System.Text.Json;

namespace DripChip.WebApi.Utils;

public sealed class UpperCaseJsonNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) => name.ToUpper();
}
