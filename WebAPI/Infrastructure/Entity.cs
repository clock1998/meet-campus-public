using System.ComponentModel;
namespace WebAPI.Infrastructure
{
    public abstract class Entity : IEquatable<Entity>
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public static bool operator == (Entity left, Entity right) => left.Id == right.Id;

        public static bool operator != (Entity left, Entity right) => left.Id != right.Id;
        public string? GetDisplayName()
        {
            Type type = GetType();
            var propertyInfos = type.GetProperties();
            var prop = propertyInfos.FirstOrDefault(n => n.GetCustomAttributes(true).Any(n => n.GetType() == typeof(DisplayNameAttribute)));
            if (prop != null)
            {
                return prop.GetValue(this)?.ToString();
            }
            return null;
        }

        public override bool Equals(object? obj) => Equals(obj as Entity);

        public override int GetHashCode() => Id.GetHashCode();

        public bool Equals(Entity? other) => other is not null && other.Id == Id;
    }
}