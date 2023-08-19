namespace WebPush.Server.Cryptography;
internal class ECDSA
{
    public static byte[] Sign(string valueToSign, byte[] privateKeyBytes)
    {
        ECParameters ecParameters = new ECParameters()
        {
            D = privateKeyBytes,
            Curve = ECCurve.NamedCurves.nistP256
        };

        using ECDsa ecdsa = ECDsa.Create(ecParameters);

        byte[] unsignetTokenBytes = Encoding.UTF8.GetBytes(valueToSign);
        byte[] signature = ecdsa.SignData(unsignetTokenBytes, HashAlgorithmName.SHA256);

        return signature;
    }
}
