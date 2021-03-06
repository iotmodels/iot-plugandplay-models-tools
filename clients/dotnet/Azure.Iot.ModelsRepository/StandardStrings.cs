﻿namespace Azure.Iot.ModelsRepository
{
    internal class StandardStrings
    {
        public const string GenericResolverError = "Unable to resolve \"{0}\". ";
        public const string InvalidDtmiFormat = "Invalid DTMI format \"{0}\". ";
        public const string ClientInitWithFetcher = "Client session {0} initialized with {1} content fetcher. ";
        public const string ProcessingDtmi = "Processing DTMI \"{0}\". ";
        public const string SkippingPreProcessedDtmi = "Already processed DTMI \"{0}\". Skipping. ";
        public const string DiscoveredDependencies = "Discovered dependencies \"{0}\". ";
        public const string FetchingModelContent = "Attempting to retrieve model content from \"{0}\". ";
        public const string ErrorFetchingModelContent = "Model file \"{0}\" not found or not accessible in target repository. ";
        public const string IncorrectDtmiCasing =
            "Retrieved model has incorrect DTMI casing. Expected \"{0}\", parsed \"{1}\". ";
    }
}
