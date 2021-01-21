using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WebAppKeyVault.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            try
            {
                string uri = Environment.GetEnvironmentVariable("KEY_VAULT_URI");
                SecretClient client = new SecretClient(new Uri(uri), new DefaultAzureCredential());

                var secret = (await client.GetSecretAsync("secret")).Value;

                ViewBag.Secret = $"Secret: {secret.Value}";
            }
            catch (Exception exp)
            {
                ViewBag.Error = $"Something went wrong: {exp.Message}";
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
