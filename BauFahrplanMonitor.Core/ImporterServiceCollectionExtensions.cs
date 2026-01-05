using BauFahrplanMonitor.Core.Importer;
using BauFahrplanMonitor.Core.Importer.Upsert;
using BauFahrplanMonitor.Core.Jobs;
using BauFahrplanMonitor.Core.Services;
using BauFahrplanMonitor.Core.Tools;
using BauFahrplanMonitor.Importer;
using BauFahrplanMonitor.Importer.Mapper;
using BauFahrplanMonitor.Importer.Upsert;
using BauFahrplanMonitor.Importer.Xml;
using BauFahrplanMonitor.Interfaces;
using BauFahrplanMonitor.Resolver;
using BauFahrplanMonitor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BauFahrplanMonitor.Core;

public static class ImporterServiceCollectionExtensions {

    public static IServiceCollection AddImporterServices(
        this IServiceCollection services) {

        services.AddSingleton<ConfigService>();

        services.AddSingleton<SharedReferenceResolver>();
        services.AddSingleton<ZvFZugCache>();

        services.AddSingleton<IZvFExportXmlLoader, ZvFExportXmlLoader>();
        services.AddSingleton<IZvFDtoMapper, ZvFDtoMapper>();
        services.AddSingleton<IZvFUpserter, ZvFUpserter>();

        services.AddSingleton<IUeBDtoMapper, UeBDtoMapper>();
        services.AddSingleton<IUeBUpserter, UeBUpserter>();

        services.AddSingleton<IFploDtoMapper, FploDtoMapper>();
        services.AddSingleton<IFploUpserter, FploUpserter>();

        services.AddSingleton<ZvFExportImporter>();
        services.AddSingleton<ZvFExportJob>();
        services.AddSingleton<ImporterFacade>();
        services.AddSingleton<ZvFExportScanService>();

        return services;
    }
}