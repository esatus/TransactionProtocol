using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Features.IssueCredential;
using Hyperledger.Aries.Features.PresentProof;
using Hyperledger.Aries.Storage;

namespace AriesFrameworkCustom.Records
{
    /// <summary>
    /// Represents a connection record in the agency wallet.
    /// </summary>
    /// <seealso cref="RecordBase" />
    public class TransactionRecord : CustomRecordBase
    {
        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        /// <returns>The type name.</returns>
        public override string TypeName => "AF.TransactionRecord";

        /// <summary>
        /// Gets or sets the connection identifier associated with this transaction.
        /// </summary>
        /// <value>The connection identifier.</value>
        public string ConnectionId
        {
            get => Get();
            set => Set(value);
        }

        /// <summary>
        /// Gets or sets the Used value for this transaction.
        /// </summary>
        /// <value>The used value.</value>
        public bool Used
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the connection associated with this transaction.
        /// </summary>
        /// <value>The connection record.</value>
        public ConnectionRecord ConnectionRecord
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the credential offer configuration.
        /// </summary>
        /// <value>The offer configuration.</value>
        public OfferConfiguration OfferConfiguration
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the credential record id.
        /// </summary>
        /// <value>The credential record id.</value>
        public string CredentialRecordId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the proof request.
        /// </summary>
        /// <value>The proof request.</value>
        public ProofRequest ProofRequest
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the proof record id.
        /// </summary>
        /// <value>The proof record id.</value>
        public string ProofRecordId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the credential message comment.
        /// </summary>
        /// <value>The credential message comment.</value>
        public string CredentialComment
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the proof message comment.
        /// </summary>
        /// <value>The proof message comment.</value>
        public string ProofComment
        {
            get;
            set;
        }
    }
}
