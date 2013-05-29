namespace com.rogusdev.UbJsonSharp
{
    public interface IUbJsonDeserializer
    {
        object Deserialize (byte[] bytes);
        object Deserialize (byte[] bytes, ref int pos);
    }
}