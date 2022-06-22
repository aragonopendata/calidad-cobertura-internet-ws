using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Examples;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace ws_cobertura.httpapi.Model.Request
{
    class PostDataRequestExample : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(PostDataRequest))
            {
                schema.Example = new OpenApiObject()
                {
                    ["fechaDesde"] = new OpenApiString("20/03/2022 00:01:00"),
                    ["fechaHasta"] = new OpenApiString("20/04/2022 00:01:00"),
                    ["municipio"] = new OpenApiString(""),
                    ["ine"] = new OpenApiString("")
                };
            }
        }
    }
}