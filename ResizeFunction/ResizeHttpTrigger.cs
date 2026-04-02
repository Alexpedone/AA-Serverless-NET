using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Net;

namespace Company.Function
{
    public class ResizeHttpTrigger
    {
        private readonly ILogger _logger;

        public ResizeHttpTrigger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ResizeHttpTrigger>();
        }

        [Function("ResizeHttpTrigger")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            if (!int.TryParse(req.Query["w"], out int w) || w <= 0)
            {
                var badReq = req.CreateResponse(HttpStatusCode.BadRequest);
                await badReq.WriteStringAsync("Paramètre 'w' invalide ou manquant.");
                return badReq;
            }

            if (!int.TryParse(req.Query["h"], out int h) || h <= 0)
            {
                var badReq = req.CreateResponse(HttpStatusCode.BadRequest);
                await badReq.WriteStringAsync("Paramètre 'h' invalide ou manquant.");
                return badReq;
            }

            try
            {
                using var msInput = new MemoryStream();
                await req.Body.CopyToAsync(msInput);
                msInput.Position = 0;

                using var image = Image.Load(msInput);
                image.Mutate(x => x.Resize(w, h));

                using var msOutput = new MemoryStream();
                image.SaveAsJpeg(msOutput);

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "image/jpeg");
                await response.WriteBytesAsync(msOutput.ToArray());
                return response;
            }
            catch (UnknownImageFormatException)
            {
                var badReq = req.CreateResponse(HttpStatusCode.BadRequest);
                await badReq.WriteStringAsync("Format d'image non reconnu.");
                return badReq;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}