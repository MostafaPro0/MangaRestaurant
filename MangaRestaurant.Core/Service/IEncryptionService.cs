namespace MangaRestaurant.Core.Service
{
    public interface IEncryptionService
    {
        string GetPublicKey();
        string Decrypt(string encryptedText);
    }
}
