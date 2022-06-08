using BlazorApp1.Data;
using BlazorApp1.Pages;
using BlazorApp1.Shared;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestProject1
{

    public class BlazorTest : TestContext
    {
        [Fact]
        public void IndexTest()
        {
            var cut = RenderComponent<BlazorApp1.Pages.Index>(parameters => parameters
            .Add(p => p.Name, "Mike") //字符串参数
            .Add(p => p.Hobbies, new List<string>() { "eat", "sleep" }) //List参数
            .Add(p => p.ShowName, e => { System.Diagnostics.Trace.WriteLine(e); }) //方法参数（回调函数）
            .AddChildContent("<h1>Hello World</h1>") //子内容参数，组件必须有个ChildContent参数
            .Add(p => p.Content, "I love u") //将字符串传给RenderFragment参数
            .Add<BlazorApp1.Shared.SurveyPrompt>(p => p.Content) //将组件传给RenderFragment参数
            .Add<BlazorApp1.Shared.SurveyPrompt>(p => p.Content, surveyParameters => //将带有参数的组件，传给RenderFragment参数
            {
                surveyParameters.Add(sp => sp.Title, "this is a test parameter");
            })
            );

            System.Diagnostics.Trace.WriteLine(cut.Markup);
        }

        [Fact]
        public void TempalteParamTest()
        {
            var cut = RenderComponent<TempalteParam<string>>(parameters => parameters
            .Add(p => p.Items, new[] { "cSharp", "golang" })
            .Add(p => p.Template, e => { return $"<span>{e}</span>"; }) // 基于HTML的模板
            .Add<BlazorApp1.Shared.SurveyPrompt, string>(p => p.Template, e => eParameters => eParameters.Add(ep => ep.Title, e)) // 基于组件的模板
            );

            System.Diagnostics.Trace.WriteLine(cut.Markup);
        }


        [Fact]
        public void CascadingParamTest()
        {
            var cut = RenderComponent<CascadingParam>(parameters => parameters
            .Add(p => p.Name, "Jane")
            .Add(p => p.Age, 5));

            System.Diagnostics.Trace.WriteLine(cut.Markup);
        }

        [Fact]
        public void IOCTest()
        {
            Services.AddSingleton<WeatherForecastService>();
            var cut = RenderComponent<FetchData>();

            System.Diagnostics.Trace.WriteLine(cut.Markup);
        }

        [Fact]
        public void RenderTreeTest()
        {
            RenderTree.Add<CascadingValue<string>>(parameters => parameters
            .Add(p => p.Value, "jane"));

            RenderTree.Add<CascadingValue<int>>(parameters => parameters
            .Add(p => p.Name, "UserAge")
            .Add(p => p.Value, 8));

            var cut = RenderComponent<CascadingParam>();
            System.Diagnostics.Trace.WriteLine(cut.Markup);
        }


        [Fact]
        public void StubTest()
        {
            ComponentFactories.AddStub<SurveyPrompt>(parameters => $"<div>{parameters.Get(x => x.Title)} this is the moke content</div>");
            var cut = RenderComponent<StubComponent>();
            System.Diagnostics.Trace.WriteLine(cut.Markup);
        }


        [Fact]
        public void EventTest()
        {
            var cut = RenderComponent<Counter>();
            var btn = cut.Find("#btn");
            btn.Click(); // 通过辅助方法trigger事件

            btn.TriggerEvent("onclick", new MouseEventArgs()); // 通过TriggerEvent trigger事件
            System.Diagnostics.Trace.WriteLine(cut.Markup);
        }

        [Fact]
        public void RenderTest()
        {
            var cut = RenderComponent<RenderTestComponent>();
            System.Diagnostics.Trace.WriteLine("RenderCount is " + cut.RenderCount);

            cut.Render();
            System.Diagnostics.Trace.WriteLine("RenderCount is " + cut.RenderCount);

            cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Name, "jane"));
            System.Diagnostics.Trace.WriteLine("RenderCount is " + cut.RenderCount);
        }


        [Fact]
        public void StateChangeTest()
        {
            var cut = RenderComponent<StateChangeComponent>();

            cut.InvokeAsync(() => cut.Instance.Calculate(1, 2));

            cut.MarkupMatches("<output>3</output>");
        }

        [Fact]
        public void WaitForStateTest()
        {
            var cut = RenderComponent<WaitForStateComponent>();
            //Assert.Contains("Jane", cut.Markup);    //不等待直接断言，不通过
            cut.WaitForState(() => cut.Markup.Contains("Jane"), TimeSpan.FromSeconds(6));
            Assert.Contains("Jane", cut.Markup);
        }

        [Fact]
        public void BasicVerifyTest()
        {
            var cut = RenderComponent<BasicVerify>();
            //Assert.Equal("<h3>hello</h3>", cut.Markup); // 由于缺少前后空格缩进，测试失败
            Assert.Equal("<h3> hello </h3>", cut.Markup);
        }

        [Fact]
        public void SemanticVerifyTest()
        {
            var cut = RenderComponent<BasicVerify>();
            cut.MarkupMatches("<h3>hello</h3>", cut.Markup); // 缺少前后空格缩进，测试成功
            cut.Find("h3").MarkupMatches("<h3>hello</h3>");  // 支持INode类型
            cut.Find("h3").TextContent.MarkupMatches(" hello"); //TextContent会去除首尾字符串
        }


        [Fact]
        public void FindFindAllTest()
        {
            Services.AddSingleton<WeatherForecastService>();
            var cut = RenderComponent<FetchData>();
            var tr = cut.Find("tr");
            var trs = cut.FindAll("tr");

            Assert.Equal(tr.TextContent, trs[0].TextContent);
        }

        [Fact]
        public void DiffTest()
        {
            var cut = RenderComponent<Counter>();

            // Act - increment the counter
            cut.Find("button").Click();

            // Assert - find differences between first render and click
            var diffs = cut.GetChangesSinceFirstRender();

            // Only expect there to be one change      
            var diff = diffs.ShouldHaveSingleChange();
            // and that change should be a text 
            // change to "Current count: 1"
            diff.ShouldBeTextChange("Current count: 1");
        }

        [Fact]
        public void InstanceTest()
        {
            var cut = RenderComponent<StubComponent>();
            var component = cut.Instance;
        }

        [Fact]
        public void FindComponentTest()
        {
            var cut = RenderComponent<StubComponent>();
            var promptCut = cut.FindComponent<SurveyPrompt>();
            Assert.Equal("stub test", promptCut.Instance.Title);
        }

        [Fact]
        public void JSTest()
        {
            JSInterop.Setup<string>("getUserName").SetResult("Mike");
            var cut = RenderComponent<JSComponent>();
            var button = cut.Find("button");
            button.Click();

            var diffs = cut.GetChangesSinceFirstRender();     
            var diff = diffs.ShouldHaveSingleChange();
            diff.ShouldBeTextChange("name is Mike");
        }

        [Fact]
        public void JSImportTest()
        {
            var jsModule = JSInterop.SetupModule("myjs.js");
            jsModule.Setup<string>("getUserName").SetResult("Mike");
            var cut = RenderComponent<JSImportComponent>();
            var button = cut.Find("button");
            button.Click();

            var diffs = cut.GetChangesSinceFirstRender();
            var diff = diffs.ShouldHaveSingleChange();
            diff.ShouldBeTextChange("name is Mike");
        }

        //未认证
        [Fact]
        public void UnAuthorizeTest()
        {
            var authContext = this.AddTestAuthorization();
            var cut = RenderComponent<AuthorizeComponent>();
            cut.MarkupMatches(@"<p>State: Not authorized</p>");
        }

        //认证中
        [Fact]
        public void AuthorizingTest()
        {
            var authContext = this.AddTestAuthorization();
            authContext.SetAuthorizing();
            var cut = RenderComponent<AuthorizeComponent>();
            cut.MarkupMatches(@"<p>State: Authorizing</p>");
        }

        //认证，未授权
        [Fact]
        public void AuthenticateAndUnAuthorizedTest()
        {
            var authContext = this.AddTestAuthorization();
            authContext.SetAuthorized("TEST USER", AuthorizationState.Unauthorized);
            var cut = RenderComponent<AuthorizeComponent>();
            cut.MarkupMatches(@"<h1>Welcome TEST USER</h1>
                    <p>State: Not authorized</p>");
        }

        //认证+授权
        [Fact]
        public void AuthenticateAndAuthorizedTest()
        {
            var authContext = this.AddTestAuthorization();
            authContext.SetAuthorized("TEST USER");
            /*
             authContext.SetRoles("admin", "superuser");
                authContext.SetPolicies("content-editor");
                authContext.SetClaims(new Claim(ClaimTypes.Email, "test@example.com"));
             */
            var cut = RenderComponent<AuthorizeComponent>();
            cut.MarkupMatches(@"<h1>Welcome TEST USER</h1>
                    <p>State: Authorized</p>");
        }

        [Fact]
        public void NavigateTest()
        {
            var navMan = Services.GetRequiredService<FakeNavigationManager>();
            var cut = RenderComponent<PrintCurrentUrl>();
            navMan.NavigateTo("newUrl");
            cut.Find("p").MarkupMatches($"<p>{navMan.BaseUri}newUrl</p>");
        }
    }
}
