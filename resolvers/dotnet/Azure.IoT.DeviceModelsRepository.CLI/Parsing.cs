﻿using Azure.IoT.DeviceModelsRepository.Resolver;
using Azure.IoT.DeviceModelsRepository.Resolver.Extensions;
using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Azure.IoT.DeviceModelsRepository.CLI
{
    internal class Parsing
    {
        private readonly ILogger _logger;
        private readonly string _repository;

        public Parsing(string repository, ILogger logger)
        {
            _logger = logger;
            _repository = repository;
        }

        public ModelParser GetParser(DependencyResolutionOption resolutionOption = DependencyResolutionOption.Enabled)
        {
            ResolverClient client = GetResolver(resolutionOption);
            ModelParser parser = new ModelParser
            {
                DtmiResolver = client.ParserDtmiResolver
            };
            return parser;
        }

        public ResolverClient GetResolver(DependencyResolutionOption resolutionOption = DependencyResolutionOption.Enabled)
        {
            string repository = _repository;
            if (Validations.IsRelativePath(repository))
            {
                repository = Path.GetFullPath(repository);
            }

            return new ResolverClient(
                repository,
                new ResolverClientOptions(resolutionOption),
                _logger);
        }

        public ModelMetadata GetModelMetadata(FileInfo fileName)
        {
            ModelQuery modelQuery = new ModelQuery(File.ReadAllText(fileName.FullName));
            return modelQuery.GetMetadata();
        }

        public List<string> ExtractModels(FileInfo modelsFile)
        {
            List<string> result = new List<string>();
            string modelText = File.ReadAllText(modelsFile.FullName);
            using JsonDocument document = JsonDocument.Parse(modelText);
            JsonElement root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                result.Add(root.GetRawText());
                return result;
            }

            throw new ArgumentException($"Importing model file contents of kind {root.ValueKind} is not yet supported.");
        }
    }
}