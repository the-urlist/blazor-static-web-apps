using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Net.Codecrete.QrCodeGenerator;
using System.Net;
using System.Threading.Tasks;

namespace Api.Functions
{
    public class GetQrCode()
    {
        [Function(nameof(GetQrCode))]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "qrcode/{vanityUrl}")] HttpRequestData req, string vanityUrl)
        {
            if (string.IsNullOrEmpty(vanityUrl))
            {
                return await req.CreateJsonResponse(HttpStatusCode.BadRequest, "vanityUrl is required");
            }

            string decodedVanityUrl = System.Web.HttpUtility.UrlDecode(vanityUrl);

            var qrCode = QrCode.EncodeText(decodedVanityUrl, QrCode.Ecc.Medium);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync(qrCode.ToSvgString(4, "#121212", "#F9FAFC"));

            return response;
        }
    }
}