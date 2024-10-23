using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using IcddWebApp.Data;
using IcddWebApp.Services.Models.Enums;
using IIB.ICDD.Handling;
using IIB.ICDD.Parsing;
using IIB.ICDD.Querying;
using Newtonsoft.Json;
using IcddWebApp.Services.Models;
using System.Text.RegularExpressions;
using IIB.ICDD.Serialisation;
using IIB.ICDD.Validation;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Shacl;
using Path = System.IO.Path;

namespace IcddWebApp.Services
{
    public class QueryService : IQueryService
    {
        private readonly DatabaseContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _workfolderPath;
        private readonly IContainerService _containerService;

        public QueryService(DatabaseContext context, IConfiguration configuration, IContainerService containerService)
        {
            _context = context;
            _configuration = configuration;
            _workfolderPath = _configuration["WorkfolderPath"];
            _containerService = containerService;
        }

        public async Task<string> GetQueryContainer(string query, string projectId, ContainerType containerType, string containerId, string containerVersion, bool applyInference)
        {
            var containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            var projectPath = Path.Combine(_workfolderPath, projectId);
            var container = new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd").Read();
            try
            {
                query = query.Replace("\n", " ");
                var query2 = IcddSparqlQueryTemplate.Query(query);
                IcddSparqlProcessor processor = new IcddSparqlProcessor(container, query2, applyInference);
                var test = processor.ExecuteQuery();
                if (containerMetadata != null)
                    return test;
                else
                    return null;
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new IcddException("Query cannot be proceeded.", e));
            }
        }

        public async Task<string> GetShaclContainer(string shapes, string projectId, ContainerType containerType, string containerId, string containerVersion, bool applyInference)
        {
            var containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            var projectPath = Path.Combine(_workfolderPath, projectId);
            var container = new IcddWorkfolderReader(projectPath, containerMetadata.InternalId, $"{containerMetadata.InternalId}.icdd").Read();
            try
            {
                shapes = shapes.Replace("\n", " ");
                Graph g = new Graph();
                TurtleParser ttlParser = new TurtleParser();
                try
                {
                    ttlParser.Load(g, new StringReader(shapes));
                }
                catch (Exception e)
                {
                    return JsonConvert.SerializeObject(new IcddException("Shape file is invalid.", e));
                }

                ShapesGraph graph = new ShapesGraph(g);

                IcddShaclValidator validation = new IcddShaclValidator(container, graph, false, true);
                var results = validation.Results();
                return JsonConvert.SerializeObject(results);

            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new IcddException("Query cannot be proceeded.", e));
            }
        }

        public async Task<bool> PostContainerQuery(string projectId, ContainerType containerType, string containerId, string containerVersion, string query, string queryName, bool applyInference)
        {
            var result = await GetQueryContainer(query, projectId, ContainerType.ICDD, containerId,
                containerVersion, applyInference);

            if (string.IsNullOrEmpty(result))
                return false;
            else
            {
                ContainerMetadata containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
                var duplicate = containerMetadata.SparqlQueries.Where(x => x.Query == query).FirstOrDefault();
                bool isDuplicate = false;
                foreach (var sparqlQuery in containerMetadata.SparqlQueries)
                {
                    if (Regex.Replace(sparqlQuery.Query, @"s", "") == Regex.Replace(query, @"s", ""))
                        isDuplicate = true;
                }

                if (duplicate != null || isDuplicate)
                {
                    return false;
                }
                else
                {
                    containerMetadata.SparqlQueries.Add(new SparqlQuery(query, queryName));
                    await _context.SaveChangesAsync();
                    return true;
                }
            }
        }

        public async Task<bool> DeleteContainerQuery(string projectId, ContainerType containerType, string containerId, string containerVersion, string queryId)
        {
            ContainerMetadata containerMetadata = await _containerService.GetContainerMetadata(projectId, containerType, containerId, containerVersion);
            var deleteQuery = containerMetadata.SparqlQueries.Where(x => x.Id == queryId).SingleOrDefault();
            if (deleteQuery != null)
            {
                containerMetadata.SparqlQueries.Remove(deleteQuery);
                _context.SparqlQueries.Remove(deleteQuery);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        #region Helpers

        private bool ContainerMetadataExists(string id)
        {
            return _context.ContainerMetadata.Any(e => e.Id == id);
        }
        private bool ContainerMetadataVersionExists(string id, string version)
        {
            return _context.ContainerMetadata.Any(e => e.Id == id && e.Version == version);
        }
        private bool ProjectExists(string id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }

        #endregion
    }
}