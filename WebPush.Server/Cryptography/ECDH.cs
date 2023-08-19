namespace WebPush.Server.Cryptography;
internal class ECDH
{
    internal static byte[] EncryptWithAes128(byte[] nonce, byte[] key, byte[] message) 
    {
        GcmBlockCipher cipher = new GcmBlockCipher(new AesEngine());
        AeadParameters parameters = new AeadParameters(new KeyParameter(key), 128, nonce);
        cipher.Init(true, parameters);

        // generate ciphern text with auth tag
        byte[] cipherText = new byte[cipher.GetOutputSize(message.Length)];
        int len = cipher.ProcessBytes(message, 0, message.Length, cipherText, 0);
        cipher.DoFinal(cipherText, len);
        return cipherText;
    }

    internal static AsymmetricCipherKeyPair GenerateECDHKeys()
    {
        X9ECParameters ecParameters = NistNamedCurves.GetByName("P-256");
        ECDomainParameters ecSpec = new ECDomainParameters(ecParameters.Curve, ecParameters.G, ecParameters.N, ecParameters.H, ecParameters.GetSeed());
        IAsymmetricCipherKeyPairGenerator keyPairGenerator = GeneratorUtilities.GetKeyPairGenerator("ECDH");
        keyPairGenerator.Init(new ECKeyGenerationParameters(ecSpec, new SecureRandom()));
        return keyPairGenerator.GenerateKeyPair();
    }

    internal static byte[] GetPublicKeyBytes(AsymmetricKeyParameter publicKey) => ((ECPublicKeyParameters)publicKey).Q.GetEncoded();

    internal static byte[] GetSharedSecret(AsymmetricKeyParameter privateKey, byte[] publicKey)
    {
        // crea la llave publica remota a partir de los bytes recibidos
        ECPublicKeyParameters ECPublicKeyParameters = ECDH.GetECPublicKeyParameters(publicKey);
        // obtener la clave secreta de ECDH
        IBasicAgreement agreement = ECDH.GetBasicECDHAgreement();
        agreement.Init(privateKey);

        BigInteger sharedSecret = agreement.CalculateAgreement(ECPublicKeyParameters);
        return sharedSecret.ToByteArrayUnsigned();
    }

    private static IBasicAgreement GetBasicECDHAgreement() => AgreementUtilities.GetBasicAgreement("ECDH");

    private static ECPublicKeyParameters GetECPublicKeyParameters(byte[] publicKey)
    {
        string pemKey = GetPublicPemKey(publicKey);

        StringReader reader = new StringReader(pemKey);
        PemReader pemReader = new PemReader(reader);
        var keyPair = pemReader.ReadObject();
        return (ECPublicKeyParameters)keyPair;
    }

    private static string GetPublicPemKey(byte[] publicKey)
    {
        const string ECPUBLICKEY_IDENTIFIER = "1.2.840.10045.2.1";
        const string ECDSA_P256_IDENTIFIER = "1.2.840.10045.3.1.7";
        const string PUBLIC_PEM_KEY_PREFIX = "-----BEGIN PUBLIC KEY-----";
        const string PUBLIC_PEM_KEY_SUFIX = "-----END PUBLIC KEY-----";

        Asn1Object keyTypeParameter = new DerSequence(new DerObjectIdentifier(ECPUBLICKEY_IDENTIFIER), new DerObjectIdentifier(ECDSA_P256_IDENTIFIER));
        Asn1Object derEncodedKey = new DerBitString(publicKey);
        Asn1Object derSequence = new DerSequence(keyTypeParameter, derEncodedKey);

        string base64EncodedDerSequence = Convert.ToBase64String(derSequence.GetEncoded());
        string pemKey = $"{PUBLIC_PEM_KEY_PREFIX}\n{base64EncodedDerSequence}\n{PUBLIC_PEM_KEY_SUFIX}";
        return pemKey;
    }

    internal static byte[] Hmac256ComputeHash(byte[] value, byte[] key) 
    {
        HMac hmac = new HMac(new Sha256Digest());
        hmac.Init(new KeyParameter(key));

        byte[] result = new byte[hmac.GetMacSize()];
        hmac.BlockUpdate(value, 0, value.Length);
        hmac.DoFinal(result, 0);
        return result;
    }
}
