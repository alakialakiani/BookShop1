using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BookShop.Areas.Api.Swagger
{
    public static class SwaggerConfigurationExtentions
    {
        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(
                    "v1",
                    new Info()
                    {
                        Title = "Library Api",
                        Version = "v1",
                        Description = "Through this Api you can access BookInfo",
                        Contact = new Contact
                        {
                            Email = "arezoo.ebrahimi@gmail.com",
                            Name = "arezoo ebrahimi",
                            Url = "http://www.mizfa.com",
                        },
                        License = new License
                        {
                            Name = "License",
                            Url = "http://www.mizfa.com",
                        },
                    });
                c.SwaggerDoc(
                   "v2",
                   new Info()
                   {
                       Title = "Library Api",
                       Version = "v2",
                       Description = "Through this Api you can access BookInfo",
                       Contact = new Contact
                       {
                           Email = "arezoo.ebrahimi@gmail.com",
                           Name = "arezoo ebrahimi",
                           Url = "http://www.mizfa.com",
                       },
                       License = new License
                       {
                           Name = "License",
                           Url = "http://www.mizfa.com",
                       },
                   });



                c.DescribeAllEnumsAsStrings();
                c.DescribeStringEnumsInCamelCase();
                c.DescribeAllParametersInCamelCase();

                c.OperationFilter<RemoveVersionParameters>();
                c.DocumentFilter<SetVersionInPaths>();

                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;

                    var versions = methodInfo.DeclaringType
                        .GetCustomAttributes<ApiVersionAttribute>(true)
                        .SelectMany(attr => attr.Versions);

                    return versions.Any(v => $"v{v.ToString()}" == docName);
                });

                c.OperationFilter<UnauthorizedResponsesOperationFilter>(true, "Bearer");
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header"
                });

                //c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                //{
                //    {"Bearer", new string[] { }}
                //});



                //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                //c.IncludeXmlComments(xmlPath);
            });

        }

        public static void UseSwaggerAndUI(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Api v1");
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "My Api v2");
            });

        }
    }
}
