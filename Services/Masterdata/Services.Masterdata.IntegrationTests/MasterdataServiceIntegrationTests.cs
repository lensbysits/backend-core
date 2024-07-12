using FluentAssertions;
using Lens.Core.Lib.Models;
using Lens.Services.Masterdata.IntegrationTests;


namespace Services.Masterdata.IntegrationTests
{
    public class MasterdataServiceIntegrationTests : IClassFixture<MasterdataServiceFixture>
    {
        private readonly MasterdataServiceFixture _fixture;

        public MasterdataServiceIntegrationTests(MasterdataServiceFixture masterdataServiceFixture)
        {
            _fixture = masterdataServiceFixture;
        }

        [Fact]
        public async Task EnsureTagsAreProperlyFetched()
        {
            await _fixture.Init();
            var result = await _fixture.MasterdataService.GetMasterdata(_fixture.MasterdataType!.Code!, _fixture.Masterdatas!.Value!.First().Key!);
            result?.Tags.Should().NotBeEmpty();
        }

        [Fact]
        public async Task FetchAllUsedTagsByMasterdataType()
        {
            await _fixture.Init();
            var result = await _fixture.MasterdataService.GetTags(_fixture.MasterdataType!.Code!, QueryModel.Default);
            var expectedTags = _fixture.Masterdatas!.Value!.First().Tags?.Order();
            result.Value.Should().Equal(expectedTags);
        }
    }
}