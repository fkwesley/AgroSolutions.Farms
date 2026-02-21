namespace Domain.ValueObjects
{
    public class Location : IEquatable<Location>
    {
        public string City { get; private set; }
        public string State { get; private set; }
        public string Country { get; private set; }

        public Location(string city, string state, string country)
        {
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City cannot be null or empty.", nameof(city));
            
            if (string.IsNullOrWhiteSpace(state))
                throw new ArgumentException("State cannot be null or empty.", nameof(state));
            
            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentException("Country cannot be null or empty.", nameof(country));

            City = city;
            State = state;
            Country = country;
        }

        public bool Equals(Location? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            
            return City == other.City && 
                   State == other.State && 
                   Country == other.Country;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Location);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(City, State, Country);
        }

        public static bool operator ==(Location? left, Location? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(Location? left, Location? right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"{City}, {State}, {Country}";
        }
    }
}
