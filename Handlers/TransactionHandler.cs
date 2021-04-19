using AriesFrameworkCustom.Interfaces;
using AriesFrameworkCustom.Messages;
using AriesFrameworkCustom.Messages.Transactions;
using Hyperledger.Aries;
using Hyperledger.Aries.Agents;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AriesFrameworkCustom.Handlers
{
    /// <inheritdoc />
    public class TransactionHandler : IMessageHandler
    {
        private readonly ITransactionService _transactionService;
        private readonly IMessageService _messageService;

        /// <summary>Initializes a new instance of the <see cref="TransactionHandler"/> class.</summary>
        /// <param name="messageService">The message service.</param>
        /// <param name="transactionService">The transaction service.</param>
        /// 
        public TransactionHandler(
            IMessageService messageService,
            ITransactionService transactionService)
        {
            _messageService = messageService;
            _transactionService = transactionService;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the supported message types.
        /// </summary>
        /// <value>
        /// The supported message types.
        /// </value>
        public IEnumerable<MessageType> SupportedMessageTypes => new[]
        {
            new MessageType(CustomMessageTypes.TransactionResponse),
            new MessageType(CustomMessageTypes.TransactionResponseHttps)
        };

        /// <summary>
        /// Processes the agent message
        /// </summary>
        /// <param name="agentContext"></param>
        /// <param name="messageContext">The agent message agentContext.</param>
        /// <returns></returns>
        /// <exception cref="AriesFrameworkException">Unsupported message type {message.Type}</exception>
        public async Task<AgentMessage> ProcessAsync(IAgentContext agentContext, UnpackedMessageContext messageContext)
        {
            switch (messageContext.GetMessageType())
            {
                case CustomMessageTypes.TransactionResponse:
                case CustomMessageTypes.TransactionResponseHttps:
                    {
                        var transaction = messageContext.GetMessage<TransactionResponseMessage>();
                        await _transactionService.ProcessTransactionAsync(agentContext, transaction, messageContext.Connection);
                        messageContext.ContextRecord = messageContext.Connection;
                        return null;
                    }

                default:
                    throw new AriesFrameworkException(ErrorCode.InvalidMessage,
                        $"Unsupported message type {messageContext.GetMessageType()}");
            }
        }
    }
}
