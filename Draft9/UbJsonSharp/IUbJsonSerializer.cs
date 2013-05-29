namespace com.rogusdev.UbJsonSharp
{
    public interface IUbJsonSerializer
    {
        byte[] Serialize(object o);
        byte[] SerializeNoop ();
        byte[] SerializePrecision (string o);
    }
}