namespace AriesFrameworkCustom.Messages
{
    /// <summary>
    /// Protocol message types
    /// </summary>
    public static class CustomMessageTypes
    {
        /// <summary>
        /// The transaction offer.
        /// </summary>
        public const string TransactionOffer = "did:sov:BzCbsNYhMrjHiqZDTUASHg;spec/transaction/1.0/offer";

        /// <summary>
        /// The transaction response.
        /// </summary>
        public const string TransactionResponse = "did:sov:BzCbsNYhMrjHiqZDTUASHg;spec/transaction/1.0/response";

        /// <summary>
        /// The transaction error.
        /// </summary>
        public const string TransactionError = "did:sov:BzCbsNYhMrjHiqZDTUASHg;spec/transaction/1.0/error";

        /// <summary>
        /// The transaction offer.
        /// </summary>
        public const string TransactionOfferHttps = "https://didcomm.org/transaction/1.0/offer";

        /// <summary>
        /// The transaction response.
        /// </summary>
        public const string TransactionResponseHttps = "https://didcomm.org/transaction/1.0/response";

        /// <summary>
        /// The transaction error.
        /// </summary>
        public const string TransactionErrorHttps = "https://didcomm.org/transaction/1.0/error";
    }
}