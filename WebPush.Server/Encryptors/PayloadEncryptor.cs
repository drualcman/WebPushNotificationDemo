using WebPush.Server.Extensions;

namespace WebPush.Server.Encryptors;
internal class PayloadEncryptor
{
    public static WebPushBodyBuilderResult Encrypt(WebPushsubscrition subscription, string payload)
    {
        byte[] p256DHBytes = subscription.P256dh.ToBytesFromBase64();
        byte[] authBytes = subscription.Auth.ToBytesFromBase64();
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        // obtener el Salt
        byte[] salt = GenerateSalt(16);

        // generar llaves ECDH publica y privada
        AsymmetricCipherKeyPair localKeysCurve = ECDH.GenerateECDHKeys();

        // crear el secreto
        byte[] sharedSecret = ECDH.GetSharedSecret(localKeysCurve.Private, p256DHBytes);

        byte[] serverPublicKey = ECDH.GetPublicKeyBytes(localKeysCurve.Public);

        byte[] pseudoRandomKey = GetHKDF(authBytes, sharedSecret, Encoding.UTF8.GetBytes("Content-Encoding: auth\0"), 32);

        byte[] context = GetContext(p256DHBytes, serverPublicKey);

        byte[] nonceInfo = CreateContentEncoding("nonce", context);
        byte[] contentEncriptionKeyInfo = CreateContentEncoding("aesgcm", context);
        byte[] nonce = GetHKDF(salt, pseudoRandomKey, nonceInfo, 12);
        byte[] contentEncryptionKey = GetHKDF(salt, pseudoRandomKey, contentEncriptionKeyInfo, 16);
        byte[] input = AddPaddingToInput(payloadBytes);

        byte[] encryptedPayload = ECDH.EncryptWithAes128(nonce, contentEncryptionKey, input);

        return new WebPushBodyBuilderResult
        {
            Salt = salt,
            Payload = encryptedPayload,
            PublicKey = serverPublicKey
        };
    }

    private static byte[] AddPaddingToInput(byte[] data)
    {
        byte[] input = new byte[2 + data.Length];
        Buffer.BlockCopy(data, 0, input, 2, data.Length);
        return input;
    }

    private static byte[] CreateContentEncoding(string contectEncoding, byte[] context)
    {
        List<byte> result = new List<byte>();
        result.AddRange(Encoding.UTF8.GetBytes($"Content-Encoding: {contectEncoding}\0"));
        result.AddRange(context);
        return result.ToArray();
    }

    private static byte[] ConvertInt(int number)
    {
        byte[] output = BitConverter.GetBytes(Convert.ToUInt16(number));
        if(BitConverter.IsLittleEndian)
        {
            Array.Reverse(output);
        }
        return output;
    }

    private static byte[] GetContext(byte[] p256DHBytes, byte[] localPublicKey)
    {
        List<byte> context = new List<byte>();
        context.AddRange(Encoding.UTF8.GetBytes("P-256\0"));
        context.AddRange(ConvertInt(p256DHBytes.Length));
        context.AddRange(p256DHBytes);
        context.AddRange(ConvertInt(localPublicKey.Length));
        context.AddRange(localPublicKey);
        return context.ToArray();
    }

    private static byte[] GenerateSalt(int lenght)
    {
        byte[] salt = new byte[lenght];
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetNonZeroBytes(salt);
        return salt;
    }

    private static byte[] GetHKDF(byte[] salt, byte[] prk, byte[] info, int lenght)
    {
        //HKDF.DeriveKey(HashAlgorithmName.SHA256, prk, lenght, salt, info);

        //extract
        byte[] key = ECDH.Hmac256ComputeHash(prk, salt);

        //expand
        byte[] infoAddOne = info.Concat(new byte[] { 0x01 }).ToArray();

        byte[] result = ECDH.Hmac256ComputeHash(infoAddOne, key);
        if(result.Length > lenght)
        {
            Array.Resize(ref result, lenght);
        }
        return result;
    }
}
