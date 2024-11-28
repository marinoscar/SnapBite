using System.Text.Json;

namespace SnapBite
{
    /// <summary>
    /// Provides a way to read values from multiple JSON documents.
    /// </summary>
    public class ConfigHelper
    {
        /// <summary>
        /// A collection of JSON documents to be queried.
        /// </summary>
        private static List<JsonDocument> _documents = new List<JsonDocument>();

        /// <summary>
        /// Adds a JSON document to the collection.
        /// </summary>
        /// <param name="json">The JSON string to parse and add.</param>
        public static void AddJsonDocument(string json)
        {
            _documents.Add(JsonDocument.Parse(json));
        }

        /// <summary>
        /// Clears all JSON documents from the collection.
        /// </summary>
        public static void ClearJsonDocuments()
        {
            _documents.Clear();
        }

        /// <summary>
        /// Retrieves a JSON value as a <see cref="JsonElement"/> for a specified property name.
        /// Searches through all added JSON documents in the order they were added.
        /// </summary>
        /// <param name="property">The name of the property to retrieve.</param>
        /// <returns>
        /// A <see cref="JsonElement"/> representing the value if the property is found;
        /// otherwise, <c>null</c>.
        /// </returns>
        public static JsonElement? GetValue(string property)
        {
            foreach (var doc in _documents)
            {
                var root = doc.RootElement;
                var result = GetValueRecursive(root, property.Split(':'));
                if (result.HasValue) return result;
            }
            return null;
        }

        private static JsonElement? GetValueRecursive(JsonElement element, string[] propertyPath, int index = 0)
        {
            if (index >= propertyPath.Length)
                return element; // Base case: reached the target value

            if (element.TryGetProperty(propertyPath[index], out var nextElement))
            {
                // Recur to the next level in the path
                return GetValueRecursive(nextElement, propertyPath, index + 1);
            }

            return null; // Property not found
        }

        /// <summary>
        /// Retrieves a value of type <typeparamref name="T"/> for a specified property name.
        /// Attempts to deserialize the JSON value to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON value into.</typeparam>
        /// <param name="property">The name of the property to retrieve.</param>
        /// <returns>
        /// The value of type <typeparamref name="T"/> if found and deserialization is successful;
        /// otherwise, the default value of <typeparamref name="T"/>.
        /// </returns>
        public static T GetValue<T>(string property)
        {
            var value = GetValue(property);

            return value.HasValue
                ? value.Value.Deserialize<T>()
                : default;
        }

        /// <summary>
        /// Retrieves a value as a string for a specified property name.
        /// Internally uses <see cref="GetValue{T}(string)"/> to attempt deserialization.
        /// </summary>
        /// <param name="property">The name of the property to retrieve.</param>
        /// <returns>
        /// A string representation of the value if found; otherwise, <c>null</c>.
        /// </returns>
        public static string GetValueAsString(string property)
        {
            return GetValue<string>(property);
        }
    }

}
