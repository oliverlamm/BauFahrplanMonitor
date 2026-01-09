using BauFahrplanMonitor.Core.Importer;
using BauFahrplanMonitor.Core.Importer.Mapper;
using BauFahrplanMonitor.Core.Importer.Upsert;
using BauFahrplanMonitor.Core.Importer.Xml;
using BauFahrplanMonitor.Core.Interfaces;
using BauFahrplanMonitor.Core.Jobs;
using BauFahrplanMonitor.Core.Resolver;
using BauFahrplanMonitor.Core.Services;
using BauFahrplanMonitor.Core.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace BauFahrplanMonitor.Core;

public static class ImporterServiceCollectionExtensions {

    public static IServiceCollection AddImporterServices(
        this IServiceCollection services) {
        services.AddSingleton<ConfigService>();

        services.AddSingleton<SharedReferenceResolver>();
        services.AddSingleton<ZvFZugCache>();

        // -----------------------------
        // ZvF / ÜB / FPLO
        // -----------------------------
        services.AddSingleton<IZvFExportXmlLoader, ZvFExportXmlLoader>();
        services.AddSingleton<IZvFDtoMapper, ZvFDtoMapper>();
        services.AddSingleton<IZvFUpserter, ZvFUpserter>();

        services.AddSingleton<IUeBDtoMapper, UeBDtoMapper>();
        services.AddSingleton<IUeBUpserter, UeBUpserter>();

        services.AddSingleton<IFploDtoMapper, FploDtoMapper>();
        services.AddSingleton<IFploUpserter, FploUpserter>();

        services.AddSingleton<ZvFExportImporter>();
        services.AddSingleton<ZvFExportScanService>();
        services.AddSingleton<ZvFExportJob>();

        // -----------------------------
        // Netzfahrplan (KSS)
        // -----------------------------
        services.AddSingleton<IKssXmlLoader, KssXmlLoader>();
        services.AddSingleton<INfplZugCache, Importer.Helper.NfplZugCache>();
        services.AddSingleton<INetzfahrplanMapper, NetzfahrplanMapper>();
        services.AddSingleton<INetzfahrplanUpserter, NetzfahrplanUpserter>();
        services.AddSingleton<NetzfahrplanImporter>();
        services.AddSingleton<NetzfahrplanJob>();

        // -----------------------------
        // BBPNeo
        // -----------------------------
        services.AddSingleton<IBbpNeoXmlStreamingLoader, BbpNeoXmlStreamingLoader>();
        services.AddSingleton<IBbpNeoUpserter, BbpNeoUpserter>();
        services.AddSingleton<BbpNeoImporter>();
        services.AddSingleton<BbpNeoJob>();

        // -----------------------------
        // Facade
        // -----------------------------
        services.AddSingleton<ImporterFacade>();

        return services;
    }
}