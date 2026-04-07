namespace Yuviron.Application.Abstractions.Messaging;

public interface ITemplateService
{
    Task<string> RenderTemplateAsync<T>(string templateName, T model);
}