using AriesFrameworkCustom.Events;
using AriesFrameworkCustom.Interfaces;
using AriesFrameworkCustom.Messages.Transactions;
using AriesFrameworkCustom.Records;
using Hyperledger.Aries;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Configuration;
using Hyperledger.Aries.Extensions;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Features.IssueCredential;
using Hyperledger.Aries.Features.PresentProof;
using Hyperledger.Aries.Storage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AriesFrameworkCustom.Services
{
    /// <inheritdoc />
    public class TransactionService : ITransactionService
    {
        /// <summary>
        /// The record service
        /// </summary>
        protected readonly IWalletRecordService RecordService;
        /// <summary>
        /// The provisioning service
        /// </summary>
        protected readonly IProvisioningService ProvisioningService;
        /// <summary>
        /// The message service
        /// </summary>
        protected readonly IMessageService MessageService;
        /// <summary>
        /// The connection service
        /// </summary>
        protected readonly IConnectionService ConnectionService;
        /// <summary>
        /// The connection service
        /// </summary>
        protected readonly ICredentialService CredentialService;
        /// <summary>
        /// The proof service
        /// </summary>
        protected readonly IProofService ProofService;
        /// <summary>
        /// The logger
        /// </summary>
        protected readonly ILogger<TransactionService> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionService"/> class.
        /// </summary>
        /// <param name="recordService">The record service.</param>
        /// <param name="provisioningService">The provisioning service.</param>
        /// <param name="messageService">The message service.</param>
        /// <param name="connectionService">The connection service.</param>
        /// <param name="credentialService">The credential service.</param>
        /// <param name="proofService">The proof service.</param>
        /// <param name="logger">The logger.</param>
        public TransactionService(
            IWalletRecordService recordService,
            IProvisioningService provisioningService,
            IMessageService messageService,
            IConnectionService connectionService,
            ICredentialService credentialService,
            IProofService proofService,
            ILogger<TransactionService> logger)
        {
            ProvisioningService = provisioningService;
            MessageService = messageService;
            ConnectionService = connectionService;
            CredentialService = credentialService;
            ProofService = proofService;
            Logger = logger;
            RecordService = recordService;
        }

        /// <inheritdoc />
        public virtual async Task<TransactionRecord> GetAsync(IAgentContext agentContext, string transactionId)
        {
            var record = await RecordService.GetAsync<TransactionRecord>(agentContext.Wallet, transactionId);

            if (record == null)
            {
                throw new AriesFrameworkException(ErrorCode.RecordNotFound, "Transaction record not found");
            }

            return record;
        }

        /// <inheritdoc />
        public virtual Task<List<TransactionRecord>> ListAsync(IAgentContext agentContext, ISearchQuery query = null, int count = 100)
        {
            return RecordService.SearchAsync<TransactionRecord>(agentContext.Wallet, query, null, count);
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(IAgentContext agentContext, string transactionId)
        {
            await RecordService.DeleteAsync<TransactionRecord>(agentContext.Wallet, transactionId);
        }

        /// <inheritdoc />
        public virtual async Task<(TransactionRecord transaction, ConnectionRecord connection, ConnectionInvitationMessage message)> CreateOrUpadteTransactionAsync(
            IAgentContext agentContext,
            string transactionId = null,
            string connectionId = null,
            OfferConfiguration offerConfiguration = null,
            ProofRequest proofRequest = null,
            InviteConfiguration connectionConfig = null,
            string CredentialComment = "",
            string ProofComment = "")
        {
            Logger.LogInformation(CustomLoggingEvents.CreateTransaction, "For {1}", transactionId);

            (ConnectionInvitationMessage message, ConnectionRecord record) connection;
            TransactionRecord transactionRecord;

            if (string.IsNullOrEmpty(connectionId))
            {
                connectionConfig = connectionConfig ?? new InviteConfiguration()
                {
                    AutoAcceptConnection = true,
                    MultiPartyInvitation = true
                };

                connection = (await ConnectionService.CreateInvitationAsync(agentContext, connectionConfig));
                connection.record.SetTag("InvitationMessage", JObject.FromObject(connection.message).ToString());
                await RecordService.UpdateAsync(agentContext.Wallet, connection.record);
            }
            else
            {
                if ((await ConnectionService.GetAsync(agentContext, connectionId)) != null)
                {
                    connection.record = await RecordService.GetAsync<ConnectionRecord>(agentContext.Wallet, connectionId);
                    var message = connection.record.GetTag("InvitationMessage");
                    connection.message = JObject.Parse(message).ToObject<ConnectionInvitationMessage>();
                }
                else
                {
                    throw new AriesFrameworkException(ErrorCode.RecordNotFound,
                        $"Connection '{connectionId}' not found.");
                }
            }

            transactionId = !string.IsNullOrEmpty(transactionId)
            ? transactionId
            : Guid.NewGuid().ToString();

            if ((await RecordService.GetAsync<TransactionRecord>(agentContext.Wallet, transactionId)) == null)
            {
                transactionRecord = new TransactionRecord
                {
                    Id = transactionId,
                    ConnectionId = connection.record.Id,
                    OfferConfiguration = offerConfiguration,
                    ProofRequest = proofRequest,
                    Used = false,
                    CredentialComment = CredentialComment,
                    ProofComment = ProofComment
                };

                await RecordService.AddAsync(agentContext.Wallet, transactionRecord);
            }
            else
            {
                transactionRecord = await RecordService.GetAsync<TransactionRecord>(agentContext.Wallet, transactionId);

                transactionRecord.ConnectionId = connection.record.Id;
                transactionRecord.OfferConfiguration = offerConfiguration;
                transactionRecord.ProofRequest = proofRequest;

                await RecordService.UpdateAsync(agentContext.Wallet, transactionRecord);
            }

            return (transactionRecord, connection.record, connection.message);
        }

        /// <inheritdoc />
        public virtual async Task ProcessTransactionAsync(IAgentContext agentContext, TransactionResponseMessage connectionTransactionMessage, ConnectionRecord connection)
        {
            Logger.LogInformation(CustomLoggingEvents.ProcessTransaction, "To {1}", connection.TheirDid);

            var transactionRecord = await RecordService.GetAsync<TransactionRecord>(agentContext.Wallet, connectionTransactionMessage.Transaction);

            if (!transactionRecord.Used)
            {
                transactionRecord.ConnectionRecord = connection;

                if (transactionRecord.ProofRequest != null)
                {
                    var (message, record) = await ProofService.CreateRequestAsync(agentContext, transactionRecord.ProofRequest);

                    transactionRecord.ProofRecordId = record.Id;
                    var deleteId = Guid.NewGuid().ToString();
                    message.AddDecorator(deleteId, "delete_id");
                    record.SetTag("delete_id", deleteId);
                    record.ConnectionId = connection.Id;

                    message.Comment = transactionRecord.ProofComment;

                    await RecordService.UpdateAsync(agentContext.Wallet, record);

                    await MessageService.SendAsync(agentContext.Wallet, message, connection);
                }

                if (transactionRecord.OfferConfiguration != null)
                {
                    var (credentialOfferMessage, credentialRecord) = await CredentialService.CreateOfferAsync(agentContext, transactionRecord.OfferConfiguration, connection.Id);

                    transactionRecord.CredentialRecordId = credentialRecord.Id;

                    credentialOfferMessage.Comment = transactionRecord.CredentialComment;

                    await MessageService.SendAsync(agentContext.Wallet, credentialOfferMessage, connection);
                }

                transactionRecord.Used = true;

                await RecordService.UpdateAsync(agentContext.Wallet, transactionRecord);
            }
            else
            {
                var message = new TransactionErrorMessage()
                {
                    Transaction = transactionRecord.Id
                };

                await MessageService.SendAsync(agentContext.Wallet, message, connection);
            }
        }

        /// <inheritdoc />
        public virtual async Task SendTransactionResponse(IAgentContext agentContext, string transactionId, ConnectionRecord connection)
        {
            TransactionResponseMessage message = new TransactionResponseMessage()
            {
                Transaction = transactionId
            };

            await MessageService.SendAsync(agentContext.Wallet, message, connection);
        }

        /// <inheritdoc />
        public virtual async Task<ConnectionRecord> CheckForExistingConnection(IAgentContext agentContext, ConnectionInvitationMessage connectionInvitationMessage, bool awaitableConnection = false)
        {
            var transactionConnections = await ConnectionService.ListAsync(agentContext, null, 2147483647);

            if (!awaitableConnection)
            {
                var transactionConnectionsEndpoints = transactionConnections.Where(x => x.Endpoint != null && x.Endpoint.Uri == connectionInvitationMessage.ServiceEndpoint && x.State == ConnectionState.Connected && x.Endpoint.Verkey != null); transactionConnections.Where(x => x.Endpoint.Uri == connectionInvitationMessage.ServiceEndpoint && x.State == ConnectionState.Connected && x.Endpoint.Verkey != null);

                return transactionConnectionsEndpoints.Where(x => x.Endpoint.Verkey.SequenceEqual(connectionInvitationMessage.RoutingKeys)).OrderByDescending(x => x.CreatedAtUtc).FirstOrDefault();
            }
            else
            {
                var transactionConnectionsEndpoints = transactionConnections.Where(x => x.Endpoint != null && x.Endpoint.Uri == connectionInvitationMessage.ServiceEndpoint && x.State == ConnectionState.Connected && x.Endpoint.Verkey != null && x.GetTag("RecipientKeys") != null); transactionConnections.Where(x => x.Endpoint.Uri == connectionInvitationMessage.ServiceEndpoint && x.State == ConnectionState.Connected && x.Endpoint.Verkey != null);

                return transactionConnectionsEndpoints.Where(x => x.Endpoint.Verkey.SequenceEqual(connectionInvitationMessage.RoutingKeys) && x.GetTag("RecipientKeys").Equals(connectionInvitationMessage.RecipientKeys.ToJson())).OrderByDescending(x => x.CreatedAtUtc).FirstOrDefault();
            }
        }

        /// <inheritdoc />
        public (string sessionId, ConnectionInvitationMessage connectionInvitationMessage, bool awaitableConnection) ReadTransactionUrl(string invitationUrl)
        {
            string sessionId = null;
            ConnectionInvitationMessage invitationMessage = null;
            bool awaitableConnection = false;

            Uri uri = null;
            try
            {
                uri = new Uri(invitationUrl);
            }
            catch (Exception)
            {
                return (null, null, false);
            }
            try
            {
                if (uri.Query.StartsWith("?t_o="))
                {
                    var arguments = uri.Query
                          .Substring(1) // Remove '?'
                          .Split('&')
                          .Select(q => q.Split('='))
                          .ToDictionary(q => q.FirstOrDefault(), q => q.Skip(1).FirstOrDefault());

                    sessionId = arguments["t_o"];

                    invitationMessage = arguments["c_i"].FromBase64().ToObject<ConnectionInvitationMessage>();

                    try
                    {
                        awaitableConnection = bool.Parse(arguments["wait"]);
                    }
                    catch (Exception)
                    {
                        awaitableConnection = false;
                    }

                    return (sessionId, invitationMessage, awaitableConnection);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return (sessionId, invitationMessage, awaitableConnection);
        }
    }
}
