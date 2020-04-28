using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using MuTest.Core.Common.Settings;
using MuTest.Core.Model.Service;
using MuTest.Service.Service;

namespace MuTest.Service.API
{
    public class MuTestController : ApiController
    {
        private static readonly MuTestSettings Settings = MuTestSettingsSection.GetSettings();

        public MuTestController()
        {
            Settings.ServiceAddress = MuTestService.BaseAddress;
        }

        [HttpGet]
        public JsonResult<string> GetStatus()
        {
            return Json("Service is running!");
        }

        [HttpPost]
        public async Task<JsonResult<BuildResult>> Build([FromBody]string options)
        {
            return Json(await new BuildService(Settings).Build(options));
        }

        [HttpPost]
        public async Task<JsonResult<TestResult>> Test([FromBody]TestInput input)
        {
            return Json(await new TestService(Settings)
            {
                KillProcessOnTestFail = input.KillProcessOnTestFail
            }.Test(input.Arguments));
        }
    }
}
