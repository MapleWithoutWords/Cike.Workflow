using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Cike.Workflow.Test.Service.Open
{
    internal class ExampleTest:BaseIntegrationTest
    {
        [Test]
        public async Task Add_Name为空_返回400()
        {
            var wsId = "";
            var r = await PostAddAsync(new { workspaceId = wsId, parentId = 0, name = "" });

            Assert.That(r.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        private Task<HttpResponseMessage> PostAddAsync(object dto) =>
            CreateClient().PostAsJsonAsync("/api/v1/Folders", dto);
    }
}
