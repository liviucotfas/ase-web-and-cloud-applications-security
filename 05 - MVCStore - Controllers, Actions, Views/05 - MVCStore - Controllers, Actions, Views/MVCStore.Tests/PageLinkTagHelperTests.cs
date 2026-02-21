using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using MVCStore.Infrastructure;
using MVCStore.ViewModels;

namespace MVCStore.Tests
{
	public class PageLinkTagHelperTests
	{
		[Fact]
		public void Can_Generate_Page_Links()
		{
			// Arrange
			var urlHelper = new Mock<IUrlHelper>();
			urlHelper.SetupSequence(x => x.Action(It.IsAny<UrlActionContext>()))
				.Returns("Test/Page1")
				.Returns("Test/Page2")
				.Returns("Test/Page3");

			var urlHelperFactory = new Mock<IUrlHelperFactory>();
			urlHelperFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
				.Returns(urlHelper.Object);

			PageLinkTagHelper helper = new PageLinkTagHelper(urlHelperFactory.Object)
			{
				PageModel = new PagingInfoViewModel
				{
					CurrentPage = 2,
					TotalItems = 28,
					ItemsPerPage = 10
				},
				PageAction = "Test"
			};

			TagHelperContext ctx = new TagHelperContext(
				new TagHelperAttributeList(),
				new Dictionary<object, object>(), "");

			var content = new Mock<TagHelperContent>();
			TagHelperOutput output = new TagHelperOutput("div",
				new TagHelperAttributeList(),
				(cache, encoder) => Task.FromResult(content.Object));

			// Act
			helper.Process(ctx, output);

			// Assert
			string result = output.Content.GetContent();
			Assert.Contains("Test/Page1", result);
			Assert.Contains("Test/Page2", result);
			Assert.Contains("Test/Page3", result);
		}
	}
}
