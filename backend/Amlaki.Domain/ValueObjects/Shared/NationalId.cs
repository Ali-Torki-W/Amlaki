using System;
using System.Text.RegularExpressions;

namespace Amlaki.Domain.ValueObjects
{
    public class NationalId : IEquatable<NationalId>
    {
        public string Value { get; }

        private NationalId(string value)
        {
            Value = value;
        }

        public static NationalId Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("National ID cannot be empty.");

            // Remove spaces
            value = value.Trim();

            // Must be exactly 10 digits
            if (!Regex.IsMatch(value, @"^\d{10}$"))
                throw new ArgumentException("National ID must be 10 digits.");

            // (Optional) checksum validation for Iranian IDs
            if (!IsValidChecksum(value))
                throw new ArgumentException("Invalid National ID checksum.");

            return new NationalId(value);
        }

        private static bool IsValidChecksum(string value)
        {
            // Iran’s checksum rule:
            // Multiply the first 9 digits by 10 down to 2, sum them, mod 11 → compare with last digit
            var digits = Array.ConvertAll(value.ToCharArray(), c => (int)char.GetNumericValue(c));

            int sum = 0;
            for (int i = 0; i < 9; i++)
                sum += digits[i] * (10 - i);

            int remainder = sum % 11;
            int checkDigit = digits[9];

            if (remainder < 2)
                return checkDigit == remainder;
            else
                return checkDigit == (11 - remainder);
        }

        // Equality by value
        public bool Equals(NationalId other)
        {
            if (other is null) return false;
            return Value == other.Value;
        }

        public override bool Equals(object obj) => Equals(obj as NationalId);
        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value;
    }
}
