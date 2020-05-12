using System;
using System.Web.Mvc;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;

namespace WebAppKeyVault.Controllers
{
    public class HomeController : Controller
    {
        public async System.Threading.Tasks.Task<ActionResult> Index()
        {
            try
            {
                string uri = Environment.GetEnvironmentVariable("KEY_VAULT_URI");
                var client = new SecretClient(new Uri(uri), new DefaultAzureCredential());

                Azure.Response<KeyVaultSecret> secret = await client.GetSecretAsync("secret");

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
