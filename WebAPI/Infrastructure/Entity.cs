using System.ComponentModel;
namespace WebAPI.Infrastructure
{
    public abstract class Entity : IEquatable<Entity>
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        //public static bool operator ==(Entity? left, Entity? right)
        //{
        //    return left is not null && right is not null && left.Equals(right);
        //}

        //public static bool operator !=(Entity? left, Entity? right)
        //{
        //    return !(left==right);
        //}
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

        public override bool Equals(object? obj)
        {
            if (obj == null) { return false; }
            if (obj.GetType() != GetType()) { return false; }
            if (obj is not Entity entity) { return false; }
            return entity.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool Equals(Entity? other)
        {
            if (other is null) { return false; }
            if (other.GetType() != GetType()) { return false; };
            return other.Id == Id;
        }
    }
}
