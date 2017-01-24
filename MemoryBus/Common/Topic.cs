namespace MemoryBus.Common
{
    using System;

    public class Topic
    {
        public Type Type { get; private set; }

        private Topic(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("Type argument cannot be null");
            Type = type;
        }

        protected bool Equals(Topic other) 
            => Type == other.Type;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((Topic)obj);
        }

        public override int GetHashCode()
            => Type.GetHashCode();

        public override string ToString()
            => Type?.ToString();

        public static Topic CreateTopic<TRequest, TResponse>()
            => new Topic(typeof(Tuple<TRequest, TResponse>));

        public static Topic CreateTopic<T>()
            => new Topic(typeof(T));
    }
}
