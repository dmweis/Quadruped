using System;

namespace Quadruped.WebInterface.VideoStreaming
{
    public class StreamerConfig : IEquatable<StreamerConfig>
    {
        public int ImageQuality { get; set; } = 85;
        public int HorizontalResolution { get; set; } = 800;
        public int VerticalResolution { get; set; } = 600;
        public int Framerate { get; set; } = 10;

        public bool Equals(StreamerConfig other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ImageQuality == other.ImageQuality && HorizontalResolution == other.HorizontalResolution && VerticalResolution == other.VerticalResolution && Framerate == other.Framerate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StreamerConfig) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ImageQuality;
                hashCode = (hashCode * 397) ^ HorizontalResolution;
                hashCode = (hashCode * 397) ^ VerticalResolution;
                hashCode = (hashCode * 397) ^ Framerate;
                return hashCode;
            }
        }

        public static bool operator ==(StreamerConfig a, StreamerConfig b)
        {
            if (ReferenceEquals(a, null))
            {
                if (ReferenceEquals(b, null))
                {
                    return true;
                }
                return false;
            }
            return a.Equals(b);
        }

        public static bool operator !=(StreamerConfig a, StreamerConfig b)
        {
            return !(a == b);
        }
    }
}
