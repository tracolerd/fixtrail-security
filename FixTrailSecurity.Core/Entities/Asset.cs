using System;
namespace FixTrailSecurity.Core.Entities
{
    public class Asset
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Environment { get; private set; }
        public string Owner { get; private set; }
        public DateTime CreatedAt { get; private set; }
        // EF Core requires a parameterless constructor
        protected Asset() 
        { 
            Name = null!;
            Environment = null!;
            Owner = null!;
        }
        public Asset(string name, string environment, string owner)
        {
            if (string.IsNullOrWhiteSpace(name)) 
                throw new ArgumentException("Asset name is required.");
            Id = Guid.NewGuid();
            Name = name;
            Environment = string.IsNullOrWhiteSpace(environment) ? "Unknown" : environment;
            Owner = string.IsNullOrWhiteSpace(owner) ? "Unassigned" : owner;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
