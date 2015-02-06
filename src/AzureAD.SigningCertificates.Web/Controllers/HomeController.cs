using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Metadata;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
using System.Web.Mvc;
using System.Xml;

namespace AzureAD.SigningCertificates.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View(GetSigningCertificates(ConfigurationManager.AppSettings["WsFederationMetadataUrl"]));
        }


        private static List<X509SecurityToken> GetSigningCertificates(string metadataAddress)
        {
            var tokens = new List<X509SecurityToken>();

            using (var metadataReader = XmlReader.Create(metadataAddress))
            {
                var serializer = new MetadataSerializer { CertificateValidationMode = X509CertificateValidationMode.None };

                var metadata = serializer.ReadMetadata(metadataReader) as EntityDescriptor;
                if (metadata != null)
                {
                    var descriptior = metadata.RoleDescriptors.OfType<SecurityTokenServiceDescriptor>().First();
                    if (descriptior != null)
                    {
                        var keys = descriptior.Keys
                            .Where(key => key.KeyInfo != null && (key.Use == KeyType.Signing || key.Use == KeyType.Unspecified))
                            .Select(key => key.KeyInfo.OfType<X509RawDataKeyIdentifierClause>().First());
                        tokens.AddRange(keys.Select(token => new X509SecurityToken(new X509Certificate2(token.GetX509RawData()))));
                    }
                }
            }

            return tokens;
        }
    }
}