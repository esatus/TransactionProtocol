using Hyperledger.Aries.Agents;
using Newtonsoft.Json;
using System;

namespace AriesFrameworkCustom.Messages.Transactions
{
    /// <summary>
    /// Represents a connection transaction message
    /// </summary>
    public class TransactionOfferMessage : AgentMessage
    {
        /// <inheritdoc />
        public TransactionOfferMessage()
        {
            Id = Guid.NewGuid().ToString();
            Type = CustomMessageTypes.TransactionOffer;
        }

        /// <summary>
        /// Gets or sets human readable comment about this Connection Transaction
        /// </summary>
        /// <value></value>
        [JsonProperty("comment")]
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets human readable transaction information about this Connection Transaction
        /// </summary>
        /// <value></value>
        [JsonProperty("transaction")]
        public string Transaction { get; set; }
    }
}
