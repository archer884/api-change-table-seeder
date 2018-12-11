using System;
using System.Collections.Generic;
using System.Linq;

namespace Tracking
{
    public interface ITrackable
    {
        IEnumerable<Property> TrackedProperties();
    }

    public class Property
    {
        public string MemberName { get; set; }
        public object Value { get; set; }

        public Property(string memberName, object value)
        {
            MemberName = memberName;
            Value = value;
        }

        public bool HasChanged(Property other)
        {
            return !Value.Equals(other.Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class PropertyChange
    {
        public string MemberName { get; set; }
        public object Original { get; set; }
        public object Modified { get; set; }

        public PropertyChange(Property property)
        {
            MemberName = property.MemberName;
            Modified = property.Value;
        }

        public PropertyChange(Property original, Property modified)
        {
            if (original.MemberName != modified.MemberName)
            {
                throw new InvalidOperationException("Property change must be created with two of the same property");
            }

            MemberName = original.MemberName;
            Original = original.Value;
            Modified = modified.Value;
        }
    }
}
