using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamixelServo.Quadruped.WebInterface.VideoStreaming
{
    public class MockVideoStream : IVideoService
    {
        public string StreamPath =>
            @"https://upload.wikimedia.org/wikipedia/en/thumb/4/47/Spongebob-squarepants.svg/1200px-Spongebob-squarepants.svg.png";
        public bool StreamRunning => true;
    }
}
