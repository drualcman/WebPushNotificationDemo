namespace WebPush.Server.Models;
internal class WebPushBodyBuilderResult
{
    public byte[] PublicKey { get; set; }
    /// <summary>
    /// Encripted message
    /// </summary>
    public byte[] Payload { get; set; }     
    public byte[] Salt { get; set; }     


}
