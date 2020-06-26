namespace Esprima.Utils
{
    public abstract class JsonWriter
    {
        public abstract void Null();
        public abstract void Number(long n);
        public abstract void Number(double n);
        public abstract void String(string? value);
        public abstract void Boolean(bool flag);
        public abstract void StartArray();
        public abstract void EndArray();
        public abstract void StartObject();
        public abstract void Member(string name);
        public abstract void EndObject();
    }
}