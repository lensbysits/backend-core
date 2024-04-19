using Lamar;
using Lens.Core.App;
using Lens.Core.Lib.Builders;
using Lens.Core.Lib.Models;
using Lens.Core.Lib.Services;
using Lens.Services.Masterdata.EF;
using Lens.Services.Masterdata.EF.Repositories;
using Lens.Services.Masterdata.Models;
using Lens.Services.Masterdata.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Text.Json;

namespace Lens.Services.Masterdata.IntegrationTests;

public class MasterdataServiceFixture : IDisposable
{
    private bool _init = false;
    public Container Services { get; private set; }
    public IMasterdataService MasterdataService { get; private set; }
    public MasterdataTypeModel? MasterdataType { get; private set; }
    public ResultPagedListModel<MasterdataModel>? Masterdatas { get; private set; }

    public MasterdataServiceFixture()
    {
        Services = new Container(registry =>
        {
            var Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .AddUserSecrets(GetType().Assembly)
                .Build();

            var appBuilder = new ApplicationSetupBuilder(registry, Configuration);
            appBuilder
                .AddAssemblies(typeof(MasterdataRepository).Assembly)
                .AddApplicationServices()
                .AddMasterdata()
                .Services
                    .AddScoped(typeof(IUserContext), services => new Mock<IUserContext>().Object);

            //appBuilder.Services.AddScoped<HtmlSanitizer>();
        });

        var iHave = Services.WhatDoIHave();

        MasterdataService = Services.GetRequiredService<IMasterdataService>();

    }

    public async Task Init()
    {
        if (_init) return;

        _init = true;
        var random4Characters = Random.Shared.Next(1111, 9999).ToString();
        var metaData = new
        {
            domain = new
            {
                someKey = "someValue",
                anotherKey = "anotherValue"
            },
            randomValue = random4Characters
        };
        
        var metaDataJsonElement = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(metaData));
        var masterdataCreate = new MasterdataImportModel
        {
            Code = random4Characters,
            Name = random4Characters,
            Description = random4Characters,
            Metadata = metaDataJsonElement,
            Masterdatas = new[] {
                new MasterdataCreateModel 
                { 
                    Key = random4Characters, 
                    Name = random4Characters,
                    Value = random4Characters,
                    Description = random4Characters,
                    Metadata = metaDataJsonElement,
                    Tags= new[] { "Red", "Green", "Blue", random4Characters }
                }
            }
        };

        MasterdataType = await MasterdataService.ImportMasterdata(masterdataCreate);
        Masterdatas = await MasterdataService.GetMasterdata(MasterdataType?.Code ?? string.Empty, MasterdataQueryModel.Default);
    }

    public void Dispose()
    {
        MasterdataService.DeleteMasterdataType(MasterdataType?.Code ?? string.Empty).GetAwaiter().GetResult();
        Services.Dispose();
    }
}
