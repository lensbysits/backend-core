using Lens.Core.Lib.Services;
using Lens.Services.Masterdata.Repositories;
using Lens.Services.Masterdata.Services;

namespace Lens.Services.Masterdata.UnitTests;

public class MasterdataServiceFixture : BaseFixture
{
    public MasterdataService MasterdataService { get; private set; } = null!;
    public Mock<IMasterdataRepository> MasterdataRepositoryMock { get; private set; } = null!;

    protected override void OnSetupReady()
    {
        var applicationService = Mock.Of<IApplicationService<MasterdataService>>();
        MasterdataRepositoryMock = new Mock<IMasterdataRepository>();
        MasterdataService = new MasterdataService(applicationService, MasterdataRepositoryMock.Object, new Ganss.Xss.HtmlSanitizer());
    }
}

