using AriesFrameworkCustom.Messages.Transactions;
using AriesFrameworkCustom.Records;
using Hyperledger.Aries;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Features.IssueCredential;
using Hyperledger.Aries.Features.PresentProof;
using Hyperledger.Aries.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AriesFrameworkCustom.Interfaces
{
    /// <summary>
    /// Transaction Service.
    /// </summary>
    public interface ITransactionService
    {
        /// <summary>
        /// Checks for an existing connection for the given endpoint and routing keys.
        /// </summary>
        /// <param name="agentContext"></param>
        /// <param name="connectionInvitationMessage"></param>
        /// <returns>Existing ConnectionRecord or NULL.</returns>
        Task<ConnectionRecord> CheckForExistingConnection(IAgentContext agentContext, ConnectionInvitationMessage connectionInvitationMessage, bool awaitableConnection = false);

        /// <summary>
        /// Create or update a transaction connection for a by default multiinvitation async.
        /// </summary>
        /// <param name="agentContext">Agent Context.</param>
        /// <param name="transactionId" >Transaction ID.</param>
        /// <param name="connectionId">Connection ID.</param>
        /// <param name="credentialOfferJson">Credential Offer Configuration.</param>
        /// <param name="proofRequest">Proof Request.</param>
        /// <param name="connectionConfig">Config for the Connection.</param>
        /// <param name="CredentialComment"></param>
        /// <param name="ProofComment"></param>
        /// <returns>The response async.</returns>
        Task<(TransactionRecord transaction, ConnectionRecord connection, ConnectionInvitationMessage message)> CreateOrUpadteTransactionAsync(IAgentContext agentContext, string transactionId = null, string connectionId = null, OfferConfiguration credentialOfferJson = null, ProofRequest proofRequest = null, InviteConfiguration connectionConfig = null, string CredentialComment = "", string ProofComment = "");

        /// <summary>
        /// Deletes the transaction record
        /// </summary>
        /// <param name="agentContext"></param>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        Task DeleteAsync(IAgentContext agentContext, string transactionId);

        /// <summary>
        /// Gets transaction record for the given identifier.
        /// </summary>
        /// <param name="agentContext">Agent Context.</param>
        /// <param name="transactionId">The credential identifier.</param>
        /// <exception cref="AriesFrameworkException">Throws with ErrorCode.RecordNotFound.</exception>
        /// <returns>The stored transaction record</returns>
        Task<TransactionRecord> GetAsync(IAgentContext agentContext, string transactionId);

        /// <summary>
        /// Retrieves a list of <see cref="TransactionRecord"/> items for the given search criteria.
        /// </summary>
        /// <param name="agentContext">Agent Context.</param>
        /// <param name="query">The query.</param>
        /// <param name="count">The number of items to return</param>
        /// <returns>A list of transaction records matching the search criteria</returns>
        Task<List<TransactionRecord>> ListAsync(IAgentContext agentContext, ISearchQuery query = null, int count = 100);

        /// <summary>
        /// Processes the transaction for a given connection async.
        /// </summary>
        /// <param name="agentContext">Agent Context.</param>
        /// <param name="connectionTransactionMessage">Transaction ID.</param>
        /// <param name="connection">Connection Record.</param>
        /// <returns>Connection identifier this request is related to.</returns>
        Task ProcessTransactionAsync(IAgentContext agentContext, TransactionResponseMessage connectionTransactionMessage, ConnectionRecord connection);

        /// <summary>
        /// Reads a tramsaction URL aync.
        /// </summary>
        /// <param name="invitationUrl">Invitation URL.</param>
        /// <returns>Transaction ID and ConnectionInvitationMessage.</returns>
        (string sessionId, ConnectionInvitationMessage connectionInvitationMessage, bool awaitableConnection) ReadTransactionUrl(string invitationUrl);

        /// <summary>
        /// Send the transaction response for a given connection async.
        /// </summary>
        /// <param name="agentContext">Agent Context.</param>
        /// <param name="transactionId">Transaction ID.</param>
        /// <param name="connection">Connection Record.</param>
        /// <returns></returns>
        Task SendTransactionResponse(IAgentContext agentContext, string transactionId, ConnectionRecord connection);
    }
}
