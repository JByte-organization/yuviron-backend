using Fluid;
using System;
using System.IO;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Infrastructure.Services;

public class FluidTemplateService : ITemplateService
{
    private readonly FluidParser _parser;

    public FluidTemplateService()
    {
        _parser = new FluidParser();
    }

    public async Task<string> RenderTemplateAsync<T>(string templateName, T model)
    {
        // Ищем файл шаблона в папке Templates/Emails рядом с исполняемым файлом
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "Emails", $"{templateName}.html");
        
        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Template {templateName} not found at {templatePath}");

        var source = await File.ReadAllTextAsync(templatePath);

        if (_parser.TryParse(source, out var template, out var error))
        {
            var options = new TemplateOptions();
            // Разрешаем шаблонизатору читать свойства переданной модели (DTO)
            options.MemberAccessStrategy.Register(typeof(T)); 
            
            var context = new TemplateContext(model, options);
            
            return await template.RenderAsync(context);
        }

        throw new InvalidOperationException($"Error parsing template {templateName}: {error}");
    }
}