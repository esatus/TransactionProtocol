using Hyperledger.Aries.Storage;
using System.Text.Json.Serialization;

namespace AriesFrameworkCustom.Records
{
    /// <inheritdoc />
    public class CustomRecordBase : RecordBase
    {
        /// <inheritdoc />
        public CustomRecordBase() : base() { }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        /// <returns>The type name.</returns>
        [JsonIgnore]
        public override string TypeName { get; }
    }
}
