using System.Reflection;

namespace SlateDb.Configuration.Converter;

public class EnumConverter : ISlateDbConfigurationConverter
{

    static List<FieldInfo> GetEnumMembers(Type enumType)
    {
        var e = Nullable.GetUnderlyingType(enumType) ?? enumType;
        
        if (!e.IsEnum)
        {
            throw new ArgumentException("Type must be an enum", nameof(e));
        }

        // Use GetFields to get all the fields of the enum type
        FieldInfo[] fields = e.GetFields();
        List<FieldInfo> enumMembers = new List<FieldInfo>();

        foreach (FieldInfo field in fields)
        {
            // Enum values are defined at compile time and marked as literals
            if (field.IsLiteral)
            {
                enumMembers.Add(field);
            }
        }

        return enumMembers;
    }

    
    public string ConvertSlateDbProperty(PropertyInfo p, object value)
    {
        if (value == null)
            return null;
        
        var fieldInfos = GetEnumMembers(p.PropertyType);
        var field = fieldInfos.FirstOrDefault(n => n.Name.Equals(value.ToString(), StringComparison.InvariantCultureIgnoreCase));
        if(field == null)
            return value.ToString();
        
        var fieldAttribute = field.GetCustomAttribute<PropertyConverter>();
        if (fieldAttribute != null)
        {
            return fieldAttribute.Value;
        }
        
        return value.ToString();
    }
}