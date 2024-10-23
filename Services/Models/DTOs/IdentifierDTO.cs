using IcddWebApp.Services.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IcddWebApp.Services.Models.DTOs
{
    public class IdentifierDTO
    {
        public LinkIdentifierType type { get; set; }

        public string identifier { get; set; }
        public string identifierField { get; set; }

        public Uri uri { get; set; }

        public string queryExpression { get; set; }
        public string queryLanguage { get; set; }

        public IdentifierDTO() { }
        public IdentifierDTO(LinkIdentifierType type)
        {
            this.type = type;
        }

        public IdentifierDTO(LinkIdentifierType type, string id, string identifierField, bool stringbased)
        {
            this.type = type;
            identifier = id;
            this.identifierField = identifierField;
        }

        public IdentifierDTO(LinkIdentifierType type, string uri)
        {
            this.type = type;
            this.uri = new Uri(uri);
        }

        public IdentifierDTO(LinkIdentifierType type, string expression, string language)
        {
            this.type = type;
            queryExpression = expression;
            queryLanguage = language;
        }
    }
}