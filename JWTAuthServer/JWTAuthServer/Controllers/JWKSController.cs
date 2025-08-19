using JWTAuthServer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace JWTAuthServer.Controllers
{
    [Route(".well-known")]
    [ApiController]
    public class JWKSController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JWKSController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("jwks.json")]
        public IActionResult GetJWKS()
        {
            var keys = _context.SigningKeys.Where(k => k.IsActive).ToList();
            var jwks = new
            {
                keys = keys.Select(k => new 
                {
                    kty = "RSA", // key type
                    use = "sig", // usage for signature
                    kid = k.KeyId, // key id to identify the key
                    alg = "RS256", // algorithm RSA SHA-256
                    n = Base64UrlEncoder.Encode(GetModulus(k.PublicKey)), // modulus
                    e = Base64UrlEncoder.Encode(GetExponent(k.PublicKey)) // exponent
                })
            };
            return Ok(jwks);
        }

        private byte[] GetModulus(string publicKey)
        {
            var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
            var parameters = rsa.ExportParameters(false);
            rsa.Dispose();

            if (parameters.Modulus == null)
            {
                throw new InvalidOperationException("RSA parameters are not valid.");
            }

            return parameters.Modulus;
        }

        private byte[] GetExponent(string publicKey) 
        { 
            var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
            var parameters = rsa.ExportParameters(false);
            rsa.Dispose();

            if (parameters.Exponent == null)
            {
                throw new InvalidOperationException("RSA parameters are not valid.");
            }

            return parameters.Exponent;
        }
    }
}
