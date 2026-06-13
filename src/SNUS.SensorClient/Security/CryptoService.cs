using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
namespace SNUS.SensorClient.Security
{
    public class CryptoService
    {
        private readonly ECDsa _ecdsa;

        private static readonly byte[] AesKey = SHA256.HashData(Encoding.UTF8.GetBytes("SuperTajniKljuZaSNUS2026"));
        private static readonly byte[] AesIv = new byte[16] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

        public CryptoService()
        {
            _ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        }

        public string GetPublicKeyBase64()
        {
            return Convert.ToBase64String(_ecdsa.ExportSubjectPublicKeyInfo());
        }

        public string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = AesKey;
                aes.IV = AesIv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public string SignData(string dataToSign)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(dataToSign);
            byte[] signatureBytes = _ecdsa.SignData(dataBytes, HashAlgorithmName.SHA256);
            return Convert.ToBase64String(signatureBytes);
        }
    }
}