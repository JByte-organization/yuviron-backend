using Fluid;
using Microsoft.Extensions.Options;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Configuration;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Yuviron.Infrastructure.Services;

public class FluidTemplateService : ITemplateService
{
    private readonly FluidParser _parser;
    private readonly FrontendOptions _frontendOptions;

    public FluidTemplateService(IOptions<FrontendOptions> frontendOptions)
    {
        _parser = new FluidParser();
        _frontendOptions = frontendOptions.Value;
    }

    public async Task<string> RenderTemplateAsync<T>(string templateName, T model)
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "Emails", $"{templateName}.html");
        
        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Template {templateName} not found at {templatePath}");

        var source = await File.ReadAllTextAsync(templatePath);

        if (_parser.TryParse(source, out var template, out var error))
        {
            var options = new TemplateOptions();
            options.MemberAccessStrategy.Register(typeof(T)); 
            
            var context = new TemplateContext(model, options);
            
            var baseUrl = _frontendOptions.BaseUrl.TrimEnd('/');
            context.SetValue("FrontendUrl", baseUrl);
            
            // Backwards compatibility for old templates using {{ BaseUrl }}
            context.SetValue("BaseUrl", baseUrl);
            
            return await template.RenderAsync(context);
        }

        throw new InvalidOperationException($"Error parsing template {templateName}: {error}");
    }
}
