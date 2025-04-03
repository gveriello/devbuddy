using System.Collections.Concurrent;
using System.ComponentModel;

namespace devbuddy.common.Services
{
    public class MemoryCacheService
    {
        private readonly ConcurrentDictionary<string, (DateTime? ExpiryDate, object? Value)> DataSource;

        public MemoryCacheService()
            => DataSource = new ConcurrentDictionary<string, (DateTime? ExpiryDate, object? Value)>();

        public bool AddOrUpdate(string key, object? value, DateTime? expiryDate = null)
        {
            key = key?.ToLower() ?? throw new ArgumentNullException(nameof(key));

            RemoveIfExpired(key);
            if (!DataSource.TryGetValue(key, out _))
            {
                return DataSource.TryAdd(key, (expiryDate, value));
            }

            DataSource[key] = (expiryDate, value);
            return true;
        }

        public bool Exists(string key)
        {
            key = key?.ToLower() ?? throw new ArgumentNullException(nameof(key));

            return DataSource.Keys.Contains(key) && DateTime.Now > DataSource[key].ExpiryDate;
        }

        public bool TryGetValueIfIsNotExpired<TToParse>(string key, out TToParse? toParse)
        {
            static TToParse ConvertValue<TToParse>(object? value)
            {
                if (value == null)
                    return default;

                if (value is TToParse castValue)
                    return castValue;

                var converter = TypeDescriptor.GetConverter(typeof(TToParse));
                if (converter != null && converter.CanConvertFrom(value.GetType()))
                {
                    return (TToParse)converter.ConvertFrom(value);
                }

                throw new InvalidCastException($"Cannot convert {value} to {typeof(TToParse)}");
            }

            toParse = default;
            key = key?.ToLower() ?? throw new ArgumentNullException(nameof(key));
            RemoveIfExpired(key);

            if (DataSource.TryGetValue(key, out var entry))
            {
                toParse = ConvertValue<TToParse>(entry.Value);
                return true;
            }

            return false;
        }

        private void RemoveIfExists(string key)
        {
            key = key?.ToLower() ?? throw new ArgumentNullException(nameof(key));

            DataSource.TryRemove(key, out _);
        }

        private void RemoveIfExpired(string key)
        {
            key = key?.ToLower() ?? throw new ArgumentNullException(nameof(key));
            if (DataSource.TryGetValue(key, out var row) && row.ExpiryDate < DateTime.Now)
                RemoveIfExists(key);
        }

        public void RemoveAllExpired()
            => DataSource.Where(row => row.Value.ExpiryDate < DateTime.Now)?
                         .ToList()
                         .ForEach(row => RemoveIfExists(row.Key));

        public void InvalidateCache(string key = null)
        {
            if (!string.IsNullOrEmpty(key))
                RemoveIfExists(key);

            RemoveAllExpired();
        }

#if DEBUG
        public override string ToString()
        {
            var toReturn = string.Empty;
            foreach (var row in DataSource)
            {
                toReturn += $"key: {row.Key} - value: {row.Value.Value} - expiryDate: {row.Value.ExpiryDate} {Environment.NewLine}";
            }
            return toReturn;
        }
#endif
    }
}
