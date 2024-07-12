using Lens.Services.Masterdata.Models;
using System.Text.Json;

namespace Lens.Services.Masterdata.UnitTests
{
    public class MasterdataServiceUnitTests : IClassFixture<MasterdataServiceFixture>
    {
        private readonly MasterdataServiceFixture _fixture;

        public MasterdataServiceUnitTests(MasterdataServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData("123", "Name", "Description", "{ \"someKey\": \"some value\" }", "123", "Name", "Description", "{ \"someKey\": \"some value\" }")]
        [InlineData("123", "Name", "This is bad: <script>alert('xss')</script>, should be sanitized", "{ \"someKey\": \"some value\" }", "123", "Name", "This is bad: , should be sanitized", "{ \"someKey\": \"some value\" }")]
        [InlineData("123", "<script>alert('xss')</script>Name", "Description", "{ \"someKey\": \"some value\" }", "123", "Name", "Description", "{ \"someKey\": \"some value\" }")]
        [InlineData("123", "Name", "Description", "{ \"someKey\": \"<script>alert('xss')</script>test<i>italic</i>\" }", "123", "Name", "Description", "{ \"someKey\": \"test\" }")]
        public async Task SanitizeMasterdataTypeCreateModelTest(string code, string name, string description, string metadata, 
                                                                string expectedCode, string expectedName, string expectedDescription, string expectedMetadata)
        {
            var metaDataNode = JsonSerializer.Deserialize<dynamic>(metadata);

            var model = new MasterdataTypeCreateModel
            {
                Code = code,
                Name = name,
                Description = description,
                Metadata = metaDataNode
            };

            _fixture.MasterdataRepositoryMock.Setup(repository => repository.AddMasterdataType(It.IsAny<MasterdataTypeCreateModel>()))
                .Callback((MasterdataTypeCreateModel callbackModel) =>
                {
                    callbackModel.Code.Should().Be(expectedCode);
                    callbackModel.Name.Should().Be(expectedName);
                    callbackModel.Description.Should().Be(expectedDescription);
                    callbackModel.Metadata?.ToString().Should().Be(expectedMetadata);
                });

            var result = await _fixture.MasterdataService.AddMasterdataType(model);
            Assert.True(true);
        }

        [Theory]
        [InlineData("test-key", "test-value", "Name", "Description", "{ \"someKey\": \"some value\" }", new string[0], "test-key", "test-value", "Name", "Description", "{ \"someKey\": \"some value\" }", new string[0])]
        [InlineData("test-key", "test-value", "Name", "Description", "{}", new string[] { "test1a", "test2a", "test3a" }, "test-key", "test-value", "Name", "Description", "{}", new string[] { "test3a", "test2a", "test1a" })]
        [InlineData("test-key", "test-value", "Name", "Description", "{}", new string[] { "color <script>alert('xss')</script>red", "very <strong>code</strong>height", "top product<script>alert('xss')</script>", "premium <script>alert('xss')</script>class <i>italic text</i>product" }, "test-key", "test-value", "Name", "Description", "{}", new string[] { "color red", "very height", "top product", "premium class product" })]
        public async Task SanitizeMasterdataCreateModelTest(string key, string value, string name, string description, string metadata, string[] tags,
                                                                string expectedKey, string expectedValue, string expectedName, string expectedDescription, string expectedMetadata, string[] expectedTags)
        {
            var masterdataType = "testing-code";
            var metaDataNode = JsonSerializer.Deserialize<dynamic>(metadata);

            var model = new MasterdataCreateModel
            {
                Key = key,
                Value = value,
                Name = name,
                Description = description,
                Metadata = metaDataNode,
                Tags = tags
            };

            _fixture.MasterdataRepositoryMock.Setup((repository) => repository.AddMasterdata(masterdataType, It.IsAny<MasterdataCreateModel>()))
                .Callback<string, MasterdataCreateModel>((masterdataType, callbackModel) =>
                {
                    callbackModel.Key.Should().Be(expectedKey);
                    callbackModel.Value.Should().Be(expectedValue);
                    callbackModel.Name.Should().Be(expectedName);
                    callbackModel.Description.Should().Be(expectedDescription);
                    callbackModel.Metadata?.ToString().Should().Be(expectedMetadata);

                    var inputTags = callbackModel.Tags ?? Array.Empty<string>();
                    Array.Sort(inputTags);
                    var outputTags = expectedTags;
                    Array.Sort(outputTags);
                    String.Join("", inputTags).Should().Be(String.Join("", outputTags));
                });

            var result = await _fixture.MasterdataService.AddMasterdata(masterdataType, model);
            Assert.True(true);
        }
    }
}